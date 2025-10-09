
using JtechnApi.Shares.Connects;
using JtechnApi.UploadDatas.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadDataController : ControllerBase
    {

        private readonly ConnectionStrings con;
        private readonly IUploadDataRepository repo;
        private readonly ILogger<UploadDataController> _logger;

        public UploadDataController(ILogger<UploadDataController> logger, ConnectionStrings c, IUploadDataRepository r)
        {
            _logger = logger;
            con = c;
            repo = r;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var result = await repo.GetPaginatedAsync(page, pageSize);
            return Ok(result);
        }
    }
}
