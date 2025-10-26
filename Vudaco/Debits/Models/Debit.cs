using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Debits.Models
{
    [Table("debits")]
    public class Debit
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("bill_id")]
        public int BillId { get; set; }

        [Column("vehicle_dispatch_id")]
        public int? VehicleDispatchId { get; set; }

        [Required]
        [Column("partner_detail_id")]
        public int PartnerDetailId { get; set; }
        [Required]
        [Column("supplier_partner_detail_id")]
        public int SupplierPartnerDetailId { get; set; }

        [Column("file_info_id")]
        public int? FileInfoId { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [Column("type")]
        public int Type { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("accounting_date")]
        public DateTime AccountingDate { get; set; }

        [Required]
        [Column("price")]
        public int Price { get; set; }

        [Required]
        [Column("purchase_price")]
        public int PurchasePrice { get; set; }

        [Required]
        [Column("vat")]
        public int Vat { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; }

        [Column("note", TypeName = "nvarchar(max)")]
        public string Note { get; set; }
        [Column("data", TypeName = "nvarchar(max)")]
        public string Data { get; set; }

        [Column("approved_by_user")]
        public int? ApprovedByUser { get; set; }

        [Column("approval_time")]
        public DateTime? ApprovalTime { get; set; }

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
