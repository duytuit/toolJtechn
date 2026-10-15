using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.Users.Models
{
    [Table("role_employee_web")]
    public class RoleEmployee
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("created_at", TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }
    }
}
