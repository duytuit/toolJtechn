

using System;
using System.ComponentModel.DataAnnotations;

namespace JtechnApi.Requireds.Models
{
    public class ChangeStatusTaskDto
    {
        [Required]
        public int Signature_Id { get; set; }
        [Required]
        public int Required_Id { get; set; }
        [Required]
        public int Status { get; set; }
        [Required]
        public int Created_By { get; set; }

    }
}
