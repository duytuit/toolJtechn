using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Works.Models
{
    [Table("works")]
    public class Work
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Column("description", TypeName = "nvarchar(max)")]
        public string Description { get; set; }

        [Required]
        [Column("type")]
        public int Type { get; set; }
        
        [Column("group")]
        public bool Group { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Column("storage_id")]
        public int StorageId { get; set; }

        [Column("status")]
        public int Status { get; set; }

        [Column("assignee_ids")]
        public string AssigneeIds { get; set; }
        [Column("attachments")]
        public string Attachments { get; set; }

        [Column("due_date")]
        public DateTime? DueDate { get; set; }

        [Column("completed_date")]
        public DateTime? CompletedDate { get; set; }

        [Column("priority")]
        public int Priority { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}