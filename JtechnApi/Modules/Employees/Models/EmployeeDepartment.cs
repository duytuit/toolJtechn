using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.Employees.Models
{
    [Table("employee_department", Schema = "dbo")]
    public class EmployeeDepartment
    {
        [Key, Column("id")]
        public int Id { get; set; }
        [Column("employee_id")]
        public int EmployeeId { get; set; }
        [Column("department_id")]
        public int DepartmentId { get; set; }
        [Column("created_at", TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }
        [Column("updated_at", TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }
        [Column("deleted_at", TypeName = "datetime2")]
        public DateTime? DeletedAt { get; set; }
    }
}
