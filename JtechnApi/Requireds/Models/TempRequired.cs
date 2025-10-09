using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.Requireds.Models
{
    [Table("temp_requireds")]
    public class TempRequired
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("msp")]
        public string Msp { get; set; }

        [Column("code_nv")]
        public string Code_nv { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [Column("created_at")]
        public DateTime? Created_at { get; set; }

        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }

        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }

    }
}
