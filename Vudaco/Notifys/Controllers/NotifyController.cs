using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Notifys.Dtos;
using Vudaco.Notifys.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Notifys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyController : BaseApiController
    {
        private readonly INotifyRepositories _repoComment;
        private readonly ILogger<NotifyController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public NotifyController(ILogger<NotifyController> logger, INotifyRepositories repoComment, VudacoDBContext context)
        {
            _logger = logger;
            _repoComment = repoComment;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] NotifyDto notifyDto = null)
        {
            // test
            var result = await _repoComment.GetObjectTaskAsync(notifyDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
      
    }
}
