using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Umesens.Models
{
    [Table("umesens")]
    public class Umesen
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("detach_content")]
        public string Detach_content { get; set; }

        [Column("code")]
        public string Code { get; set; }
        [Column("lot")]
        public string Lot { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("accessory")]
        public string Accessory { get; set; }
        [Column("code_by")]
        public string Code_by { get; set; }

        [Column("ok")]
        public int Ok { get; set; }

        [Column("ng")]
        public int Ng { get; set; }
        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("date")]
        public DateTime? Date { get; set; }

        [Column("location_quantity")]
        public int Location_quantity { get; set; }
        [Column("hat")]
        public int Hat { get; set; }

        [Column("status_ng")]
        public int Status_ng { get; set; }

        [Column("image")]
        public string Image { get; set; }

        [Column("created_at")]
        public DateTime? Created_at { get; set; }
        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }

    }
}
