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

        public async Task Invoke(HttpContext context)
        {
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
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;

                    var userId = jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    var deviceId = jwtToken.Claims.FirstOrDefault(x => x.Type == "device_id")?.Value;

                    // Check token tồn tại trong Redis (chỉ khi có deviceId)
                    if (!string.IsNullOrEmpty(deviceId))
                    {
                        var redisKey = $"token:{userId}:{deviceId}";
                        var redisToken = await _redis.GetAsync(redisKey);

                        if (string.IsNullOrEmpty(redisToken))
                        {
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("Token đã hết hạn hoặc thiết bị không hợp lệ");
                            return;
                        }
                    }

                    // Gán UserId để controller dùng
                    context.Items["UserId"] = int.Parse(userId);

                    // Gán claims
                    var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                    context.User = new ClaimsPrincipal(claimsIdentity);
                }
                catch
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Token không hợp lệ");
                    return;
                }
            }

            await _next(context);
        }
    }
}
