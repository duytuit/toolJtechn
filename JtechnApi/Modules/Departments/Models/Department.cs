using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.Departments.Models
{
    [Table("department", Schema = "dbo")]
    public class Department
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Required, MaxLength(50), Column("code", TypeName = "varchar(50)")]
        public string Code { get; set; }

        [Required, MaxLength(150), Column("name", TypeName = "nvarchar(150)")]
        public string Name { get; set; }

        [MaxLength(1000), Column("description", TypeName = "nvarchar(1000)")]
        public string Description { get; set; }

        [Column("created_at", TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at", TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at", TypeName = "datetime2")]
        public DateTime? DeletedAt { get; set; }

    }
}
