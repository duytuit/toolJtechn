using System.ComponentModel.DataAnnotations;

namespace JtechnApi.Exams.Dtos
{
    public class DepartmentExamInput
    {
        [Range(1, int.MaxValue)] public int DepartmentId { get; set; }
        [Required, StringLength(50), RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9_-]*$")]
        public string Code { get; set; }
        [Required, StringLength(200)] public string Name { get; set; }
        [StringLength(1000)] public string Description { get; set; }
        [Range(1, 480)] public int Minutes { get; set; } = 30;
        [Required, StringLength(1000000)] public string DefinitionJson { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
