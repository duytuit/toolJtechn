using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Comments.Models
{
    [Table("comments")]
    public class Comment
    {
       [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [Column("post_id")]
        public int PostId { get; set; }

        [Required]
        [Column("type")]
        public int Type { get; set; }

        [Column("message", TypeName = "nvarchar(max)")]
        public string? Message { get; set; }

        [Column("attach", TypeName = "nvarchar(max)")]
        public string? Attach { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Required]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
    public class AttachmentInfo
    {
        public string[] Files { get; set; }
        public string[] Images { get; set; }
    }
}
