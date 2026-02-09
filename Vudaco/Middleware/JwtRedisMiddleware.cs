using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using Vudaco.Shares;
using Microsoft.AspNetCore.Authorization;
using Vudaco.Controllers;
using System.Text.Json;
using Vudaco.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;

namespace Vudaco.Middlewares
{
    public class JwtRedisMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RedisService _redis;
        private readonly string _jwtSecret;

        public JwtRedisMiddleware(RequestDelegate next, IConfiguration config, RedisService redis)
        {
            _next = next;
            _redis = redis;
            _jwtSecret = config["Jwt:Secret"];
        }

        public async Task Invoke(HttpContext context, VudacoDBContext _dbContext)
        {
            // 👉 Nếu có [AllowAnonymous] thì bỏ qua middleware này
            var endpoint = context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(_jwtSecret);

                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = false, // ⚠️ bỏ qua kiểm tra expired
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;

                    var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    var deviceId = jwtToken.Claims.FirstOrDefault(x => x.Type == "device_id")?.Value;
                    var type = jwtToken.Claims.FirstOrDefault(x => x.Type == "type")?.Value;
                    if (string.IsNullOrEmpty(type))
                    {
                        var redisKey = $"token:{userId}:{deviceId}";
                        var redisToken = await _redis.GetAsync(redisKey);
                        if (redisToken != token)
                        {
                            var _UserTokens = await _dbContext.UserTokens
                            .FirstOrDefaultAsync(s => s.UserId == int.Parse(userId) && s.DeviceId == deviceId);
                            if (_UserTokens == null || _UserTokens.Token != token || _UserTokens.ExpiryTime < DateTime.UtcNow)
                            {
                                var response = new ApiResponse<object>(false, "Token đã hết hạn hoặc thiết bị không hợp lệ");
                                context.Response.ContentType = "application/json";
                                context.Response.StatusCode = 401;
                                var json = JsonSerializer.Serialize(response);
                                await context.Response.WriteAsync(json);
                                return;
                            }
                            // 🔹 Token hợp lệ trong DB → cập nhật lại Redis
                            var ttl = _UserTokens.ExpiryTime - DateTime.UtcNow;
                            await _redis.SetAsync(redisKey, _UserTokens.Token, ttl > TimeSpan.Zero ? ttl : TimeSpan.FromDays(7));
                        }
                    }else{
                        var redisKey = $"{type}_token:{userId}:{deviceId}";
                        var redisToken = await _redis.GetAsync(redisKey);
                        if (redisToken != token)
                        {
                            var _UserTokens = await _dbContext.UserTokens
                            .FirstOrDefaultAsync(s => s.UserId == int.Parse(userId) && s.DeviceId == deviceId && s.Type == type);
                            if (_UserTokens == null || _UserTokens.Token != token || _UserTokens.ExpiryTime < DateTime.UtcNow)
                            {
                                var response = new ApiResponse<object>(false, "Token đã hết hạn hoặc thiết bị không hợp lệ");
                                context.Response.ContentType = "application/json";
                                context.Response.StatusCode = 401;
                                var json = JsonSerializer.Serialize(response);
                                await context.Response.WriteAsync(json);
                                return;
                            }
                            // 🔹 Token hợp lệ trong DB → cập nhật lại Redis
                            var ttl = _UserTokens.ExpiryTime - DateTime.UtcNow;
                            await _redis.SetAsync(redisKey, _UserTokens.Token, ttl > TimeSpan.Zero ? ttl : TimeSpan.FromDays(7));
                        }
                    }
                    context.Items["UserId"] = int.Parse(userId);
                    var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                    context.User = new ClaimsPrincipal(claimsIdentity);
                }
                catch(Exception ex)
                {
                    var response = new ApiResponse<object>(false, ex.Message);
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 401;
                    var json = JsonSerializer.Serialize(response);
                    await context.Response.WriteAsync(json);
                    return;
                }
            }
            else
            {
                var response = new ApiResponse<object>(false, "Thiếu token");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 401;
                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
                return;
            }

            await _next(context);
        }
    }
}
