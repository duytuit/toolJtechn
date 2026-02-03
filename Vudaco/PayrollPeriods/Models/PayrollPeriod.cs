using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.PayrollPeriods.Models
{
    [Table("payroll_periods")]
    public class PayrollPeriod
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

        [Required]
        [Column("status")]
        public int Status { get; set; }

        [Column("note", TypeName = "nvarchar(max)")]
        public string? Note { get; set; }

        [Column("cycle_name")]
        [MaxLength(191)]
        public string? CycleName { get; set; }

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
}
