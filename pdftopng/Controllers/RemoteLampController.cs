using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using pdftopng.Services;

namespace pdftopng.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RemoteLampController : ControllerBase
    {
        private readonly WebSocketClientService _webSocketClientService;

        public RemoteLampController(WebSocketClientService webSocketClientService)
        {
            _webSocketClientService = webSocketClientService;
        }

        [HttpPost]
        [Route("control-lamp")]
        public async Task<IActionResult> RemoteLamp([FromForm] RemoteLampFromTemplateRequest request, CancellationToken stoppingToken)
        {
            if (request.Chanel == null || request.Event == 0)
            {
                return BadRequest("No Chanel.");
            }

            try
            {
                var obj = new
                {
                    Event = request.Event,
                    Chanel = request.Chanel,
                    Status= request.Status,
                    MessageText= request.MessageText,
                    Mode= request.Mode
                };
                string jsonData = JsonConvert.SerializeObject(obj);
                await _webSocketClientService.SendMessage(jsonData, stoppingToken);
                return StatusCode(200, "thành công");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
       
    }
    public sealed class RemoteLampFromTemplateRequest
    {
        [Required]
        public int Event { get; set; }

        public string Chanel { get; set; }

        public int Status { get; set; }

        public string MessageText { get; set; }

        public int Mode { get; set; }
    }
}
