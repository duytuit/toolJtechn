using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vudaco.Auth.Dtos;
using Vudaco.Auth.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : BaseApiController
    {
        private readonly ILogger<DeviceController> _logger;
        private readonly VudacoDBContext _context;
        private readonly IWebHostEnvironment _env;
        public int userId => (int)HttpContext.Items["UserId"];

        public DeviceController(ILogger<DeviceController> logger, VudacoDBContext context, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _env = env;
        }
          [HttpPost("register-token")]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterDeviceTokenDto dto)
        {
            dto.Platform = dto.Platform?.Trim()?.ToLower();

            if (dto.Platform != "android" && dto.Platform != "ios")
                 return ApiResponseResult<object>(false, "Platform chỉ nhận: android | ios", null);

            // 1) Token đã tồn tại -> update
            var existedByToken = await _context.UserDeviceTokens
                .FirstOrDefaultAsync(x => x.DeviceToken == dto.DeviceToken);

            if (existedByToken != null)
            {
                existedByToken.UserId = dto.UserId;
                existedByToken.Platform = dto.Platform;
                existedByToken.DeviceId = dto.DeviceId;
                existedByToken.IsActive = true;
                existedByToken.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return ApiResponseResult<object>(true, "Updated token", null);
            }

            // 2) Nếu có DeviceId thì update token theo deviceId
            if (!string.IsNullOrEmpty(dto.DeviceId))
            {
                var existedByDeviceId = await _context.UserDeviceTokens
                    .FirstOrDefaultAsync(x => x.UserId == dto.UserId && x.DeviceId == dto.DeviceId);

                if (existedByDeviceId != null)
                {
                    existedByDeviceId.DeviceToken = dto.DeviceToken;
                    existedByDeviceId.Platform = dto.Platform;
                    existedByDeviceId.IsActive = true;
                    existedByDeviceId.UpdatedAt = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    return ApiResponseResult<object>(true, "Updated token by deviceId", null);
                }
            }

            // 3) Insert mới
            var entity = new UserDeviceToken
            {
                UserId = dto.UserId,
                DeviceToken = dto.DeviceToken,
                Platform = dto.Platform,
                DeviceId = dto.DeviceId,
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.UserDeviceTokens.Add(entity);
            await _context.SaveChangesAsync();

            return ApiResponseResult(true, "Registered token", entity);
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDeviceDto dto)
        {
            if (string.IsNullOrEmpty(dto.DeviceToken) && string.IsNullOrEmpty(dto.DeviceId))
                return ApiResponseResult<object>(false, "Phải truyền DeviceToken hoặc DeviceId", null);

            var query = _context.UserDeviceTokens
                .Where(x => x.UserId == dto.UserId && x.IsActive);

            if (!string.IsNullOrEmpty(dto.DeviceToken))
                query = query.Where(x => x.DeviceToken == dto.DeviceToken);

            if (!string.IsNullOrEmpty(dto.DeviceId))
                query = query.Where(x => x.DeviceId == dto.DeviceId);

            var list = await query.ToListAsync();

            foreach (var item in list)
            {
                item.IsActive = false;
                item.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return ApiResponseResult<object>(true, "Logged out", null);
        }
    }
}
