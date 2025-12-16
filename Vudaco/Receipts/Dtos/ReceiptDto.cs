using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Receipts.Dtos
{
    public class ReceiptDto
    {
        public int Id { get; set; }
        public string CodeReceipt { get; set; }
        public int StorageId { get; set; }
        public int? PartnerDetailId { get; set; }
        public int? FileInfoId { get; set; }
        public int? Object { get; set; }
        public int? ObjectId { get; set; }
        public int? EmployeeId { get; set; }
        public int? Type { get; set; }
        public DateTime AccountingDate { get; set; }
        public int? FundId { get; set; }
        public int? IncomeExpenseCategoryId { get; set; }
        public string Bill { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
        public int FormOfPayment { get; set; }
        public int TypeReceipt { get; set; }
        public int? BankId { get; set; }
        public int? Status { get; set; }
        public int Amount { get; set; }
        public int Vat { get; set; }
        public string Data { get; set; }
        public string Debits { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public ChuyenTienNoiBo? ChuyenTu { get; set; }
        public ChuyenTienNoiBo? ChuyenDen { get; set; }
    }
    public class ChuyenTienNoiBo
    {
        public int? BankId { get; set; }
        public int FormOfPayment { get; set; }
        public int? FundId { get; set; }
    }
}
