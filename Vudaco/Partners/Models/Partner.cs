using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Partners.Models
{
    [Table("partners")]
    public class Partner
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [MaxLength(191)]
        [Column("code")]
        public string Code { get; set; }

        [Required]
        [MaxLength(191)]
        [Column("name")]
        public string Name { get; set; }

        [MaxLength(600)]
        [Column("address")]
        public string Address { get; set; }

        [MaxLength(50)]
        [Column("tax_code")]
        public string TaxCode { get; set; }

        [MaxLength(50)]
        [Column("phone")]
         [Required]
        public string Phone { get; set; }

        [MaxLength(191)]
        [Column("email")]
        public string Email { get; set; }

        [MaxLength(50)]
        [Column("bank_account")]
        public string BankAccount { get; set; }

        [Column("allowed_debt_days")]
        public int? AllowedDebtDays { get; set; }

        [Column("max_debt")]
        public double? MaxDebt { get; set; }

        [Column("note", TypeName = "nvarchar(max)")]
        public string Note { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }
        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("abbreviation")]
        public string Abbreviation { get; set; }

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
