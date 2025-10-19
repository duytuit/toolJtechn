using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.ContractFiles.Models
{
    [Table("file_infos")] // đổi tên bảng thực tế
    public class FileInfo
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("partner_detail_id")]
        public int? PartnerDetailId { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("file_number")]
        public string FileNumber { get; set; }

        [MaxLength(50)]
        [Column("declaration")]
        public string Declaration { get; set; }

        [MaxLength(50)]
        [Column("bill")]
        public string Bill { get; set; }

        [MaxLength(50)]
        [Column("quantity")]
        public string Quantity { get; set; }

        [MaxLength(50)]
        [Column("container_code")]
        public string ContainerCode { get; set; }

        [Column("sales_id")]
        public int? SalesId { get; set; }

        [Column("type")]
        public int? Type { get; set; }

        [Column("feature")]
        public int? Feature { get; set; }

        [Column("declaration_quantity")]
        public int? DeclarationQuantity { get; set; }

        [Column("declaration_type")]
        public int? DeclarationType { get; set; }

        [Column("business")]
        public int? Business { get; set; }

        [Column("occurrence")]
        public int? Occurrence { get; set; }

        [Column("note", TypeName = "nvarchar(max)")]
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
