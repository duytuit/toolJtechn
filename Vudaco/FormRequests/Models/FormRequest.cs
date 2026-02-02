using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.FormRequests.Models
{
    [Table("form_requests")]
    public class FormRequest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("description", TypeName = "nvarchar(max)")]
        public string Description { get; set; }

        [Column("confirm_by")]
        public int? ConfirmBy { get; set; }
        [Column("total_day_leave")]
        public decimal TotalDayLeave { get; set; }

        [Column("confirm_at")]
        public DateTime? ConfirmAt { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; }

        [Required]
        [Column("type")]
        public int Type { get; set; }

        [Column("note", TypeName = "nvarchar(max)")]
        public string Note { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
    public class LeaveRequestDto
    {
        public DateTime Date { get; set; }
        public int IsPaidLeave { get; set; }
        public decimal DurationLeave { get; set; }
    }
}
