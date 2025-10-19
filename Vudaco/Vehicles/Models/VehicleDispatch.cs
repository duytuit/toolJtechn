using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Vehicles.Models
{
    [Table("vehicle_dispatchs")]
    public class VehicleDispatch
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("dispatch_code")]
        public string DispatchCode { get; set; }

        [Column("file_info_id")]
        public int? FileInfoId { get; set; }

        [Required]
        [Column("vehicle_id")]
        public int VehicleId { get; set; }

        [Column("partner_detail_id")]
        public int? PartnerDetailId { get; set; }

        [Column("storage_id")]
        public int? StorageId { get; set; }

        [Required]
        [Column("accounting_date")]
        public DateTime AccountingDate { get; set; }

        [Required]
        [MaxLength(191)]
        [Column("route")]
        public string Route { get; set; }

        [Column("driver_fee")]
        public int? DriverFee { get; set; }

        [Column("meal_fee")]
        public int? MealFee { get; set; }

        [Column("ticket_fee")]
        public int? TicketFee { get; set; }

        [Column("overnight_fee")]
        public int? OvernightFee { get; set; }

        [Column("penalty_fee")]
        public int? PenaltyFee { get; set; }

        [Column("goods_fee")]
        public int? GoodsFee { get; set; }

        [Column("approved")]
        public int? Approved { get; set; }

        [MaxLength(191)]
        [Column("approved_by_user")]
        public string ApprovedByUser { get; set; }

        [MaxLength(50)]
        [Column("file_number")]
        public string FileNumber { get; set; }

        [Column("note", TypeName = "nvarchar(max)")]
        public string Note { get; set; }

        [Column("approval_time")]
        public DateTime? ApprovalTime { get; set; }

        [MaxLength(191)]
        [Column("delivery_point")]
        public string DeliveryPoint { get; set; }

        [MaxLength(50)]
        [Column("customer_vehicle_type")]
        public string CustomerVehicleType { get; set; }

        [MaxLength(50)]
        [Column("supplier_vehicle_type")]
        public string SupplierVehicleType { get; set; }

        [Column("customs_status")]
        public int? CustomsStatus { get; set; }

        [Required]
        [Column("selling_price")]
        public int SellingPrice { get; set; }

        [Required]
        [Column("purchase_price")]
        public int PurchasePrice { get; set; }

        [MaxLength(50)]
        [Column("vehicle_number")]
        public string VehicleNumber { get; set; }

        [MaxLength(191)]
        [Column("driver_name")]
        public string DriverName { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
