using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Employees.Models
{
    [Table("employee_departments")]
    public class EmployeeDepartment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("employee_id")]
        public int? Employee_id { get; set; }
        [Column("department_id")]
        public int? Department_id { get; set; }
        [Column("positions")]
        public int? Positions { get; set; }

        [Column("unit_id")]
        public int? Unit_id { get; set; }
        [Column("dept_id")]
        public int? Dept_id { get; set; }
        [Column("team_id")]
        public int? Team_id { get; set; }
        [Column("process_id")]
        public int? Process_id { get; set; }

        [Column("permissions")]
        public string permissions { get; set; }

        [Column("created_by")]
        public int? Created_by { get; set; }
        [Column("updated_by")]
        public int? Updated_by { get; set; }
        [Column("deleted_by")]
        public int? Deleted_by { get; set; }
        [Column("created_at")]
        public DateTime? Created_at { get; set; }
        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }
        public Employee Employee { get; set; }

    }
}
