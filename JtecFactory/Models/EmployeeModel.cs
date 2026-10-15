using System;

namespace JtecFactory.Models
{
    public class EmployeeModel : ISoftDeletable
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
         // Employee information
        public string Department { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Position { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
        public DateTime? LeaveDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }
}
