using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using JtechnApi.Accessorys.Models;
using JtechnApi.Departments.Models;
using JtechnApi.Employees.Models;

namespace JtechnApi.Requireds.Models
{
    [Table("requireds")]
    public class Required
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("title")]
        public string Title { get; set; }

        [Column("code_required")]
        public string Code_required { get; set; }
        [Column("code")]
        public string Code { get; set; }
        [Column("quantity")]
        public double? Quantity { get; set; }
        [Column("unit_price")]
        public double? Unit_price { get; set; }
        [Column("content")]
        public string Content { get; set; }
        [Column("size")]
        public int? Size { get; set; }
        [Column("attach")]
        public string Attach { get; set; }

        [Column("required_department_id")]
        public int? Required_department_id { get; set; }
        [Column("receiving_department_ids")]
        public string Receiving_department_ids { get; set; }
        [Column("status")]
        public int? Status { get; set; } 
        [Column("from_type")]
        public int? From_type { get; set; }
        [Column("date_completed")]
        public DateTime? Date_completed { get; set; }

        [Column("order")]
        public int? Order { get; set; }
      
        [Column("created_by")]
        public int? Created_by { get; set; }

        [Column("updated_by")]
        public int? Updated_by { get; set; }

        [Column("completed_by")]
        public int? Completed_by { get; set; }

        [Column("deleted_by")]
        public int? Deleted_by { get; set; }
        [Column("created_at")]
        public DateTime? Created_at { get; set; }
        [Column("updated_at")]
        public DateTime? Updated_at { get; set; }
        [Column("deleted_at")]
        public DateTime? Deleted_at { get; set; }
        [Column("created_client")]
        public DateTime? Created_client { get; set; }
        [Column("content_form")]
        public string Content_form { get; set; }
        [Column("usage_status")]
        public int? Usage_status { get; set; }
        [Column("type")]
        public int? Type { get; set; }
        [Column("confirm_form")]
        public string Confirm_form { get; set; }
        [Column("remaining")]
        public double? Remaining { get; set; }

        [Column("location")]
        public string Location { get; set; }

        [Column("quantity_detail")]
        public int? Quantity_detail { get; set; }

        [Column("pc_name")]
        public string Pc_name { get; set; }

        public ICollection<SignatureSubmission> SignatureSubmissions { get; set; }
        public Employee Employee { get; set; } 
        public Department Department { get; set; } 
        public Accessory Accessory { get; set; } 
    }
}
