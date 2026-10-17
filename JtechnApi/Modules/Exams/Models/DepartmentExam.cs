using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.Exams.Models
{
    [Table("department_exam", Schema = "dbo")]
    public class DepartmentExam
    {
        [Key, Column("id")] public int Id { get; set; }
        [Column("department_id")] public int DepartmentId { get; set; }
        [Required, MaxLength(50), Column("code", TypeName = "varchar(50)")] public string Code { get; set; }
        [Required, MaxLength(200), Column("name")] public string Name { get; set; }
        [MaxLength(1000), Column("description")] public string Description { get; set; }
        [Column("minutes")] public int Minutes { get; set; }
        [Column("definition_json")] public string DefinitionJson { get; set; }
        [Column("is_active")] public bool IsActive { get; set; }
        [Column("created_at")] public DateTime CreatedAt { get; set; }
        [Column("updated_at")] public DateTime? UpdatedAt { get; set; }
        [Column("deleted_at")] public DateTime? DeletedAt { get; set; }
    }
}
