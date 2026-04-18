using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Auth.Dtos
{
   public class SendNotifyToUserDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }
        public int Type { get; set; } = 0;
        public string Screen { get; set; }
        public string Data { get; set; }


    }
}
