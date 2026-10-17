using System.ComponentModel.DataAnnotations;

namespace JtechnApi.Employees.Dtos
{
    public class EmployeeDepartmentInput
    {
        [Required, MaxLength(1000)]
        public int[] DepartmentIds { get; set; }
    }
}
