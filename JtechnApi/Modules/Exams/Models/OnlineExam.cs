namespace JtechnApi.Exams.Models
{
    public class OnlineExamDefinition
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Minutes { get; set; }
        public decimal[] Scores { get; set; }
        public List<OnlineExamGroup> Groups { get; set; } = new();
    }

    public class OnlineExamGroup
    {
        public string Title { get; set; }
        public int Columns { get; set; }
        public List<OnlineExamQuestion> Questions { get; set; } = new();
    }

    public class OnlineExamQuestion
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public decimal Points { get; set; }
        public string Image { get; set; }
        public string Kind { get; set; }
        public List<OnlineExamOption> Options { get; set; } = new();
        public string[] Correct { get; set; } = Array.Empty<string>();
    }

    public class OnlineExamOption
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }

    public class OnlineExamAttempt
    {
        public DateTime? ExamDate { get; set; }
        public bool? IsNewEmployee { get; set; }
        public int? CatalogExamId { get; set; }
        public Guid Id { get; set; }
        public string EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? SubmittedAt { get; set; }
        public OnlineExamDefinition Definition { get; set; }
        public Dictionary<int, string[]> Answers { get; set; } = new();
        public decimal Score { get; set; }
        public int Mission { get; set; } = 1;
    }

    public class OnlineExamAnswers
    {
        public Dictionary<int, string[]> Answers { get; set; } = new();
    }

    public class OnlineExamStartInput
    {
        [System.ComponentModel.DataAnnotations.Required]
        public DateTime? ExamDate { get; set; }
        public bool IsNewEmployee { get; set; }
    }
}
