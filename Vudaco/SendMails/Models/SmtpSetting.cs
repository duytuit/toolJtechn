using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.SendMails.Models
{
    [Table("smtp_settings")]
    public class SmtpSetting
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
    [MaxLength(191)]
    [Column("host")]
    public string Host { get; set; }

    [Required]
    [Column("port")]
    public int Port { get; set; }

    [Required]
    [MaxLength(191)]
    [Column("username")]
    public string Username { get; set; }

    [Required]
    [Column("password", TypeName = "nvarchar(max)")]
    public string Password { get; set; }

    [Required]
    [MaxLength(191)]
    [Column("from_email")]
    public string FromEmail { get; set; }

    [Required]
    [MaxLength(191)]
    [Column("from_name")]
    public string FromName { get; set; }

    [Required]
    [Column("enable_ssl")]
    public bool EnableSsl { get; set; }

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; }

    [Required]
    [Column("is_default")]
    public bool IsDefault { get; set; }

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
