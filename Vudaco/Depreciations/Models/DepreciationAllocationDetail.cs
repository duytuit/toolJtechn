using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Depreciations.Models
{
    [Table("depreciation_allocation_details")]
    public class DepreciationAllocationDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("depreciation_id")]
        public int? DepreciationId { get; set; }

        [Column("depreciation_allocation_id")]
        public int? DepreciationAllocationId { get; set; }

        [Required]
        [Column("monthly_depreciation")]
        public int MonthlyDepreciation { get; set; }

        [Column("storage_id")]
        public int? StorageId { get; set; }

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
