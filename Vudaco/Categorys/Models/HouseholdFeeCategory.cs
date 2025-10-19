using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Categorys.Models
{
    [Table("household_fee_categorys")]
    public class HouseholdFeeCategory
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("household_code")]
        public string HouseholdCode { get; set; }

        [Required]
        [MaxLength(191)]
        [Column("household_name")]
        public string HouseholdName { get; set; }

        [Column("storage_id")]
        public int? StorageId { get; set; }

        [Column("amount")]
        public double? Amount { get; set; }

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
