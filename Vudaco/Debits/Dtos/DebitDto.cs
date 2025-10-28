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
        public int? VehicleId { get; set; }
        public int? EmployeeStaffId { get; set; }
        public int? EmployeeDriverId { get; set; }
        public int? PartnerDetailId { get; set; }
        public int? SupplierPartnerDetailId { get; set; }
        public int? FileInfoId { get; set; }
        public int StorageId { get; set; }
        public int Type { get; set; }
        public string DispatchCode { get; set; }
        public string Name { get; set; } = null!;
        public DateTime AccountingDate { get; set; }
        public int PurchasePrice { get; set; }      // Cước mua
        public int Price { get; set; }
        public int Vat { get; set; }
        public int? DriverFee { get; set; }         // lái xe thu cước
        public int? MealFee { get; set; }           // tiền ăn
        public int? TicketFee { get; set; }         // tiền vé
        public int? OvernightFee { get; set; }      // tiền ăn đêm
        public int? PenaltyFee { get; set; }        // tiền luật
        public int? GoodsFee { get; set; }          // lượng hàng về
        public int Status { get; set; }
        public string Data { get; set; }
        public string Note { get; set; }
        public string CustomerVehicleType { get; set; }
        public string SupplierVehicleType { get; set; }
        public int? ApprovedByUser { get; set; }
        public DateTime? ApprovalTime { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Route { get; set; }
    }
}
