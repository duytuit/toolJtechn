using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.Employees.Models
{
    [Table("employee")]
    public class Employee
    {
        [Key]
        [Column("Id", TypeName = "int")]
        public int Id { get; set; }

        [Column("EmployeeCode", TypeName = "varchar")]
        public string EmployeeCode { get; set; }

        [Column("Username", TypeName = "varchar")]
        public string Username { get; set; }

        [Column("FullName", TypeName = "nvarchar")]
        public string FullName { get; set; }

        [Column("PasswordHash", TypeName = "varchar")]
        public string PasswordHash { get; set; }

        [Column("Email", TypeName = "varchar")]
        public string Email { get; set; }

        [Column("Phone", TypeName = "varchar")]
        public string Phone { get; set; }

        [Column("IsActive", TypeName = "bit")]
        public bool IsActive { get; set; }

        [Column("created_at", TypeName = "datetime2")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at", TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at", TypeName = "datetime2")]
        public DateTime? DeletedAt { get; set; }

        [Column("Department", TypeName = "nvarchar")]
        public string Department { get; set; }

        [Column("DateOfBirth", TypeName = "date")]
        public DateTime? DateOfBirth { get; set; }

        [Column("Gender", TypeName = "nvarchar")]
        public string Gender { get; set; }

        [Column("Position", TypeName = "nvarchar")]
        public string Position { get; set; }

        [Column("JoinDate", TypeName = "datetime")]
        public DateTime? JoinDate { get; set; }

        [Column("ProbationEndDate", TypeName = "datetime")]
        public DateTime? ProbationEndDate { get; set; }

        [Column("LeaveDate", TypeName = "datetime")]
        public DateTime? LeaveDate { get; set; }
    }
}
