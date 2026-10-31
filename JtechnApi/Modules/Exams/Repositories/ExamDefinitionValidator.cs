using System.Text.Json;
using JtechnApi.Exams.Models;

namespace JtechnApi.Exams.Repositories
{
    public static class ExamDefinitionValidator
    {
        private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };
        public static OnlineExamDefinition Parse(string json)
        {
            var definition = JsonSerializer.Deserialize<OnlineExamDefinition>(json, Json);
            if (definition?.Groups == null || definition.Groups.Count == 0 || definition.Groups.Any(g => g == null || g.Questions == null || g.Questions.Count == 0))
                throw new ArgumentException("Đề phải có nhóm và câu hỏi.");
            if (definition.Scores != null && (definition.Scores.Length != 2 || definition.Scores[0] > 100 ||
                definition.Scores[1] < 0 || definition.Scores[0] <= definition.Scores[1]))
                throw new ArgumentException("Mốc điểm phải gồm 2 số: 0 ≤ mốc kiểm tra lại < mốc đạt ≤ 100.");
            var questions = definition.Groups.SelectMany(g => g.Questions).ToList();
            if (questions.Count > 500 || questions.Any(q => q == null) || questions.Select(q => q.Id).Distinct().Count() != questions.Count)
                throw new ArgumentException("Tối đa 500 câu, ID câu hỏi phải duy nhất.");
            foreach (var q in questions)
            {
                if (q.Id <= 0 || q.Points <= 0 || q.Options == null || q.Options.Count == 0 || q.Options.Any(o => o == null || string.IsNullOrWhiteSpace(o.Id) || string.IsNullOrWhiteSpace(o.Text)) ||
                    q.Options.Select(o => o.Id).Distinct().Count() != q.Options.Count || q.Correct == null || q.Correct.Length == 0 ||
                    q.Correct.Distinct().Count() != q.Correct.Length || !new[] { "single", "multiple", "matching" }.Contains(q.Kind))
                    throw new ArgumentException($"Câu {q.Id}: kiểm tra loại câu, điểm, lựa chọn và đáp án.");
                if (q.Kind == "matching")
                {
                    if (q.Correct.Length != q.Options.Count || q.Correct.Any(v => !int.TryParse(v, out var n) || n < 1 || n > q.Options.Count))
                        throw new ArgumentException($"Câu {q.Id}: đáp án ghép số không hợp lệ.");
                }
                else if ((q.Kind == "single" && q.Correct.Length != 1) || q.Correct.Any(v => !q.Options.Any(o => o.Id == v)))
                    throw new ArgumentException($"Câu {q.Id}: đáp án không thuộc danh sách lựa chọn.");
                if (!string.IsNullOrEmpty(q.Image) && (!q.Image.StartsWith("/img/", StringComparison.Ordinal) || q.Image.Contains("..") || q.Image.Contains('\\') || q.Image.Contains('?') || q.Image.Contains('#')))
                    throw new ArgumentException($"Câu {q.Id}: ảnh phải nằm dưới /img/.");
            }
            var totalPoints = decimal.Round(questions.Sum(q => q.Points), 2, MidpointRounding.AwayFromZero);
            if (totalPoints != 100m) throw new ArgumentException("Tổng điểm đề thi phải bằng 100.");
            return definition;
        }

    }
}
