using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Receipts.Models
{
    [Table("receipts")]
    public class Receipt
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("code_receipt")]
        public string CodeReceipt { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [Column("partner_detail_id")]
        public int PartnerDetailId { get; set; }

        [Column("accounting_date")]
        public DateTime? AccountingDate { get; set; }

        [MaxLength(50)]
        [Column("code_fund")]
        public string CodeFund { get; set; }

        [MaxLength(50)]
        [Column("code")]
        public string Code { get; set; }

        [MaxLength(50)]
        [Column("bill")]
        public string Bill { get; set; }

        [Column("note", TypeName = "nvarchar(max)")]
        public string Note { get; set; }

        [Column("description", TypeName = "nvarchar(max)")]
        public string Description { get; set; }

        [Required]
        [Column("form_of_payment")]
        public int FormOfPayment { get; set; }

        [Required]
        [Column("type_receipt")]
        public int TypeReceipt { get; set; }

        [MaxLength(191)]
        [Column("account_number")]
        public string AccountNumber { get; set; }

        [MaxLength(191)]
        [Column("bank_name")]
        public string BankName { get; set; }

        [MaxLength(191)]
        [Column("branch_name")]
        public string BranchName { get; set; }

        [MaxLength(191)]
        [Column("account_holder")]
        public string AccountHolder { get; set; }

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
