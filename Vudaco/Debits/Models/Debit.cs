using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Vudaco.ContractFiles.Models;
using Vudaco.Receipts.Models;

namespace Vudaco.Debits.Models
{
    [Table("debits")]
    public class Debit
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("bill_id")]
        public int? BillId { get; set; }
        [Column("supplier_bill_id")]
        public int? SupplierBillId { get; set; }

        [Column("vehicle_id")]
        public int? VehicleId { get; set; }

        [Column("customer_detail_id")]
        public int? CustomerDetailId { get; set; }
        [Column("supplier_detail_id")]
        public int? SupplierDetailId { get; set; }

        [Column("employee_staff_id")]
        public int? EmployeeStaffId { get; set; }

        [Column("employee_driver_id")]
        public int? EmployeeDriverId { get; set; }

        [Column("file_info_id")]
        public int? FileInfoId { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [Column("type")]
        public int Type { get; set; }
        [Column("service_id")]
        public int? ServiceId { get; set; }
        [Column("service_detail")]
        public string ServiceDetail { get; set; }

        [MaxLength(255)]
        [Column("dispatch_code")]
        public string DispatchCode { get; set; }
        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; }
        [Required]
        [Column("service_date", TypeName = "date")]
        public DateTime ServiceDate { get; set; }
        [Required]
        [Column("accounting_date", TypeName = "date")]
        public DateTime AccountingDate { get; set; }
        [Column("purchase_accounting_date", TypeName = "date")]
        public DateTime? PurchaseAccountingDate { get; set; }
        [Required]
        [Column("purchase_price")]
        public int PurchasePrice { get; set; }

        [Required]
        [Column("price")]
        public int Price { get; set; }

        [Required]
        [Column("vat")]
        public int Vat { get; set; }
        [Required]
        [Column("purchase_vat")]
        public int PurchaseVat { get; set; }

        [Column("purchase_com")]
        public int PurchaseCom { get; set; }
        [Column("price_com")]
        public int PriceCom { get; set; }
        [Column("driver_fee")]
        public int? DriverFee { get; set; }

        [Column("meal_fee")]
        public int? MealFee { get; set; }

        [Column("ticket_fee")]
        public int? TicketFee {get; set; }

        [Column("overnight_fee")]
        public int? OvernightFee { get; set; }

        [Column("penalty_fee")]
        public int? PenaltyFee { get; set; }

        [Column("goods_fee")]
        public int? GoodsFee { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; }

        [Required]
        [Column("purchase_status")]
        public int PurchaseStatus { get; set; }
        [Required]
        [Column("service_status")]
        public int ServiceStatus { get; set; }
        [Required]
        [Column("driver_status")]
        public int DriverStatus { get; set; }

        [Column("data")]
        public string Data { get; set; }

        [Column("note")]
        public string Note { get; set; }
        [Column("purchase_note")]
        public string PurchaseNote { get; set; }
        [Column("bill")]
        public string Bill { get; set; }
        [Column("purchase_bill")]
        public string PurchaseBill { get; set; }
        [Column("link_bill")]
        public string LinkBill { get; set; }
        [Column("code_bill")]
        public string CodeBill { get; set; }

        [MaxLength(50)]
        [Column("customer_vehicle_type")]
        public string CustomerVehicleType { get; set; }

        [MaxLength(50)]
        [Column("supplier_vehicle_type")]
        public string SupplierVehicleType { get; set; }

        [Column("approved_by_user")]
        public int? ApprovedByUser { get; set; }

        [Column("approval_time")]
        public DateTime? ApprovalTime { get; set; }
        [Column("vehicle_number")]
        public string VehicleNumber { get; set; }
        [Column("cus_bill")]
        public string CusBill { get; set; }
        [Column("cus_bill_date", TypeName = "date")]
        public DateTime? CusBillDate { get; set; }
         [Column("sup_bill")]
        public string SupBill { get; set; }
        [Column("sup_bill_date", TypeName = "date")]
        public DateTime? SupBillDate { get; set; }

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
        
        [NotMapped]
        public FileInfo FileInfo { get; set; }
        [NotMapped]
        public Receipt Receipt { get; set; }

    }
}
