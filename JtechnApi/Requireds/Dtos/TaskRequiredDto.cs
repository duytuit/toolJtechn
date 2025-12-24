

using System;
using System.ComponentModel.DataAnnotations;

namespace JtechnApi.Requireds.Models
{
    public class TaskRequiredDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        [Required]
        public string Code { get; set; }
        public string Content { get; set; }
        public string Content_form { get; set; }
        [Required]
        public string Emp_depts { get; set; }
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        public string Attach { get; set; }
        [Required]
        public string Task_types { get; set; }
        public DateTime? Created_client { get; set; }

    }
}
