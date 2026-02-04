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
    public class NotificationController : BaseApiController
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly VudacoDBContext _context;
        private readonly FcmService _fcmService;
        private readonly IWebHostEnvironment _env;
        public int userId => (int)HttpContext.Items["UserId"];

        public NotificationController(ILogger<NotificationController> logger, VudacoDBContext context, IWebHostEnvironment env, FcmService fcmService)
        {
            _logger = logger;
            _context = context;
            _fcmService = fcmService;
            _env = env;
        }
          [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromBody] SendNotifyToUserDto dto)
        {
            var tokens = await _context.UserDeviceTokens
                .Where(x => x.UserId == dto.UserId && x.IsActive)
                .Select(x => x.DeviceToken)
                .Distinct()
                .ToListAsync();

            if (tokens.Count == 0)
                return Ok(new { message = "User không có token active" });

            var response = await _fcmService.SendMulticastAsync(tokens, dto.Title, dto.Body, dto.Data);

            return Ok(new
            {
                message = "Sent",
                totalTokens = tokens.Count,
                successCount = response.SuccessCount,
                failureCount = response.FailureCount
            });
        }
    }
}
