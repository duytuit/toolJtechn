using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vudaco.Debits.Models;

namespace Vudaco.Receipts.Models
{
    [Table("off_sets")]
    public class Offset
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("a_receipt_id")]
        public int? AReceiptId { get; set; }
        [Column("b_receipt_id")]
        public int? BReceiptId { get; set; }
        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }
        [Column("accounting_date")]
        public DateTime AccountingDate { get; set; }
        [Column("price")]
        public int Price { get; set; }
        [Column("note")]
        public string Note { get; set; }
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
