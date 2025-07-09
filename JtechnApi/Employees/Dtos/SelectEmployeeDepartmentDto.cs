using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Employees.Dtos
{
    public class SelectEmployeeDepartmentDto
    {
        public int Id { get; set; }
        public int Employee_id { get; set; }
        public int Department_id { get; set; }
        public string Permissions { get; set; }

    }
}
