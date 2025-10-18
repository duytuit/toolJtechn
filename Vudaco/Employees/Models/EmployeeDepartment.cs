using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Employees.Models
{
    [Table("employee_department")]
    public class EmployeeDepartment
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Required]
        [Column("department_id")]
        public int DepartmentId { get; set; }

        [Required]
        [Column("positions")]
        public int Positions { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("unit_id")]
        public int? UnitId { get; set; }

        [Column("dept_id")]
        public int? DeptId { get; set; }

        [Column("team_id")]
        public int? TeamId { get; set; }

        [Column("process_id")]
        public int? ProcessId { get; set; }

        [MaxLength(191)]
        [Column("permissions")]
        public string Permissions { get; set; }
    }
}
