using System.Text.Json;
using JtechnApi.Exams.Models;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;

namespace JtechnApi.Exams.Repositories
{
    public sealed class ExamReportingRepository : IExamReportingRepository
    {
        private readonly DBJtecContext context;
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ExamReportingRepository(DBJtecContext context) => this.context = context;

        public async Task<ExamResultFilterOptions> ResultFilterOptions()
        {
            var departments = await context.Departments.AsNoTracking()
                .OrderBy(department => department.Name)
                .Select(department => new ExamResultDepartmentOption { Id = department.Id, Name = department.Name })
                .ToListAsync();
            var exams = await context.DepartmentExams.IgnoreQueryFilters().AsNoTracking()
                .OrderBy(exam => exam.Name)
                .Select(exam => new ExamResultExamOption
                {
                    Id = exam.Id,
                    DepartmentId = exam.DepartmentId,
                    Name = exam.Name
                })
                .ToListAsync();
            return new ExamResultFilterOptions { Departments = departments, Exams = exams };
        }

        public async Task<ExamResultPage> Results(int page, int pageSize, int? departmentId, int? examId, string search)
        {
            var results = FilteredResults(departmentId, examId, search);
            var totalItems = await results.CountAsync();
            var items = await results
                .OrderByDescending(item => item.SubmittedAt)
                .ThenByDescending(item => item.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ExamResultPage { Items = items, TotalItems = totalItems, Page = page, PageSize = pageSize };
        }

        public async Task<IReadOnlyList<ExamResultItem>> ExportResults(int? departmentId, int? examId, string search) =>
            await FilteredResults(departmentId, examId, search)
                .OrderByDescending(item => item.SubmittedAt)
                .ThenByDescending(item => item.Id)
                .ToListAsync();

        public async Task<ExamQuickStatistics> QuickStatistics(int? departmentId, int? examId)
        {
            var buckets = await BuildQuickStatistics(departmentId, examId);
            return new ExamQuickStatistics { Periods = buckets.Select(bucket => bucket.Statistics).ToList() };
        }

        public async Task<IReadOnlyList<ExamQuickStatisticsEmployee>> ExportQuickStatistics(
            int period, string category, int? departmentId, int? examId)
        {
            var bucket = (await BuildQuickStatistics(departmentId, examId))
                .FirstOrDefault(item => item.Statistics.Period == period);
            if (bucket == null) return Array.Empty<ExamQuickStatisticsEmployee>();

            return category.ToLowerInvariant() switch
            {
                "total" => bucket.All,
                "passed" => bucket.Passed,
                "failed" => bucket.Failed,
                "retake" => bucket.Retake,
                "retrain" => bucket.Retrain,
                "nottaken" => bucket.NotTaken,
                _ => throw new ArgumentOutOfRangeException(nameof(category), "Loại thống kê không hợp lệ.")
            };
        }

        private async Task<IReadOnlyList<QuickStatisticsBucket>> BuildQuickStatistics(int? departmentId, int? examId)
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonth = monthStart.AddMonths(1);
            var effectiveDepartmentId = departmentId;
            string selectedExamName = null;
            if (examId.HasValue)
            {
                var exam = await context.DepartmentExams.AsNoTracking()
                    .Where(item => item.Id == examId.Value)
                    .Select(item => new { item.DepartmentId, item.Name })
                    .FirstOrDefaultAsync();
                if (exam == null) return EmptyQuickStatistics(monthStart, nextMonth);
                if (departmentId.HasValue && departmentId.Value != exam.DepartmentId)
                    return EmptyQuickStatistics(monthStart, nextMonth);
                effectiveDepartmentId ??= exam.DepartmentId;
                selectedExamName = exam.Name;
            }

            var rosterRows = await (
                from assignment in context.EmployeeDepartments.AsNoTracking()
                join employee in context.Employee.AsNoTracking() on assignment.EmployeeId equals employee.Id
                join department in context.Departments.AsNoTracking() on assignment.DepartmentId equals department.Id
                where employee.IsActive &&
                    (!effectiveDepartmentId.HasValue || assignment.DepartmentId == effectiveDepartmentId.Value)
                orderby department.Name, employee.EmployeeCode
                select new QuickStatisticsRosterEmployee
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.FullName,
                    EmployeeCode = employee.EmployeeCode,
                    DepartmentId = department.Id,
                    DepartmentName = department.Name
                }).ToListAsync();
            var roster = rosterRows.GroupBy(row => row.EmployeeId)
                .Select(group => group.First())
                .ToList();

            var attempts = new List<QuickStatisticsAttempt>();
            if (roster.Count > 0)
            {
                var query = context.OnlineExamAttempts.AsNoTracking()
                    .Where(attempt => attempt.DeletedAt == null &&
                        attempt.SubmittedAt != null &&
                        attempt.CreateDate >= monthStart &&
                        attempt.CreateDate < nextMonth);
                if (departmentId.HasValue)
                    query = query.Where(attempt => attempt.DepartmentId == departmentId.Value);
                if (examId.HasValue)
                    query = query.Where(attempt => attempt.CatalogExamId == examId.Value);

                attempts = await (
                    from attempt in query
                    join exam in context.DepartmentExams.IgnoreQueryFilters().AsNoTracking()
                        on attempt.CatalogExamId equals (int?)exam.Id into examJoin
                    from exam in examJoin.DefaultIfEmpty()
                    join department in context.Departments.AsNoTracking()
                        on attempt.DepartmentId equals department.Id into departmentJoin
                    from department in departmentJoin.DefaultIfEmpty()
                    select new QuickStatisticsAttempt
                    {
                        Id = attempt.Id,
                        EmployeeId = attempt.EmployeeId,
                        EmployeeCode = attempt.Code,
                        EmployeeName = attempt.Name,
                        DepartmentId = attempt.DepartmentId,
                        DepartmentName = department == null ? string.Empty : department.Name,
                        ExamName = exam == null ? attempt.Type ?? "Bài thi" : exam.Name,
                        ExamDate = attempt.CreateDate,
                        CycleName = attempt.CycleName,
                        Mission = attempt.Mission,
                        CorrectAnswers = attempt.Results,
                        TotalQuestions = attempt.TotalQuestions,
                        Score = attempt.Scores,
                        Passed = attempt.Status,
                        CountingTime = attempt.CountingTime,
                        SubmittedAt = attempt.SubmittedAt,
                        AttemptJson = attempt.AttemptJson
                    }).ToListAsync();
            }

            var employeeIds = roster.Select(employee => employee.EmployeeId).ToHashSet();
            var employeesByCode = roster
                .Where(employee => !string.IsNullOrWhiteSpace(employee.EmployeeCode))
                .GroupBy(employee => employee.EmployeeCode, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First().EmployeeId, StringComparer.OrdinalIgnoreCase);
            var attemptsByEmployee = new Dictionary<int, List<QuickStatisticsAttempt>>();
            foreach (var attempt in attempts)
            {
                int employeeId;
                if (attempt.EmployeeId.HasValue && employeeIds.Contains(attempt.EmployeeId.Value))
                    employeeId = attempt.EmployeeId.Value;
                else if (attempt.EmployeeId == null &&
                    !string.IsNullOrWhiteSpace(attempt.EmployeeCode) &&
                    employeesByCode.TryGetValue(attempt.EmployeeCode, out var matchedEmployeeId))
                    employeeId = matchedEmployeeId;
                else
                    continue;

                if (!attemptsByEmployee.TryGetValue(employeeId, out var employeeAttempts))
                {
                    employeeAttempts = new List<QuickStatisticsAttempt>();
                    attemptsByEmployee.Add(employeeId, employeeAttempts);
                }
                employeeAttempts.Add(attempt);
            }

            var firstPeriodEnd = monthStart.AddDays(15);
            return new[]
            {
                BuildQuickStatisticsPeriod(1, monthStart, firstPeriodEnd, roster, attemptsByEmployee, selectedExamName),
                BuildQuickStatisticsPeriod(2, firstPeriodEnd, nextMonth, roster, attemptsByEmployee, selectedExamName)
            };
        }

        private static IReadOnlyList<QuickStatisticsBucket> EmptyQuickStatistics(DateTime monthStart, DateTime nextMonth) =>
            new[]
            {
                BuildQuickStatisticsPeriod(1, monthStart, monthStart.AddDays(15), new List<QuickStatisticsRosterEmployee>(),
                    new Dictionary<int, List<QuickStatisticsAttempt>>(), null),
                BuildQuickStatisticsPeriod(2, monthStart.AddDays(15), nextMonth, new List<QuickStatisticsRosterEmployee>(),
                    new Dictionary<int, List<QuickStatisticsAttempt>>(), null)
            };

        private static QuickStatisticsBucket BuildQuickStatisticsPeriod(
            int period,
            DateTime startDate,
            DateTime endDate,
            IReadOnlyList<QuickStatisticsRosterEmployee> roster,
            IReadOnlyDictionary<int, List<QuickStatisticsAttempt>> attemptsByEmployee,
            string selectedExamName)
        {
            var all = new List<ExamQuickStatisticsEmployee>();
            var passed = new List<ExamQuickStatisticsEmployee>();
            var failed = new List<ExamQuickStatisticsEmployee>();
            var retake = new List<ExamQuickStatisticsEmployee>();
            var retrain = new List<ExamQuickStatisticsEmployee>();
            var notTaken = new List<ExamQuickStatisticsEmployee>();

            foreach (var employee in roster)
            {
                QuickStatisticsAttempt latestAttempt = null;
                if (attemptsByEmployee.TryGetValue(employee.EmployeeId, out var employeeAttempts))
                    latestAttempt = employeeAttempts
                        .Where(attempt => attempt.ExamDate >= startDate && attempt.ExamDate < endDate)
                        .OrderByDescending(attempt => attempt.SubmittedAt)
                        .ThenByDescending(attempt => attempt.Id)
                        .FirstOrDefault();

                if (latestAttempt == null)
                {
                    var missing = new ExamQuickStatisticsEmployee
                    {
                        EmployeeName = employee.EmployeeName,
                        EmployeeCode = employee.EmployeeCode,
                        DepartmentName = employee.DepartmentName,
                        DepartmentId = employee.DepartmentId,
                        ExamName = selectedExamName ?? "Chưa thi",
                        ResultStatus = "Chưa thi"
                    };
                    all.Add(missing);
                    notTaken.Add(missing);
                    continue;
                }

                var result = new ExamQuickStatisticsEmployee
                {
                    EmployeeName = latestAttempt.EmployeeName ?? employee.EmployeeName,
                    EmployeeCode = latestAttempt.EmployeeCode ?? employee.EmployeeCode,
                    DepartmentName = latestAttempt.DepartmentName ?? employee.DepartmentName,
                    DepartmentId = latestAttempt.DepartmentId,
                    ExamName = latestAttempt.ExamName,
                    ExamDate = latestAttempt.ExamDate,
                    CycleName = latestAttempt.CycleName,
                    Mission = latestAttempt.Mission,
                    CorrectAnswers = latestAttempt.CorrectAnswers,
                    TotalQuestions = latestAttempt.TotalQuestions,
                    Score = latestAttempt.Score,
                    CountingTime = latestAttempt.CountingTime,
                    SubmittedAt = latestAttempt.SubmittedAt
                };
                var classification = GetQuickStatisticsClassification(latestAttempt);
                result.ResultStatus = classification;
                all.Add(result);
                if (classification == "Đạt")
                {
                    passed.Add(result);
                    continue;
                }

                failed.Add(result);
                if (classification == "Kiểm tra lại")
                    retake.Add(result);
                else
                    retrain.Add(result);
            }

            return new QuickStatisticsBucket
            {
                Statistics = new ExamQuickStatisticsPeriod
                {
                    Period = period,
                    StartDate = startDate,
                    EndDate = endDate.AddDays(-1),
                    Total = all.Count,
                    Passed = passed.Count,
                    Failed = failed.Count,
                    Retake = retake.Count,
                    Retrain = retrain.Count,
                    NotTaken = notTaken.Count
                },
                All = all,
                Passed = passed,
                Failed = failed,
                Retake = retake,
                Retrain = retrain,
                NotTaken = notTaken
            };
        }

        private static string GetQuickStatisticsClassification(QuickStatisticsAttempt attempt)
        {
            if (attempt.Passed == true) return "Đạt";
            if (attempt.Score.HasValue && !string.IsNullOrWhiteSpace(attempt.AttemptJson))
            {
                var snapshot = JsonSerializer.Deserialize<OnlineExamAttempt>(attempt.AttemptJson, Json);
                if (snapshot?.Definition?.Scores?.Length >= 2)
                    return OnlineExamRepository.Classification(attempt.Score.Value, snapshot.Definition);
            }
            if (!attempt.Score.HasValue) return "Không đạt";
            var fallbackClassification = OnlineExamRepository.Classification(attempt.Score.Value);
            return attempt.Passed == false && fallbackClassification == "Đạt"
                ? "Kiểm tra lại"
                : fallbackClassification;
        }

        private sealed class QuickStatisticsBucket
        {
            public ExamQuickStatisticsPeriod Statistics { get; set; }
            public IReadOnlyList<ExamQuickStatisticsEmployee> All { get; set; }
            public IReadOnlyList<ExamQuickStatisticsEmployee> Passed { get; set; }
            public IReadOnlyList<ExamQuickStatisticsEmployee> Failed { get; set; }
            public IReadOnlyList<ExamQuickStatisticsEmployee> Retake { get; set; }
            public IReadOnlyList<ExamQuickStatisticsEmployee> Retrain { get; set; }
            public IReadOnlyList<ExamQuickStatisticsEmployee> NotTaken { get; set; }
        }

        private sealed class QuickStatisticsRosterEmployee
        {
            public int EmployeeId { get; set; }
            public string EmployeeName { get; set; }
            public string EmployeeCode { get; set; }
            public int DepartmentId { get; set; }
            public string DepartmentName { get; set; }
        }

        private sealed class QuickStatisticsAttempt
        {
            public int Id { get; set; }
            public int? EmployeeId { get; set; }
            public string EmployeeCode { get; set; }
            public string EmployeeName { get; set; }
            public int DepartmentId { get; set; }
            public string DepartmentName { get; set; }
            public string ExamName { get; set; }
            public DateTime? ExamDate { get; set; }
            public string CycleName { get; set; }
            public int Mission { get; set; }
            public int? CorrectAnswers { get; set; }
            public int? TotalQuestions { get; set; }
            public decimal? Score { get; set; }
            public bool? Passed { get; set; }
            public int? CountingTime { get; set; }
            public DateTime? SubmittedAt { get; set; }
            public string AttemptJson { get; set; }
        }

        public async Task<bool> DeleteResult(int id, string deletedBy)
        {
            var result = await context.OnlineExamAttempts.FirstOrDefaultAsync(attempt =>
                attempt.Id == id && attempt.DeletedAt == null && attempt.SubmittedAt != null);
            if (result == null) return false;

            var deletedAt = DateTime.UtcNow;
            result.DeletedAt = deletedAt;
            result.DeletedBy = deletedBy;
            result.UpdatedAt = deletedAt;
            result.UpdatedBy = deletedBy;
            await context.SaveChangesAsync();
            return true;
        }

        private IQueryable<ExamResultItem> FilteredResults(int? departmentId, int? examId, string search)
        {
            var attempts = context.OnlineExamAttempts.AsNoTracking()
                .Where(attempt => attempt.DeletedAt == null && attempt.SubmittedAt != null);
            if (departmentId.HasValue)
                attempts = attempts.Where(attempt => attempt.DepartmentId == departmentId.Value);
            if (examId.HasValue)
                attempts = attempts.Where(attempt => attempt.CatalogExamId == examId.Value);

            var joined = (
                from attempt in attempts
                join exam in context.DepartmentExams.IgnoreQueryFilters().AsNoTracking()
                    on attempt.CatalogExamId equals (int?)exam.Id into examJoin
                from exam in examJoin.DefaultIfEmpty()
                join department in context.Departments.AsNoTracking()
                    on attempt.DepartmentId equals department.Id into departmentJoin
                from department in departmentJoin.DefaultIfEmpty()
                select new ExamResultItem
                {
                    Id = attempt.Id,
                    EmployeeName = attempt.Name,
                    EmployeeCode = attempt.Code,
                    ExamName = exam == null ? attempt.Type ?? "Bài thi" : exam.Name,
                    DepartmentName = department == null ? string.Empty : department.Name,
                    DepartmentId = attempt.DepartmentId,
                    ExamDate = attempt.CreateDate,
                    CycleName = attempt.CycleName,
                    SubmittedAt = attempt.SubmittedAt,
                    CorrectAnswers = attempt.Results,
                    TotalQuestions = attempt.TotalQuestions,
                    ScoreSummary = attempt.Results.HasValue && attempt.TotalQuestions.HasValue
                        ? attempt.Results.Value + "/" + attempt.TotalQuestions.Value
                        : null,
                    Score = attempt.Scores,
                    Passed = attempt.Status,
                    ResultStatus = attempt.Status == true ? "Đạt" : attempt.Status == false ? "Không đạt" : "Chưa xác định",
                    Mission = attempt.Mission,
                    Newbie = attempt.Newbie,
                    CountingTime = attempt.CountingTime
                });
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = $"%{search.Trim()}%";
                joined = joined.Where(item =>
                    EF.Functions.Like(item.EmployeeName, term) ||
                    EF.Functions.Like(item.EmployeeCode, term) ||
                    EF.Functions.Like(item.ExamName, term));
            }
            return joined;
        }

        public async Task<ExamStatistics> Statistics(int page, int pageSize, string search)
        {
            var attempts = await (
                from attempt in context.OnlineExamAttempts.AsNoTracking()
                join exam in context.DepartmentExams.AsNoTracking()
                    on attempt.CatalogExamId equals (int?)exam.Id into examJoin
                from exam in examJoin.DefaultIfEmpty()
                where attempt.DeletedAt == null && attempt.SubmittedAt != null
                orderby attempt.SubmittedAt descending
                select new
                {
                    attempt.Id,
                    attempt.Name,
                    attempt.Code,
                    attempt.DepartmentId,
                    ExamName = exam == null ? attempt.Type ?? "Bài thi" : exam.Name,
                    ExamDefinitionJson = exam == null ? null : exam.DefinitionJson,
                    attempt.CreateDate,
                    attempt.Scores,
                    attempt.Status,
                    attempt.FailAnswers,
                    attempt.AttemptJson
                }).ToListAsync();

            var wrongAnswers = new List<WrongAnswerItem>();
            foreach (var attempt in attempts)
            {
                if (string.IsNullOrWhiteSpace(attempt.FailAnswers)) continue;
                var failedQuestions = JsonSerializer.Deserialize<List<FailedQuestion>>(attempt.FailAnswers, Json);
                if (failedQuestions == null) continue;
                var questionImages = GetQuestionImages(attempt.AttemptJson, attempt.ExamDefinitionJson);
                wrongAnswers.AddRange(failedQuestions.Select(failed => new WrongAnswerItem
                {
                    AttemptId = attempt.Id,
                    EmployeeName = attempt.Name,
                    EmployeeCode = attempt.Code,
                    DepartmentId = attempt.DepartmentId,
                    ExamName = attempt.ExamName,
                    ExamDate = attempt.CreateDate,
                    Score = attempt.Scores,
                    QuestionId = failed.QuestionId,
                    Question = failed.Question ?? string.Empty,
                    ImageUrl = failed.Image ?? (questionImages.TryGetValue(failed.QuestionId, out var image) ? image : null),
                    Answers = failed.Answers ?? Array.Empty<string>(),
                    AnswerSummary = string.Join(", ", failed.Answers ?? Array.Empty<string>())
                }));
            }

            var topQuestions = wrongAnswers
                .Where(item => !string.IsNullOrWhiteSpace(item.Question))
                .GroupBy(item => item.Question.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(group => new TopWrongQuestionItem
                {
                    Question = group.Key,
                    ImageUrl = group.Select(item => item.ImageUrl).FirstOrDefault(image => !string.IsNullOrWhiteSpace(image)),
                    WrongCount = group.Count()
                })
                .OrderByDescending(item => item.WrongCount)
                .ThenBy(item => item.Question)
                .Take(10)
                .ToList();
            var passed = attempts.Count(attempt => attempt.Status == true);
            var filteredWrongAnswers = string.IsNullOrWhiteSpace(search)
                ? wrongAnswers
                : wrongAnswers.Where(item =>
                    Contains(item.EmployeeName, search) ||
                    Contains(item.EmployeeCode, search) ||
                    Contains(item.ExamName, search) ||
                    Contains(item.Question, search) ||
                    Contains(item.AnswerSummary, search)).ToList();

            return new ExamStatistics
            {
                SubmittedAttempts = attempts.Count,
                PassedAttempts = passed,
                FailedAttempts = attempts.Count(attempt => attempt.Status == false),
                WrongAnswerTotalItems = filteredWrongAnswers.Count,
                WrongAnswerPage = page,
                WrongAnswerPageSize = pageSize,
                WrongAnswers = filteredWrongAnswers.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                TopWrongQuestions = topQuestions
            };
        }

        private static bool Contains(string value, string search) =>
            !string.IsNullOrEmpty(value) && value.Contains(search, StringComparison.OrdinalIgnoreCase);

        private sealed class FailedQuestion
        {
            public int QuestionId { get; set; }
            public string Question { get; set; }
            public string Image { get; set; }
            public string[] Answers { get; set; }
        }

        private static Dictionary<int, string> GetQuestionImages(string attemptJson, string definitionJson)
        {
            var images = new Dictionary<int, string>();
            if (!string.IsNullOrWhiteSpace(attemptJson))
            {
                var attempt = JsonSerializer.Deserialize<OnlineExamAttempt>(attemptJson, Json);
                if (attempt?.Definition?.Groups != null)
                    AddQuestionImages(images, attempt.Definition);
            }

            if (!string.IsNullOrWhiteSpace(definitionJson))
            {
                var definition = JsonSerializer.Deserialize<OnlineExamDefinition>(definitionJson, Json);
                if (definition?.Groups != null)
                    AddQuestionImages(images, definition, onlyMissing: true);
            }

            return images;
        }

        private static void AddQuestionImages(Dictionary<int, string> images, OnlineExamDefinition definition, bool onlyMissing = false)
        {
            foreach (var question in definition.Groups.SelectMany(group => group.Questions))
            {
                if (!string.IsNullOrWhiteSpace(question.Image) && (!onlyMissing || !images.ContainsKey(question.Id)))
                    images[question.Id] = question.Image;
            }
        }
    }
}
