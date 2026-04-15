using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Vudaco.Auth.Dtos;
using Vudaco.Auth.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Notifys.Dtos;
using Vudaco.Notifys.Repositories;
using Microsoft.AspNetCore.Authorization;
namespace Vudaco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // 👈 thêm dòng này
    public class NotificationController : BaseApiController
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly VudacoDBContext _context;
        private readonly FcmService _fcmService;
        private readonly IWebHostEnvironment _env;
        private readonly IFcmQueue _fcmQueue;
        public int userId => (int)HttpContext.Items["UserId"];

        public NotificationController(ILogger<NotificationController> logger, VudacoDBContext context, IWebHostEnvironment env, FcmService fcmService, IFcmQueue fcmQueue)
        {
            _logger = logger;
            _context = context;
            _fcmService = fcmService;
            _fcmQueue = fcmQueue;
            _env = env;
        }
          [HttpPost("send-to-user")]
        public async Task<IActionResult> SendToUser([FromBody] SendNotifyToUserDto dto)
        {
         
            await _fcmQueue.EnqueueAsync(new FcmJobDto
            {
                UserIds = new List<int> { dto.UserId },
                Title = dto.Title,
                Body = dto.Body,
                StorageId = 1,
                PostId = 1,
                Type = 0,
                Screen = "chuyenxe"
            });

            return Ok(new
            {
                message = "Sent",
               
            });
        }
    }
}
