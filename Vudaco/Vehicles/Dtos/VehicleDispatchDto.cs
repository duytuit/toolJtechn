
using System;

namespace Vudaco.Vehicles.Dtos
{
    public class VehicleDispatchDto
    {
       public int Id { get; set; }
        public string DispatchCode { get; set; }
        public int? FileInfoId { get; set; }
        public int VehicleId { get; set; }
        public int? PartnerDetailId { get; set; }
        public int? StorageId { get; set; }
        public DateTime AccountingDate { get; set; }
        public string Route { get; set; }
        public int? DriverFee { get; set; }
        public int? MealFee { get; set; }
        public int? TicketFee { get; set; }
        public int? OvernightFee { get; set; }
        public int? PenaltyFee { get; set; }
        public int? GoodsFee { get; set; }
        public int? Approved { get; set; }
        public string ApprovedByUser { get; set; }
        public string FileNumber { get; set; }
        public string Note { get; set; }
        public DateTime? ApprovalTime { get; set; }
        public string DeliveryPoint { get; set; }
        public string CustomerVehicleType { get; set; }
        public string SupplierVehicleType { get; set; }
        public int? CustomsStatus { get; set; }
        public int SellingPrice { get; set; }
        public int PurchasePrice { get; set; }
        public string VehicleNumber { get; set; }
        public string DriverName { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
    }
}
