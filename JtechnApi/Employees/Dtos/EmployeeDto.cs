
using System;
using System.ComponentModel.DataAnnotations;

namespace JtechnApi.Employees.Dtos
{
    public class EmployeeDto
    {
        [Required]
        public string EmployeeCode { get; set; }

        public string Username { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Password { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; } = true;
        public string Department { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Position { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? LeaveDate { get; set; }
    }
}
