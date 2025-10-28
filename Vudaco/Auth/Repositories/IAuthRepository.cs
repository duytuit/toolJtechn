using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Auth.Dtos;
using Vudaco.Auth.Models;
using Vudaco.Employees.Models;

namespace Vudaco.Auth.Repositories
{
    public interface IAuthRepository
    {
        Task<User> RegisterAsync(RegisterRequest request);
        Task<(string accessToken, string refreshToken, Employee employee)> LoginAsync(LoginRequest request);
        Task LogoutAsync(int userId, string deviceId);
        Task<(string accessToken, string refreshToken, Employee employee)> RefreshTokenAsync(RefreshTokenRequest request);
        Task LogoutAllAsync(int userId);
    }
}
