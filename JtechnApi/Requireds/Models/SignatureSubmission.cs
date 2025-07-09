using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace JtechnApi.Requireds.Models
{
    [Table("signature_submissions")]
    public class SignatureSubmission
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("required_id")]
        public int Required_id { get; set; }
        [Column("department_id")]
        public int Department_id { get; set; }
        [Column("content")]
        public string Content { get; set; }
        [Column("positions")]
        public int? Positions { get; set; }
        [Column("approve_id")]
        public string Approve_id { get; set; }
        [Column("signature_id")]
        public int? Signature_id { get; set; }
        [Column("status")]
        public int? Status { get; set; }
        [Column("type")]
        public int? Type { get; set; }
        [Column("created_at")]
        public DateTime? Created_at { get; set; }
        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }

    }
}
