using System;

namespace JtechnApi.Employees.Dtos
{
    public class EmployeeResponseDto
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsActive { get; set; }
        public string Status => IsActive ? "Đang làm việc" : "Ngừng hoạt động";
        public string Department { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Position { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
