using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Auth.Models;

namespace Vudaco.Auth.Repositories
{
    public interface ITokenService
    {
        public string GenerateAccessToken(User user, string deviceId, int expire,string type);
        public string GenerateRefreshToken();
    }
}
