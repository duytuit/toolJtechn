using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Works.Models
{
    [Table("work_details")]
    public class WorkDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("work_id")]
        public int WorkId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Column("description", TypeName = "nvarchar(max)")]
        public string Description { get; set; }

        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [Column("checked")]
        public bool Checked { get; set; }

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