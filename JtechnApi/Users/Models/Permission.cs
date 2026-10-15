using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.Users.Models
{
    [Table("permission_web")]
    public class Permission
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("code", TypeName = "varchar(100)")]
        public string Code { get; set; }

        [Required]
        [Column("name", TypeName = "nvarchar(150)")]
        public string Name { get; set; }

        [Required]
        [Column("category", TypeName = "nvarchar(100)")]
        public string Category { get; set; }

        [Column("description", TypeName = "nvarchar(500)")]
        public string Description { get; set; }

        [Column("is_active", TypeName = "bit")]
        public bool IsActive { get; set; }

        [Column("created_at", TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; }
    }
}
