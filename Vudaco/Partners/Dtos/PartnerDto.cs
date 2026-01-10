using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Partners.Dtos
{
    public class PartnerDto
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string TaxCode { get; set; }
        public int UserId { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string BankAccount { get; set; }
        public int? AllowedDebtDays { get; set; }
        public double? MaxDebt { get; set; }
        public string Note { get; set; }
        public int Status { get; set; }
        public int StorageId { get; set; }
        public string Abbreviation { get; set; }
        public int CustomerCreditLimit { get; set; }
        public int SupplierCreditLimit { get; set; }
        public int CustomerId { get; set; }
        public int SupplierId { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
