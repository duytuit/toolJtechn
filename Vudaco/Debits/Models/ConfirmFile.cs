using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Debits.Models
{
    [Table("confirm_file_infos")]
    public class ConfirmFile
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("file_info_id")]
        public int? FileInfoId { get; set; }
        [Required]
        [Column("debit_id")]
        public int DebitId { get; set; }
        [Column("partner_detail_id")]
        [Required]
        public int? PartnerDetailId { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [Column("status")]
        public int? Status { get; set; }

        [Column("status_confirm")]
        public int StatusConfirm { get; set; }
       

        [MaxLength(500)]
        [Column("note")]
        public string Note { get; set; }

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
