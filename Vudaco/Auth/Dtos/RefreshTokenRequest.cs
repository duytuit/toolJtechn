using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Auth.Dtos
{
    public class RefreshTokenRequest
    {
        public int UserId { get; set; }
        public string DeviceId { get; set; }
        public string RefreshToken { get; set; }
    }
}
