using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Auth.Models
{
    [Table("user_device_tokens")]
    public class UserDeviceToken
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("device_token")]
        public string DeviceToken { get; set; }

        [Column("platform")]
        public string Platform { get; set; } // android | ios

        [Column("device_id")]
        public string DeviceId { get; set; }
        [Column("env")]
        public string Env { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

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
