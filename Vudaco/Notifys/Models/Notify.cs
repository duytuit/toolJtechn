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
        [Column("storage_id")]
        public int StorageId { get; set; }
        [Required]
        [Column("post_id")]
        public int PostId { get; set; }

        [Required]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("screen")]
        public string Screen { get; set; }
        
        [Column("title")]
        [MaxLength(191)]
        public string? Title { get; set; }

        [Column("description", TypeName = "nvarchar(max)")]
        public string? Description { get; set; }

        [Required]
        [Column("status")]
        public int Status { get; set; }

        [Required]
        [Column("type")]
        public int Type { get; set; }

        [Column("image")]
        [MaxLength(255)]
        public string? Image { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_by")]
        public int? DeletedBy { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}
