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
        public int? EmployeeId { get; set; }
        public int? Type { get; set; }
        public DateTime? AccountingDate { get; set; }
        public string CodeFund { get; set; }
        public string Code { get; set; }
        public string Bill { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
        public int FormOfPayment { get; set; }
        public int TypeReceipt { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string AccountHolder { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
