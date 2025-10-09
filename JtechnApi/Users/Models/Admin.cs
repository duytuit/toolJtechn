using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JtechnApi.Users.Models
{
    [Table("admins")]
    public class Admin
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("first_name")]
        public string First_name { get; set; }
        [Column("last_name")]
        public string Last_name { get; set; }
        [Column("username")]
        public string Username { get; set; }
        [Column("phone_no")]
        public string Phone_no { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("email_verified_at")]
        public DateTime? Email_verified_at { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("avatar")]
        public string Avatar { get; set; }
        [Column("status")]
        public bool Status { get; set; }
        [Column("visible_in_team")]
        public bool Visible_in_team { get; set; }
        [Column("designation")]
        public string Designation { get; set; }
        [Column("social_links")]
        public string Social_links { get; set; }
        [Column("remember_token")]
        public string Remember_token { get; set; }
      
        [Column("created_by")]
        public int? Created_by { get; set; }
        [Column("updated_by")]
        public int? Updated_by { get; set; }
        [Column("deleted_by")]
        public int? Deleted_by { get; set; }
        [Column("created_at")]
        public DateTime? Created_at { get; set; }
        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }
        [Column("employee_id")]
        public int? Employee_id { get; set; }

    }
}
