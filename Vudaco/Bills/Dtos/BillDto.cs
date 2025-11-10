using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Bills.Dtos
{
    public class BillDto
    {
        public int Id { get; set; }
        public string BillCode { get; set; }
        public int? FileInfoId { get; set; }
        public int StorageId { get; set; }
        public int? CustomerDetailId { get; set; }
        public int? SupplierDetailId { get; set; }
        public DateTime AccountingDate { get; set; }
        public string Name { get; set; }
        public int CycleName { get; set; }
        public int? Status { get; set; }
        public int? ApprovedByUser { get; set; }
        public DateTime? ApprovalTime { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
