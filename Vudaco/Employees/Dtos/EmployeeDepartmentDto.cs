using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Employees.Models;

namespace Vudaco.Employees.Dtos
{
    public class EmployeeDepartmentDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public int Positions { get; set; }
        public int StorageId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? UnitId { get; set; }
        public int? DeptId { get; set; }
        public int? TeamId { get; set; }
        public int? ProcessId { get; set; }
        public string Permissions { get; set; }
    }
}
