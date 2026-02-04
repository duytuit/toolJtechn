using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.SendMails.Models
{
    [Table("email_templates")]
    public class EmailTemplate
    {
        [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("storage_id")]
    public int StorageId { get; set; }

    [Required]
    [MaxLength(191)]
    [Column("code")]
    public string Code { get; set; }

    [Required]
    [Column("subject", TypeName = "nvarchar(max)")]
    public string Subject { get; set; }

    [Required]
    [Column("body", TypeName = "nvarchar(max)")]
    public string Body { get; set; }

    [Column("description", TypeName = "nvarchar(max)")]
    public string Description { get; set; }

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; }

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
