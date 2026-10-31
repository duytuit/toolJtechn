namespace JtechnApi.Exams.Models
{
    public sealed class ExamResultItem
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string ExamName { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        public DateTime? ExamDate { get; set; }
        public string CycleName { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int? CorrectAnswers { get; set; }
        public int? TotalQuestions { get; set; }
        public string ScoreSummary { get; set; }
        public decimal? Score { get; set; }
        public bool? Passed { get; set; }
        public string ResultStatus { get; set; }
        public int Mission { get; set; }
        public bool Newbie { get; set; }
        public int? CountingTime { get; set; }
    }

    public sealed class ExamResultPage
    {
        public IReadOnlyList<ExamResultItem> Items { get; set; }
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public sealed class ExamResultFilterOptions
    {
        public IReadOnlyList<ExamResultDepartmentOption> Departments { get; set; }
        public IReadOnlyList<ExamResultExamOption> Exams { get; set; }
    }

    public sealed class ExamResultDepartmentOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public sealed class ExamResultExamOption
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }
    }

    public sealed class ExamQuickStatistics
    {
        public IReadOnlyList<ExamQuickStatisticsPeriod> Periods { get; set; }
    }

    public sealed class ExamQuickStatisticsPeriod
    {
        public int Period { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Total { get; set; }
        public int Passed { get; set; }
        public int Failed { get; set; }
        public int Retake { get; set; }
        public int Retrain { get; set; }
        public int NotTaken { get; set; }
    }

    public sealed class ExamQuickStatisticsEmployee
    {
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        public string ExamName { get; set; }
        public DateTime? ExamDate { get; set; }
        public string CycleName { get; set; }
        public int? Mission { get; set; }
        public int? CorrectAnswers { get; set; }
        public int? TotalQuestions { get; set; }
        public decimal? Score { get; set; }
        public string ResultStatus { get; set; }
        public int? CountingTime { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    public sealed class WrongAnswerItem
    {
        public int AttemptId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public int DepartmentId { get; set; }
        public string ExamName { get; set; }
        public DateTime? ExamDate { get; set; }
        public decimal? Score { get; set; }
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public string ImageUrl { get; set; }
        public string[] Answers { get; set; }
        public string AnswerSummary { get; set; }
    }

    public sealed class TopWrongQuestionItem
    {
        public string Question { get; set; }
        public string ImageUrl { get; set; }
        public int WrongCount { get; set; }
    }

    public sealed class ExamStatistics
    {
        public int SubmittedAttempts { get; set; }
        public int PassedAttempts { get; set; }
        public int FailedAttempts { get; set; }
        public int WrongAnswerTotalItems { get; set; }
        public int WrongAnswerPage { get; set; }
        public int WrongAnswerPageSize { get; set; }
        public IReadOnlyList<WrongAnswerItem> WrongAnswers { get; set; }
        public IReadOnlyList<TopWrongQuestionItem> TopWrongQuestions { get; set; }
    }
}
