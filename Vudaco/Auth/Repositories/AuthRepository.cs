using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Auth.Dtos;
using Vudaco.Auth.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Auth.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly VudacoDBContext _db;
        private readonly RedisService _redis;
        private readonly ITokenService _tokenService;

        public AuthRepository(VudacoDBContext db, RedisService redis, ITokenService tokenService)
        {
            _db = db;
            _redis = redis;
            _tokenService = tokenService;
        }

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
                throw new Exception("Tên đăng nhập đã tồn tại");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                Password = passwordHash,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return user;
        }

        public async Task<(string accessToken, string refreshToken)> LoginAsync(LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Username == request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new Exception("Sai tên đăng nhập hoặc mật khẩu");

            var accessToken = _tokenService.GenerateAccessToken(user, request.DeviceId);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var redisKey = $"token:{user.Id}:{request.DeviceId}";
            await _redis.SetAsync(redisKey, refreshToken, TimeSpan.FromDays(7));
            var userToken = new UserToken
            {
                UserId = user.Id,
                DeviceId = request.DeviceId,
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(7)
            };
            _db.UserTokens.Add(userToken);
            await _db.SaveChangesAsync();
            return (accessToken, refreshToken);
        }

        public async Task LogoutAsync(int userId, string deviceId)
        {
            var redisKey = $"token:{userId}:{deviceId}";
            await _redis.RemoveAsync(redisKey);
            var dbToken = await _db.UserTokens.FirstOrDefaultAsync(t => t.UserId == userId && t.DeviceId == deviceId);

            if (dbToken != null)
            {
                _db.UserTokens.Remove(dbToken);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<(string accessToken, string refreshToken)> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var redisKey = $"token:{request.UserId}:{request.DeviceId}";
            var savedToken = await _redis.GetAsync(redisKey);

            if (string.IsNullOrEmpty(savedToken) || savedToken != request.RefreshToken)
                throw new Exception("Refresh token không hợp lệ hoặc đã hết hạn");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null)
                throw new Exception("Người dùng không tồn tại");

            var newAccessToken = _tokenService.GenerateAccessToken(user, request.DeviceId);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            await _redis.SetAsync(redisKey, newRefreshToken, TimeSpan.FromDays(7));
            var userToken = await _db.UserTokens.FirstOrDefaultAsync(t => t.UserId == request.UserId && t.DeviceId == request.DeviceId);

            if (userToken != null)
            {
                userToken.Token = newRefreshToken;
                userToken.ExpiryTime = DateTime.UtcNow.AddDays(7);
                await _db.SaveChangesAsync();
            }
            return (newAccessToken, newRefreshToken);
        }
        public async Task LogoutAllAsync(int userId)
        {
            var pattern = $"token:{userId}:*";
            var keys = _redis.GetKeysByPattern(pattern);

            foreach (var key in keys)
            {
                await _redis.RemoveAsync(key);
            }
            // 2. Xóa DB
            var tokens = await _db.UserTokens.Where(t => t.UserId == userId).ToListAsync();
            if (tokens.Any())
            {
                _db.UserTokens.RemoveRange(tokens);
                await _db.SaveChangesAsync();
            }
        }
    }
}
