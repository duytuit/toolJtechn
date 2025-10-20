using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Debits.Dtos
{
    public class DebitDto
    {
        public int Id { get; set; }
        public int BillId { get; set; }
        public int? VehicleDispatchId { get; set; }
        public int PartnerDetailId { get; set; }
        public int? FileInfoId { get; set; }
        public int StorageId { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public DateTime AccountingDate { get; set; }
        public int Price { get; set; }
        public int Vat { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
        public int? ApprovedByUser { get; set; }
        public DateTime? ApprovalTime { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
