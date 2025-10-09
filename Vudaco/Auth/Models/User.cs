using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Auth.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("first_name")]
        public string FirstName { get; set; }
        [Column("last_name")]
        public string LastName { get; set; }
        [Column("username")]
        public string Username { get; set; }
        [Column("phone_no")]
        public string PhoneNo { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("email_verified_at")]
        public DateTime? EmailVerifiedAt { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("avatar")]
        public string Avatar { get; set; }

        [Column("status")]
        public byte? Status { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }
    }
}
