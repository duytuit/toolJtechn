using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Notifys.Models
{
    [Table("notifys")]
    public class Notify
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("code")]
        public string Code { get; set; }

        [Required]
        [MaxLength(191)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [Column("parent_id")]
        public int ParentId { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

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

        [Column("permissions", TypeName = "nvarchar(max)")]
        public string Permissions { get; set; }
    }
}
