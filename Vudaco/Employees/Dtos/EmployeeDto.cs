using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Employees.Models;

namespace Vudaco.Employees.Dtos
{
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string Username  { get; set; }
        public string  Password  { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IdentityCard { get; set; }
        public string NativeLand { get; set; }
        public string Addresss { get; set; }
        public DateTime? Birthday { get; set; }
        public int? Status { get; set; }
        public int? Marital { get; set; }
        public int? Worker { get; set; }
        public int? Positions { get; set; }
        public DateTime? BeginDateCompany { get; set; }
        public DateTime? EndDateCompany { get; set; }
        public int StorageId { get; set; }
        public int DepartmentId { get; set; }
        public long? CreatedBy { get; set; }
        public long? UpdatedBy { get; set; }
        public long? DeletedBy { get; set; }
        public string Avatar { get; set; }
        public int BaseSalary { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string BankNumber { get; set; }
        public string BankName { get; set; }
        public int? UserId { get; set; }
        public string StorageIds { get; set; }
        public int[] departmentIds { get; set; }
        public EmployeeDepartment? EmployeeDepartment { get; set; } = null;
    }
}
