using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JtechnApi.Requireds.Models;

namespace JtechnApi.Accessorys.Models
{
    [Table("accessories")]
    public class Accessory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }
        [Column("location_k")]
        public string Location_k { get; set; }
        [Column("location_c")]
        public string Location_c { get; set; }
        [Column("location")]
        public string Location { get; set; }
        [Column("material_norms")]
        public int? Material_norms { get; set; }
        [Column("image")]
        public string Image { get; set; }
        [Column("status")]
        public int? Status { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }
        [Column("created_at")]
        public DateTime? Created_at { get; set; }
        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }
        [Column("unit")]
        public string Unit { get; set; }
        [Column("inventory")]
        public double Inventory { get; set; }
        [Column("accessory_dept")]
        public string Accessory_dept { get; set; }
        [Column("invoice_data")]
        public string Invoice_data { get; set; }
        [Column("type")]
        public int? Type { get; set; }
        [Column("note_type")]
        public string Note_type { get; set; }
    }
}
