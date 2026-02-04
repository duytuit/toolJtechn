using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Auth.Dtos
{
   public class LogoutDeviceDto
    {
        [Required]
        public int UserId { get; set; }

        public string DeviceToken { get; set; }
        public string DeviceId { get; set; }
    }
}
