using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using JtechnApi.Exams.Models;
using Microsoft.AspNetCore.Hosting;

namespace JtechnApi.Exams.Repositories
{
    // Single-host file storage. Keep the folder on persistent storage and back it up.
    public sealed class OnlineExamRepository : IOnlineExamRepository
    {
        private readonly IWebHostEnvironment environment;
        private readonly IServiceScopeFactory scopes;
        private readonly SemaphoreSlim gate = new(1, 1);
        private readonly JsonSerializerOptions json = new() { PropertyNameCaseInsensitive = true };
        private string Folder => Path.Combine(environment.ContentRootPath, "App_Data", "OnlineExams");

        public OnlineExamRepository(IWebHostEnvironment environment, IServiceScopeFactory scopes = null)
        {
            this.environment = environment;
            this.scopes = scopes;
        }

        public async Task<object> Catalog(string employeeId, int? examId = null)
        {
            if (scopes != null && !examId.HasValue)
            {
                if (!int.TryParse(employeeId, out var id)) throw new UnauthorizedAccessException();
                using var scope = scopes.CreateScope();
                return new { selectExam = true, exams = await scope.ServiceProvider.GetRequiredService<IDepartmentExamRepository>().Available(id) };
            }
            EnsureEligible(employeeId, examId);
            var definition = Definition(examId);
            return new { definition.Id, definition.Title, definition.Minutes,
                TotalQuestions = definition.Groups.Sum(g => g.Questions.Count),
                TotalPoints = definition.Groups.SelectMany(g => g.Questions).Sum(q => q.Points),
                MissingImages = MissingImages(examId).Length,
                Attempts = (await History(employeeId)).Where(a => a.CatalogExamId == examId).Select(a => new { a.Id, a.ExamDate, a.IsNewEmployee, a.StartedAt, a.SubmittedAt,
                    Score = a.SubmittedAt != null ? (decimal?)a.Score : null,
                    Classification = a.SubmittedAt != null ? Classification(a.Score, a.Definition) : "Chưa nộp" }) };
        }

        public async Task<object> GetCandidate(Guid id, string employeeId)
        {
            var attempt = await Get(id, employeeId);
            if (attempt == null) return null;
            if (attempt.SubmittedAt == null && attempt.CatalogExamId.HasValue) EnsureEligible(employeeId, attempt.CatalogExamId);
            if (attempt.SubmittedAt == null && DateTimeOffset.UtcNow >= attempt.ExpiresAt)
                attempt = await Update(id, employeeId, attempt.Answers, true);
            return CandidateView(attempt);
        }

        private void EnsureEligible(string employeeId, int? examId)
        {
            if (scopes == null) return;
            if (!examId.HasValue || !int.TryParse(employeeId, out var id)) throw new UnauthorizedAccessException("Vui lòng chọn bài thi thuộc bộ phận của bạn.");
            using var scope = scopes.CreateScope();
            if (!scope.ServiceProvider.GetRequiredService<IDepartmentExamRepository>().CanTake(id, examId.Value))
                throw new UnauthorizedAccessException("Bạn không được làm bài thi này hoặc bài thi đã đóng.");
        }

        public async Task<object> StartCandidate(string employeeId, string name, int? examId = null, OnlineExamStartInput input = null)
        {
            if (input?.ExamDate == null || input.ExamDate.Value.Year < 1900 || input.ExamDate.Value.Year > 2100)
                throw new ArgumentException("Vui lòng chọn ngày thi hợp lệ.");
            EnsureEligible(employeeId, examId);
            return CandidateView(await Start(employeeId, name, examId, input.ExamDate.Value.Date, input.IsNewEmployee));
        }

        public async Task<object> SaveCandidate(Guid id, string employeeId, Dictionary<int, string[]> answers, bool submit)
        {
            var attempt = await Update(id, employeeId, answers, submit);
            return attempt == null ? null : CandidateView(attempt);
        }

        public OnlineExamDefinition Definition(int? examId = null)
        {
            if (!examId.HasValue)
                return JsonSerializer.Deserialize<OnlineExamDefinition>(File.ReadAllText(Path.Combine(environment.ContentRootPath, "Modules", "Exams", "Data", "stamping-new.json")), json);
            if (scopes == null) throw new InvalidOperationException("Database exam repository is not configured.");
            using var scope = scopes.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IDepartmentExamRepository>().LoadDefinition(examId.Value);
        }

        public string[] MissingImages(int? examId = null) => Definition(examId).Groups.SelectMany(g => g.Questions)
            .Where(q => !string.IsNullOrEmpty(q.Image)).Select(q => q.Image).Distinct()
            .Where(path => !File.Exists(Path.Combine(environment.WebRootPath, path.TrimStart('/')))).ToArray();

        private string FileName(Guid id) => Path.Combine(Folder, id.ToString("N") + ".json");

        private async Task Write(OnlineExamAttempt attempt)
        {
            using var scope = scopes?.CreateScope();
            var store = scope?.ServiceProvider.GetService(typeof(IOnlineExamAttemptStore)) as IOnlineExamAttemptStore;
            if (store != null)
            {
                await store.Save(attempt);
                return;
            }

            Directory.CreateDirectory(Folder);
            var target = FileName(attempt.Id);
            var temporary = target + ".tmp";
            await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(attempt, json));
            File.Move(temporary, target, true);
        }

        private async Task<OnlineExamAttempt> Read(Guid id, string employeeId)
        {
            using var scope = scopes?.CreateScope();
            var store = scope?.ServiceProvider.GetService(typeof(IOnlineExamAttemptStore)) as IOnlineExamAttemptStore;
            if (store != null) return await store.Get(id, employeeId);

            if (!File.Exists(FileName(id))) return null;
            var attempt = JsonSerializer.Deserialize<OnlineExamAttempt>(await File.ReadAllTextAsync(FileName(id)), json);
            return attempt.EmployeeId == employeeId ? attempt : null;
        }

        public async Task<List<OnlineExamAttempt>> History(string employeeId)
        {
            await gate.WaitAsync();
            try { return await HistoryCore(employeeId); }
            finally { gate.Release(); }
        }

        private async Task<List<OnlineExamAttempt>> HistoryCore(string employeeId)
        {
            using var scope = scopes?.CreateScope();
            var store = scope?.ServiceProvider.GetService(typeof(IOnlineExamAttemptStore)) as IOnlineExamAttemptStore;
            if (store != null) return await store.History(employeeId);

            var result = new List<OnlineExamAttempt>();
            if (!Directory.Exists(Folder)) return result;
            foreach (var file in Directory.EnumerateFiles(Folder, "*.json"))
            {
                var attempt = JsonSerializer.Deserialize<OnlineExamAttempt>(await File.ReadAllTextAsync(file), json);
                if (attempt.EmployeeId == employeeId) result.Add(attempt);
            }
            return result.OrderByDescending(a => a.StartedAt).ToList();
        }

        public async Task<OnlineExamAttempt> Start(string employeeId, string name, int? examId = null, DateTime? examDate = null, bool isNewEmployee = false)
        {
            EnsureEligible(employeeId, examId);
            await gate.WaitAsync();
            try
            {
                var active = (await HistoryCore(employeeId)).FirstOrDefault(a => a.SubmittedAt == null && a.CatalogExamId == examId);
                if (active != null) return active;
                if (MissingImages(examId).Length > 0)
                    throw new InvalidOperationException("Đề chưa đủ hình ảnh. Vui lòng liên hệ quản lý để bổ sung trước khi thi.");
                var definition = Definition(examId);
                var now = DateTimeOffset.UtcNow;
                var attempt = new OnlineExamAttempt { ExamDate = examDate?.Date, IsNewEmployee = isNewEmployee, CatalogExamId = examId, Id = Guid.NewGuid(), EmployeeId = employeeId,
                    EmployeeName = name, StartedAt = now, ExpiresAt = now.AddMinutes(definition.Minutes), Definition = definition };
                await Write(attempt);
                return attempt;
            }
            finally { gate.Release(); }
        }

        public async Task<OnlineExamAttempt> Get(Guid id, string employeeId)
        {
            await gate.WaitAsync();
            try { return await Read(id, employeeId); }
            finally { gate.Release(); }
        }

        public static decimal Grade(OnlineExamDefinition definition, Dictionary<int, string[]> answers) =>
            definition.Groups.SelectMany(g => g.Questions).Sum(q =>
            {
                if (!answers.TryGetValue(q.Id, out var selected) || selected == null) return 0m;
                var correct = q.Kind == "matching" ? selected.SequenceEqual(q.Correct, StringComparer.Ordinal)
                    : new HashSet<string>(selected, StringComparer.Ordinal).SetEquals(q.Correct);
                return correct ? q.Points : 0m;
            });

        private static void Validate(OnlineExamAttempt attempt, Dictionary<int, string[]> answers)
        {
            if (answers == null) throw new ArgumentException("Dữ liệu câu trả lời không hợp lệ.");
            var questions = attempt.Definition.Groups.SelectMany(g => g.Questions).ToDictionary(q => q.Id);
            foreach (var entry in answers)
            {
                if (!questions.TryGetValue(entry.Key, out var question) || entry.Value == null)
                    throw new ArgumentException("Câu hỏi không hợp lệ.");
                var values = entry.Value;
                if (question.Kind == "matching")
                {
                    if (values.Length != question.Options.Count || values.Any(v => v != "" &&
                        (!int.TryParse(v, out var n) || n < 1 || n > question.Options.Count)))
                        throw new ArgumentException("Số ghép đáp án không hợp lệ.");
                }
                else if (values.Length > (question.Kind == "single" ? 1 : question.Options.Count) ||
                    values.Distinct().Count() != values.Length || values.Any(v => !question.Options.Any(o => o.Id == v)))
                    throw new ArgumentException("Đáp án không hợp lệ.");
            }
        }

        public async Task<OnlineExamAttempt> Update(Guid id, string employeeId, Dictionary<int, string[]> answers, bool submit)
        {
            await gate.WaitAsync();
            try
            {
                var attempt = await Read(id, employeeId);
                if (attempt == null || attempt.SubmittedAt != null) return attempt;
                if (attempt.CatalogExamId.HasValue) EnsureEligible(employeeId, attempt.CatalogExamId);
                var now = DateTimeOffset.UtcNow;
                // After deadline, grade only answers already received by the server.
                if (now < attempt.ExpiresAt)
                {
                    Validate(attempt, answers);
                    attempt.Answers = answers;
                }
                if (submit || now >= attempt.ExpiresAt)
                {
                    attempt.Score = Grade(attempt.Definition, attempt.Answers);
                    attempt.SubmittedAt = now;
                }
                await Write(attempt);
                return attempt;
            }
            finally { gate.Release(); }
        }

        public static string Classification(decimal score) => score >= 80 ? "Đạt" : score > 60 ? "Kiểm tra lại" : score >= 50 ? "Đào tạo lại" : "Không đạt";

        public static string Classification(decimal score, OnlineExamDefinition definition)
        {
            var scores = definition?.Scores;
            if (scores == null) return Classification(score);
            return score >= scores[0] ? "Đạt" : score >= scores[1] ? "Kiểm tra lại" : "Không đạt (Đào tạo lại)";
        }

        // Never serialize the stored definition to candidates: it contains the answer key.
        public static object CandidateView(OnlineExamAttempt a) => new
        {
            a.Id, a.EmployeeName, a.ExamDate, a.IsNewEmployee, a.StartedAt, a.ExpiresAt, a.SubmittedAt, a.Answers,
            ServerNow = DateTimeOffset.UtcNow,
            Score = a.SubmittedAt != null ? (decimal?)a.Score : null,
            Classification = a.SubmittedAt != null ? Classification(a.Score, a.Definition) : null,
            Definition = new { a.Definition.Title, a.Definition.Minutes,
                Groups = a.Definition.Groups.Select(g => new { g.Title, g.Columns,
                    Questions = g.Questions.Select(q => new { q.Id, q.Text, q.Points, q.Image, q.Kind, q.Options }) }) }
        };
    }
}
