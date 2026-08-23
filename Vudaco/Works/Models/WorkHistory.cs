using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Works.Models
{
    [Table("work_histories")]
    public class WorkHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("type")]
        public int Type { get; set; }

        [Column("content", TypeName = "nvarchar(max)")]
        public string Content { get; set; }

        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [Column("model_id")]
        public int ModelId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("model")]
        public string Model { get; set; }

        [Required]
        [Column("action")]
        public int Action { get; set; }

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