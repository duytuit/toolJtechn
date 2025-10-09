using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Auth.Models
{
    [Table("user_token")]
    public class UserToken
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("UserId")]
        public int UserId { get; set; }

        [Column("DeviceId")]
        [Required]
        [MaxLength(255)]
        public string DeviceId { get; set; }

        [Column("Token")]
        [Required]
        [MaxLength(500)]
        public string Token { get; set; }

        [Column("ExpiryTime")]
        public DateTime ExpiryTime { get; set; }
    }
}
