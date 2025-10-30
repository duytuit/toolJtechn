using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Receipts.Dtos
{
    public class ReceiptDetailDto
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public int StorageId { get; set; }
        public int? DebitId { get; set; }
        public DateTime AccountingDate { get; set; }
        public int Amount { get; set; }
        public int Vat { get; set; }
        public string Note { get; set; }
        public string Data { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
