using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.UploadDatas.Models
{
    [Table("upload_data")]
    public class UploadData
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("url")]
        public string Url { get; set; }

        [Column("type")]
        public string Type { get; set; }

        [Column("version")]
        public int? Version { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [Column("image_connector")]
        public string Image_connector { get; set; }

        [Column("image_connector_by")]
        public string Image_connector_by { get; set; }

        [Column("image_connector_updated_at")]
        public DateTime? Image_connector_updated_at { get; set; }

        [Column("cutting_wire")]
        public int? Cutting_wire { get; set; }

        [Column("date_status_connector")]
        public DateTime? Date_status_connector { get; set; }

        [Column("created_at")]
        public DateTime? Created_at { get; set; }
        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }

        [Column("created_by")]
        public string Created_by { get; set; }
        [Column("updated_by")]
        public string Updated_by { get; set; }


    }
}
