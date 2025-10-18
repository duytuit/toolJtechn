using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Category.Models
{
    [Table("administrative_fee_category")]
    public class AdministrativeFeeCategory
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("fee_code")]
        public string FeeCode { get; set; }

        [Required]
        [MaxLength(191)]
        [Column("fee_name")]
        public string FeeName { get; set; }

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
