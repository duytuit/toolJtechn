using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vudaco.Debits.Models;

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

        [Column("partner_detail_id")]
        public int? PartnerDetailId { get; set; }
        [Column("file_info_id")]
        public int? FileInfoId { get; set; }
         [Column("employee_id")]
        public int? EmployeeId { get; set; }
        [Column("type")]
        public int? Type { get; set; }

        [Column("accounting_date")]
        public DateTime AccountingDate { get; set; }

        [Column("fund_id")]
        public int? FundId { get; set; }

        [Column("income_expense_category_id")]
        public int? IncomeExpenseCategoryId { get; set; }

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

        [Column("bank_id")]
        public int? BankId { get; set; }
        [Column("status")]
        public int? Status { get; set; }

        [Column("data")]
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
        [NotMapped]
        public List<ReceiptDetail> ReceiptDetails { get; set; }
        
    }
}
