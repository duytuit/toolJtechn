

using System;
using System.ComponentModel.DataAnnotations;

namespace JtechnApi.Requireds.Models
{
    public class RequiredDto
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public string Code_nv { get; set; }
        [Required]
        public string Department { get; set; }
        public string Content { get; set; }
        public int? Size { get; set; }
        [Required]
        public int Usage_status { get; set; }
        [Required]
        public double Quantity { get; set; }
        [Required]
        public int Type { get; set; }
        [Required]
        public int Print { get; set; }
        [Required]
        public string Pc_name { get; set; }
        public DateTime? Created_at { get; set; }
        public DateTime? Updated_at { get; set; }
        public DateTime? Deleted_at { get; set; }

    }
}
