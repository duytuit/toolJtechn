using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Vehicles.Models
{
    [Table("vehicles")]
    public class Vehicle
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("number_code")]
        public string NumberCode { get; set; }

        [Required]
        [Column("is_external_driver")]
        public bool IsExternalDriver { get; set; }

        [MaxLength(191)]
        [Column("note")]
        public string Note { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

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
