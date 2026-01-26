using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.BorrowProducts.Models
{
    [Table("upload_data_cams")]
    public class DataSay
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("lot")]
        public string Lot { get; set; } 

        [Column("code")]
        public string Code { get; set; }   

        [Column("content")]
        public string Content { get; set; }

        [Column("type")]
        public int Type { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("note")]
        public string Note { get; set; }

        [Column("user_by")]
        public string UserBy { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("updated_by")]
        public int? UpdatedBy { get; set; }
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

    }
}
