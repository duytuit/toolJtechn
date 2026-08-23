using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Works.Models
{
    [Table("work_files")]
    public class WorkFile
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [MaxLength(1000)]
        [Column("path")]
        public string Path { get; set; }

        [MaxLength(100)]
        [Column("type")]
        public string Type { get; set; }

        [MaxLength(100)]
        [Column("model")]
        public string Model { get; set; }

        [Column("size")]
        public long? Size { get; set; }

        [Column("model_id")]
        public int ModelId { get; set; }

        [Column("storage_id")]
        public int StorageId { get; set; }

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