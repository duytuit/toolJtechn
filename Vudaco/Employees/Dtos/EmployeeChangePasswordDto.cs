using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Employees.Models;

namespace Vudaco.Employees.Dtos
{
    public class EmployeeChangePasswordDto
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
