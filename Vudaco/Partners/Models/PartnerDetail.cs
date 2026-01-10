using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Partners.Models
{
    [Table("partner_details")]
    public class PartnerDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Required]
        [Column("partner_id")]
        public int PartnerId { get; set; }
        [Required]
        [Column("status")]
        public int Status { get; set; }
        [Required]
        [Column("customer_credit_limit")]
        public int CustomerCreditLimit { get; set; }
        [Required]
        [Column("supplier_credit_limit")]
        public int SupplierCreditLimit { get; set; }
        [MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } 
        [MaxLength(191)]
        [Column("note")]
        public string Note { get; set; }
        [Required]
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
