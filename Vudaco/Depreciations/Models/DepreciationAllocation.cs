using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Depreciations.Models
{
    [Table("depreciation_allocations")]
    public class DepreciationAllocation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [MaxLength(50)]
        [Column("code")]
        public string Code { get; set; }

        [Column("type")]
        [Required]
        public int Type { get; set; }

        [MaxLength(1000)]
        [Column("description")]
        public string Description { get; set; }

        [MaxLength(1000)]
        [Column("note")]
        public string Note { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

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
}
