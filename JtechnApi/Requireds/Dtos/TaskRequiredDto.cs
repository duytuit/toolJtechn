

using System;
using System.ComponentModel.DataAnnotations;

namespace JtechnApi.Requireds.Models
{
    public class TaskRequiredDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Content { get; set; }
        public string Content_form { get; set; }
        [Required]
        public string Attach { get; set; }
        [Required]
        public DateTime? Created_client { get; set; }

    }
}
