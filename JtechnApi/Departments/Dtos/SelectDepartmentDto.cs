using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JtechnApi.Requireds.Models;

namespace JtechnApi.Departments.Dtos
{
    public class SelectDepartmentDto
    {
        public int Id { get; set; }
        public int? Code { get; set; }
        public string Name { get; set; }
        public int? Parent_id { get; set; }
        public int? Status { get; set; }
        public string Permissions { get; set; }
        public int? Created_by { get; set; }
        public int? Updated_by { get; set; }
        public int? Deleted_by { get; set; }
        public DateTime? Created_at { get; set; }
        public DateTime? Updated_at { get; set; }
        public DateTime? Deleted_at { get; set; }
    }
}
