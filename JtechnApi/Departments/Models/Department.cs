using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JtechnApi.Requireds.Models;

namespace JtechnApi.Departments.Models
{
    [Table("departments")]
    public class Department
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("code")]
        public int? Code { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("parent_id")]
        public int? Parent_id { get; set; }
        [Column("status")]
        public int? Status { get; set; }
        [Column("permissions")]
        public string? Permissions { get; set; }
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
        public ICollection<Required> Requireds { get; set; }

        // public ICollection<EmployeeDepartment> EmployeeDepartments { get; set; }
    }
}
