using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.Users.Models
{
    [Table("role_web")]
    public class Role
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("code", TypeName = "varchar(50)")]
        public string Code { get; set; }

        [Required]
        [Column("name", TypeName = "nvarchar(100)")]
        public string Name { get; set; }

        [Column("description", TypeName = "nvarchar(500)")]
        public string Description { get; set; }

        [Column("is_system", TypeName = "bit")]
        public bool IsSystem { get; set; }

        [Column("is_active", TypeName = "bit")]
        public bool IsActive { get; set; }

        [Column("created_at", TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at", TypeName = "datetime2")]
        public DateTime? UpdatedAt { get; set; }
    }
}
