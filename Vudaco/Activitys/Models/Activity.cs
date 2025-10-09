using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Activitys.Models
{
    [Table("activities")]
    public class Activity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("content_id")]
        public int? ContentId { get; set; }

        [Column("content_type")]
        [MaxLength(100)]
        public string ContentType { get; set; }

        [Required]
        [Column("action")]
        [MaxLength(50)]
        public string Action { get; set; }

        [Column("description")]
        [MaxLength(255)]
        public string Description { get; set; }

        [Column("old_data")]
        public string OldData { get; set; }

        [Column("new_data")]
        public string NewData { get; set; }

        [Column("sql")]
        public string Sql { get; set; }

        [Column("ip_address")]
        [MaxLength(45)]
        public string IpAddress { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}
