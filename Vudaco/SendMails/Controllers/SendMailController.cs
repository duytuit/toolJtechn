using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.SendMails.Dtos;
using Vudaco.SendMails.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.SendMails.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SendMailController : BaseApiController
    {
        private readonly ISmtpSettingRepositories _repoSmtp;
        private readonly ILogger<SendMailController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public SendMailController(ILogger<SendMailController> logger, ISmtpSettingRepositories repoSmtp, VudacoDBContext context)
        {
            _logger = logger;
            _repoSmtp = repoSmtp;
            _context = context;
        }
      
        [HttpPost("send")]
        public async Task<IActionResult> SendMail([FromBody] SendMailRequest request)
        {
            var result = await _repoSmtp.SendAsync(request);

            if (!result.Success)
            {
                return ApiResponseResult<object>(false, "Gửi mail thất bại: ", result);
            }
            return ApiResponseResult(true, "Gửi mail thành công", result);
        }
      
    }
}
