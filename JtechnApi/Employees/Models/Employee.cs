using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JtechnApi.Requireds.Models;

namespace JtechnApi.Employees.Models
{
    [Table("employees")]
    public class Employee
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        public string Code { get; set; }
        [Column("first_name")]
        public string First_name { get; set; }
        [Column("last_name")]
        public string Last_name { get; set; }
        [Column("identity_card")]
        public string Identity_card { get; set; }
        [Column("native_land")]
        public string Native_land { get; set; }
        [Column("addresss")]
        public string Addresss { get; set; }
        [Column("birthday")]
        public DateTime? Birthday { get; set; }
        [Column("unit_id")]
        public int? Unit_id { get; set; }
        [Column("dept_id")]
        public int? Dept_id { get; set; }
        [Column("team_id")]
        public int? Team_id { get; set; }
        [Column("process_id")]
        public int? Process_id { get; set; }
        [Column("status")]
        public int Status { get; set; }
        [Column("marital")]
        public int? Marital { get; set; }
        [Column("worker")]
        public int? Worker { get; set; }
        [Column("positions")]
        public int? Positions { get; set; }
        [Column("begin_date_company")]
        public DateTime? Begin_date_company { get; set; }
        [Column("end_date_company")]
        public DateTime? End_date_company { get; set; }
        [Column("avatar")]
        public string Avatar { get; set; }
        [Column("phone")]
        public string Phone { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("bank_number")]
        public string Bank_number { get; set; }
        [Column("bank_name")]
        public string Bank_name { get; set; }
        [Column("status_exam")]
        public int? Status_exam { get; set; }
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

    }
}
