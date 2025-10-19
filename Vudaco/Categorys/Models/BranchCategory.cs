using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Categorys.Models
{
    [Table("branch_categorys")]
    public class BranchCategory
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("branch_code")]
        public string BranchCode { get; set; }

        [Required]
        [MaxLength(191)]
        [Column("branch_name")]
        public string BranchName { get; set; }

        [MaxLength(50)]
        [Column("parent_code")]
        public string ParentCode { get; set; }

        [Column("storage_id")]
        public int? StorageId { get; set; }

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
