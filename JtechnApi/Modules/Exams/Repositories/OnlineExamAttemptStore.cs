using System.Text.Json;
using System.Globalization;
using JtechnApi.Employees.Models;
using JtechnApi.Exams.Models;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;

namespace JtechnApi.Exams.Repositories
{
    public sealed class OnlineExamAttemptStore : IOnlineExamAttemptStore
    {
        private readonly DBJtecContext context;
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public OnlineExamAttemptStore(DBJtecContext context) => this.context = context;

        public async Task<OnlineExamAttempt> Get(Guid id, string employeeId)
        {
            if (!int.TryParse(employeeId, out var employeeKey)) return null;
            var row = await context.OnlineExamAttempts.AsNoTracking().FirstOrDefaultAsync(a =>
                a.AttemptKey == id && a.EmployeeId == employeeKey && a.DeletedAt == null && a.AttemptJson != null);
            return row == null ? null : JsonSerializer.Deserialize<OnlineExamAttempt>(row.AttemptJson, Json);
        }

        public async Task<List<OnlineExamAttempt>> History(string employeeId)
        {
            if (!int.TryParse(employeeId, out var employeeKey)) return new List<OnlineExamAttempt>();
            var stored = await context.OnlineExamAttempts.AsNoTracking()
                .Where(a => a.EmployeeId == employeeKey && a.DeletedAt == null && a.AttemptJson != null)
                .Select(a => a.AttemptJson)
                .ToListAsync();
            return stored.Select(value => JsonSerializer.Deserialize<OnlineExamAttempt>(value, Json))
                .Where(attempt => attempt != null)
                .OrderByDescending(attempt => attempt.StartedAt)
                .ToList();
        }

        public async Task Save(OnlineExamAttempt attempt)
        {
            if (!int.TryParse(attempt.EmployeeId, out var employeeKey))
                throw new InvalidOperationException("Không thể lưu lượt thi: mã tài khoản nhân viên không hợp lệ.");

            var employee = await context.Employee.AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == employeeKey && item.IsActive);
            if (employee == null)
                throw new InvalidOperationException("Không tìm thấy nhân viên đang hoạt động để lưu kết quả thi.");

            int departmentId;
            if (attempt.CatalogExamId.HasValue)
            {
                var exam = await context.DepartmentExams.AsNoTracking()
                    .FirstOrDefaultAsync(item => item.Id == attempt.CatalogExamId.Value);
                if (exam == null)
                    throw new InvalidOperationException("Không tìm thấy bài thi để lưu kết quả.");
                departmentId = exam.DepartmentId;
            }
            else
            {
                var assignment = await context.EmployeeDepartments.AsNoTracking()
                    .Where(item => item.EmployeeId == employeeKey)
                    .OrderBy(item => item.Id)
                    .FirstOrDefaultAsync();
                if (assignment == null)
                    throw new InvalidOperationException("Nhân viên chưa được gán bộ phận để lưu kết quả thi.");
                departmentId = assignment.DepartmentId;
            }

            var row = await context.OnlineExamAttempts
                .FirstOrDefaultAsync(item => item.AttemptKey == attempt.Id && item.DeletedAt == null);
            var isNew = row == null;
            if (isNew)
            {
                var mission = await context.OnlineExamAttempts.CountAsync(item =>
                    item.EmployeeId == employeeKey && item.CatalogExamId == attempt.CatalogExamId && item.DeletedAt == null);
                row = new OnlineExamAttemptRecord
                {
                    AttemptKey = attempt.Id,
                    EmployeeId = employeeKey,
                    CatalogExamId = attempt.CatalogExamId,
                    CreatedAt = attempt.StartedAt.UtcDateTime,
                    Mission = mission + 1
                };
                context.OnlineExamAttempts.Add(row);
            }

            attempt.Mission = row.Mission;
            var now = DateTime.UtcNow;
            var questionList = attempt.Definition.Groups.SelectMany(group => group.Questions).ToList();
            row.Name = employee.FullName ?? attempt.EmployeeName;
            row.Code = employee.EmployeeCode;
            row.DepartmentId = departmentId;
            row.CycleName = attempt.ExamDate?.ToString("MMyyyy", CultureInfo.InvariantCulture);
            row.CreateDate = attempt.ExamDate?.Date;
            row.TotalQuestions = questionList.Count;
            row.CountingTime = ElapsedSeconds(attempt);
            row.LimitTime = attempt.Definition.Minutes;
            row.UpdatedAt = isNew ? null : now;
            row.UpdatedBy = employeeKey.ToString();
            row.Mission = attempt.Mission;
            row.Scores = attempt.SubmittedAt.HasValue ? attempt.Score : null;
            row.Examinations = null;
            row.DateExaminations = null;
            row.Type = null;
            row.Newbie = attempt.IsNewEmployee ?? false;
            row.StartedAt = attempt.StartedAt.UtcDateTime;
            row.ExpiresAt = attempt.ExpiresAt.UtcDateTime;
            row.SubmittedAt = attempt.SubmittedAt?.UtcDateTime;
            row.AttemptJson = JsonSerializer.Serialize(attempt, Json);

            if (attempt.SubmittedAt.HasValue)
            {
                var answers = attempt.Answers ?? new Dictionary<int, string[]>();
                row.Results = questionList.Count(question => IsCorrect(question, answers));
                var passingScore = attempt.Definition.Scores?.FirstOrDefault() ?? 80m;
                row.Status = attempt.Score >= passingScore;
                row.FailAnswers = JsonSerializer.Serialize(questionList
                    .Where(question => !IsCorrect(question, answers))
                    .Select(question => new
                    {
                        questionId = question.Id,
                        question = question.Text,
                        image = question.Image,
                        answers = AnswerText(question, answers)
                    }), Json);
            }
            else
            {
                row.Results = null;
                row.Status = null;
                row.FailAnswers = null;
            }

            await context.SaveChangesAsync();
        }

        private static int ElapsedSeconds(OnlineExamAttempt attempt)
        {
            var end = attempt.SubmittedAt ?? DateTimeOffset.UtcNow;
            return (int)Math.Clamp((end - attempt.StartedAt).TotalSeconds, 0, int.MaxValue);
        }

        private static bool IsCorrect(OnlineExamQuestion question, Dictionary<int, string[]> answers)
        {
            if (!answers.TryGetValue(question.Id, out var selected) || selected == null) return false;
            return question.Kind == "matching"
                ? selected.SequenceEqual(question.Correct, StringComparer.Ordinal)
                : new HashSet<string>(selected, StringComparer.Ordinal).SetEquals(question.Correct);
        }

        private static string[] AnswerText(OnlineExamQuestion question, Dictionary<int, string[]> answers)
        {
            if (!answers.TryGetValue(question.Id, out var selected) || selected == null) return Array.Empty<string>();
            if (question.Kind == "matching") return selected;
            return selected.Select(id => question.Options.FirstOrDefault(option => option.Id == id)?.Text ?? id).ToArray();
        }
    }
}
