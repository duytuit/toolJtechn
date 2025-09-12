using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JtechnApi.Requireds.Models;

namespace JtechnApi.UploadKTNQ.Models
{
    [Table("upload_ktnq")]
    public class KTNQ
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("code")]
        public string Code { get; set; }
        [Column("content")]
        public string Content { get; set; }
        [Column("created_by")]
        public int? Created_by { get; set; }
        [Column("updated_by")]
        public int? Updated_by { get; set; }
        [Column("deleted_by")]
        public int? Deleted_by { get; set; }
        [Column("created_at")]
        public DateTime? Created_at { get; set; }
        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }
    }
}
