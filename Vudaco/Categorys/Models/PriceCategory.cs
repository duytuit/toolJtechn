using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Categorys.Models
{
    [Table("price_categorys")]
    public class PriceCategory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Column("partner_detail_id")]
        public int? PartnerDetailId { get; set; }

        [Required]
        [Column("selling_price")]
        public int SellingPrice { get; set; }

        [Required]
        [Column("purchase_price")]
        public int PurchasePrice { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; }

        [Required]
        [Column("type")]
        public int Type { get; set; }

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
