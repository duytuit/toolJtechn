using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.ProductionPlans.Models
{
    [Table("production_plans")]
    public class ProductionPlan
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("lot_no")]
        public string Lot_no { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("note")]
        public string Note { get; set; }

        [Column("plan_lot_no")]
        public string Plan_lot_no { get; set; }

        [Column("order")]
        public int? Order { get; set; }
        [Column("kttm")]
        public string Kttm { get; set; }

        [Column("ktnq")]
        public string Ktnq { get; set; }

        [Column("cam")]
        public string Cam { get; set; }
        [Column("dap1")]
        public string Dap1 { get; set; }

        [Column("dap2")]
        public string Dap2 { get; set; }

        [Column("cat")]
        public string Cat { get; set; }
        [Column("flag_a")]
        public int? Flag_a { get; set; }

        [Column("flag_b")]
        public int? Flag_b { get; set; }
        [Column("version")]
        public int? Version { get; set; }

        [Column("ktnq_url")]
        public string Ktnq_url { get; set; }

        [Column("created_at")]
        public DateTime? Created_at { get; set; }
        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }

    }
}
