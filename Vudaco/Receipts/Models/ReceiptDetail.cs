using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Receipts.Models
{
    [Table("receipt_details")]
    public class ReceiptDetail
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("receipt_id")]
        public int ReceiptId { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Column("debit_id")]
        public int? DebitId { get; set; }

        [Required]
        [Column("accounting_date")]
        public DateTime AccountingDate { get; set; }

        [Required]
        [Column("amount")]
        public int Amount { get; set; }

        [Required]
        [Column("vat")]
        public int Vat { get; set; }

        [Column("note", TypeName = "nvarchar(max)")]
        public string Note { get; set; }
        [Column("data", TypeName = "nvarchar(max)")]
        public string Data { get; set; }

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
