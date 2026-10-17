using System.ComponentModel.DataAnnotations;

namespace JtechnApi.Departments.Dtos
{
    public class DepartmentInput
    {
        [Required(ErrorMessage = "Vui lòng nhập mã bộ phận."), StringLength(50)]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9_-]*$", ErrorMessage = "Mã chỉ gồm chữ không dấu, số, dấu gạch ngang và gạch dưới; bắt đầu bằng chữ hoặc số.")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên bộ phận."), StringLength(150)]
        public string Name { get; set; }
        [StringLength(1000)]
        public string Description { get; set; }
    }
}
