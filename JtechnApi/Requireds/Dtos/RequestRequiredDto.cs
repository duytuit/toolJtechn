

using System;
using System.ComponentModel.DataAnnotations;

namespace JtechnApi.Requireds.Models
{
    public class RequestRequiredDto
    {
        public string Keyword { get; set; }
        public string Code { get; set; }
        public string Code_nv { get; set; }
        public string Department { get; set; }
        public string Content { get; set; }
        public int? From_type { get; set; }
        public int? Size { get; set; }
        public int Usage_status { get; set; }
        public double Quantity { get; set; }
        public int Type { get; set; }
        public int Print { get; set; }
        public string Pc_name { get; set; }
        public int? Status { get; set; } = null;
        public DateTime? Created_at { get; set; } = null;
        public DateTime? Updated_at { get; set; } = null;    
        public DateTime? Deleted_at { get; set; } = null;

    }
}
