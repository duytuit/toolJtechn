using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Receipts.Dtos
{
    public class ReceiptHoanUngGiaoNhanDto
    {
        public int? EmployeeId { get; set; }
        public int FormOfPayment { get; set; }
        public int TypeReceipt { get; set; }
        public int? FundId { get; set; }
        public int? BankId { get; set; }
        public int StorageId { get; set; }
        public DateTime AccountingDate { get; set; }
        public int Amount { get; set; }
        public int Vat { get; set; }
        public string Bill { get; set; }
        public string Note { get; set; }
        public string Description { get; set; }
        public string Data { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
