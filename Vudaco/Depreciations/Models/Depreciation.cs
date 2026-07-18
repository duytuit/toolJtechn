using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Depreciations.Models
{
    [Table("depreciations")]
    public class Depreciation
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [MaxLength(50)]
        [Column("code_number")]
        public string CodeNumber { get; set; }

        [MaxLength(200)]
        [Column("name")]
        public string Name { get; set; }

        [Column("original_cost")]
        public int? OriginalCost { get; set; }

        [Column("useful_life")]
        public int? UsefulLife { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("monthly_depreciation")]
        public int? MonthlyDepreciation { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [Column("type")]
        public int Type { get; set; }
        [Column("vehicle_id")]
        public int? VehicleId { get; set; }

        [Column("create_date")]
        public DateTime CreateDate { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; }

        [Column("note")]
        public string Note { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}
