using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Categorys.Models
{
    [Table("income_expense_categorys")]
    public class IncomeExpenseCategory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("code")]
        public string Code { get; set; }

        [Required]
        [MaxLength(191)]
        [Column("name")]
        public string Name { get; set; }

        [MaxLength(50)]
        [Column("parent_id")]
        public int ParentId { get; set; }

        [MaxLength(50)]
        [Column("type")]
        public int? Type { get; set; }
        [Column("enable")]
        public int? Enable { get; set; }
        [Column("status")]
        public int Status { get; set; }

        [Column("storage_id")]
        public int? StorageId { get; set; }

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
