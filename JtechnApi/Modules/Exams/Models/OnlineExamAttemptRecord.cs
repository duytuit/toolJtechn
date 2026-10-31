using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.Exams.Models
{
    [Table("online_exam_attempt", Schema = "dbo")]
    public class OnlineExamAttemptRecord
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("attempt_key")]
        public Guid? AttemptKey { get; set; }

        [Column("employee_id")]
        public int? EmployeeId { get; set; }

        [Column("catalog_exam_id")]
        public int? CatalogExamId { get; set; }

        [Required, MaxLength(200), Column("name")]
        public string Name { get; set; }

        [MaxLength(50), Column("code")]
        public string Code { get; set; }

        [Column("dept_id")]
        public int DepartmentId { get; set; }

        [MaxLength(200), Column("cycle_name")]
        public string CycleName { get; set; }

        [Column("create_date", TypeName = "date")]
        public DateTime? CreateDate { get; set; }

        [Column("results")]
        public int? Results { get; set; }

        [Column("total_questions")]
        public int? TotalQuestions { get; set; }

        [Column("status")]
        public bool? Status { get; set; }

        [Column("confirm")]
        public bool Confirm { get; set; }

        [Column("counting_time")]
        public int? CountingTime { get; set; }

        [Column("limit_time")]
        public int? LimitTime { get; set; }

        [Column("created_at", TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at", TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at", TypeName = "datetime2")]
        public DateTime? DeletedAt { get; set; }

        [MaxLength(50), Column("updated_by")]
        public string UpdatedBy { get; set; }

        [MaxLength(50), Column("deleted_by")]
        public string DeletedBy { get; set; }

        [Column("mission")]
        public int Mission { get; set; }

        [Column("scores", TypeName = "decimal(10,4)")]
        public decimal? Scores { get; set; }

        [MaxLength(200), Column("examinations")]
        public string Examinations { get; set; }

        [MaxLength(200), Column("date_examinations")]
        public string DateExaminations { get; set; }

        [MaxLength(100), Column("type")]
        public string Type { get; set; }

        [Column("fail_aws")]
        public string FailAnswers { get; set; }

        [Column("newbie")]
        public bool Newbie { get; set; }

        [Column("started_at", TypeName = "datetime2")]
        public DateTime? StartedAt { get; set; }

        [Column("expires_at", TypeName = "datetime2")]
        public DateTime? ExpiresAt { get; set; }

        [Column("submitted_at", TypeName = "datetime2")]
        public DateTime? SubmittedAt { get; set; }

        [Column("attempt_json")]
        public string AttemptJson { get; set; }
    }
}
