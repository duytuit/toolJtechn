using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Storage.Models
{
    [Table("data_storage")]
    public class Storages
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        [MaxLength(191)]
        public string Code { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(191)]
        public string Name { get; set; }

        [Column("note")]
        [MaxLength(191)]
        public string Note { get; set; }

        [Column("address")]
        [MaxLength(191)]
        public string Address { get; set; }

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
