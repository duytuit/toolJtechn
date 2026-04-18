using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Auth.Dtos
{
    public class RegisterDeviceTokenDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string DeviceToken { get; set; }

        [Required]
        public string Platform { get; set; } // android | ios

        public string DeviceId { get; set; }
        public string Env { get; set; }
    }
}
