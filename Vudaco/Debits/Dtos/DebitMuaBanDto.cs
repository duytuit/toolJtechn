using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Debits.Dtos
{
    public class DebitMuaBanDto
    {
        public int Id { get; set; }
        public int? CustomerDetailId { get; set; }
        public int? SupplierDetailId { get; set; }
        public DateTime AccountingDate { get; set; }
        public int StorageId { get; set; }
        public int Type { get; set; }
        public string DispatchCode { get; set; }
        public string Name { get; set; } = null!;
        public int PurchaseVat { get; set; }
        public string PurchaseNote { get; set; }
        public int PurchasePrice { get; set; }      // Cước mua
        public int Price { get; set; }
        public int Vat { get; set; }
        public string Data { get; set; }
        public int? Status { get; set; }
        public int StatusConfirm { get; set; }
        public string Note { get; set; }
        public int? FundId { get; set; }
        public int? IncomeExpenseCategoryId { get; set; }
        public int FormOfPayment { get; set; }
        public int TypeReceipt { get; set; }
        public int? BankId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
