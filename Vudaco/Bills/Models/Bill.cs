using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Bills.Models
{
    [Table("bills")]
    public class Bill
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [MaxLength(255)]
        [Column("bill_code")]
        public string BillCode { get; set; }

        [Column("file_info_id")]
        public int? FileInfoId { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Column("customer_detail_id")]
        public int? CustomerDetailId { get; set; }
        [Column("supplier_detail_id")]
        public int? SupplierDetailId { get; set; }

        [Required]
        [Column("accounting_date")]
        public DateTime AccountingDate { get; set; }

        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("cycle_name")]
        public int CycleName { get; set; }

        [Column("status")]
        public int? Status { get; set; }

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
