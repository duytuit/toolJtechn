using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vudaco.Employees.Models
{
    [Table("employees")]
    public class Employee
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(191)]
        [Column("code")]
        public string Code { get; set; }

        [Required]
        [MaxLength(191)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(191)]
        [Column("last_name")]
        public string LastName { get; set; }

        [MaxLength(191)]
        [Column("identity_card")]
        public string IdentityCard { get; set; }

        [MaxLength(600)]
        [Column("native_land")]
        public string NativeLand { get; set; }

        [MaxLength(600)]
        [Column("addresss")]
        public string Addresss { get; set; }

        [Column("birthday")]
        public DateTime? Birthday { get; set; }

        [Column("status")]
        public int? Status { get; set; }

        [Column("marital")]
        public int? Marital { get; set; }

        [Column("worker")]
        public int? Worker { get; set; }

        [Column("positions")]
        public int? Positions { get; set; }

        [Required]
        [Column("begin_date_company")]
        public DateTime BeginDateCompany { get; set; }

        [Column("end_date_company")]
        public DateTime? EndDateCompany { get; set; }

        [Required]
        [Column("storage_id")]
        public int StorageId { get; set; }

        [Column("created_by")]
        public long? CreatedBy { get; set; }

        [Column("updated_by")]
        public long? UpdatedBy { get; set; }

        [Column("deleted_by")]
        public long? DeletedBy { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [MaxLength(191)]
        [Column("avatar")]
        public string Avatar { get; set; }

        [MaxLength(191)]
        [Column("phone")]
         [Required]
        public string Phone { get; set; }

        [MaxLength(191)]
        [Column("email")]
        public string Email { get; set; }

        [MaxLength(191)]
        [Column("bank_number")]
        public string BankNumber { get; set; }

        [MaxLength(191)]
        [Column("bank_name")]
        public string BankName { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }
        [NotMapped]
        public EmployeeDepartment EmployeeDepartment { get; set; }
    }
}
