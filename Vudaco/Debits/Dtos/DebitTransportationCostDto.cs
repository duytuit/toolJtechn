using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Debits.Dtos
{
    public class DebitTransportationCostDto
    {
        public int Id { get; set; }
        public int? BillId { get; set; }
        public int? SupplierBillId { get; set; }
        public int? VehicleId { get; set; }
        public int? EmployeeStaffId { get; set; }
        public int? EmployeeDriverId { get; set; }
        public int? CustomerDetailId { get; set; }
        public int? SupplierDetailId { get; set; }
        public int? FileInfoId { get; set; }
        public int StorageId { get; set; }
        public int Type { get; set; }
        public int? ServiceId { get; set; }
        public string ServiceDetail { get; set; }
        public string DispatchCode { get; set; }
        public string Name { get; set; } = null!;
        public int PurchaseVat { get; set; }
        public TransportationCost? TransportationCost { get; set; }
        public int PurchaseStatus { get; set; }
        public int ServiceStatus { get; set; }
        public int DriverStatus { get; set; }
        public int Invoice { get; set; }
        public string PurchaseNote { get; set; }
        public string PurchaseBill { get; set; }
        public DateTime? PurchaseAccountingDate { get; set; }
        public DateTime ServiceDate { get; set; }
        public DateTime AccountingDate { get; set; }
        public int isExternalDriver { get; set; }      // loại xe 0: xe công ty, 1: xe ngoài
        public int PurchasePrice { get; set; }      // Cước mua
        public int Price { get; set; }
        public int Vat { get; set; }
        public int PurchaseCom { get; set; }
        public int PriceCom { get; set; }
        public int? DriverFee { get; set; }         // lái xe thu cước
        public int? MealFee { get; set; }           // tiền ăn
        public int? TicketFee { get; set; }         // tiền vé
        public int? OvernightFee { get; set; }      // tiền ăn đêm
        public int? PenaltyFee { get; set; }        // tiền luật
        public int? GoodsFee { get; set; }          // lượng hàng về
        public int? DeliveryPoint { get; set; }          // điểm giao hàng
        public int Status { get; set; }
        public string Data { get; set; }
        public string Bill { get; set; }
        public string LinkBill { get; set; }
        public string CodeBill { get; set; }
        public string Note { get; set; }
        public string CustomerVehicleType { get; set; }
        public string SupplierVehicleType { get; set; }
        public int? ApprovedByUser { get; set; }
        public DateTime? ApprovalTime { get; set; }
        public string VehicleNumber { get; set; }
        public string CusBill { get; set; }
        public DateTime? CusBillDate { get; set; }
        public string SupBill { get; set; }
        public DateTime? SupBillDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Route { get; set; }
        public string FileInfoIds { get; set; }
        public int ExportHasBill { get; set; }
        public int? IncomeExpenseCategoryId { get; set; }
        public int? EmployeeId { get; set; }
        public List<MuaHangNCCDto> MuaHangNCC { get; set; }
        public List<serviceDto> productChiho { get; set; }
        public List<serviceDto> productHaiquan { get; set; }
        public List<serviceDto> productNangha { get; set; }
    }
   public class TransportationCost
    {
        public int? DauDinhMuc { get; set; }
        public int? DauThucTe { get; set; }
        public int? SoPhieu { get; set; }
        public int? CayDau { get; set; }
        public int? PhatSinh { get; set; }
        public int? CaoToc { get; set; }
        public int? LuongChuyen { get; set; }
    }
}


