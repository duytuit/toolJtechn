
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using JtechnApi.Employees.Models;
using JtechnApi.Requireds.Models;

namespace JtechnApi.Employees.Dtos
{
    public class SelectEmployeeDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string First_name { get; set; }
        public string Last_name { get; set; }
        public ICollection<EmployeeDepartment> EmployeeDepartments { get; set; }
        [NotMapped]
        public SelectEmployeeDepartmentDto SelectEmployeeDepartmentDto { get; set; }
        public ICollection<Required> Requireds { get; set; }

    }
}
