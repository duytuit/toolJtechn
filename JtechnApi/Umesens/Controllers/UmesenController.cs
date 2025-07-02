
using JtechnApi.Shares.Connects;
using JtechnApi.Umesens.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UmesenController : ControllerBase
    {

        private readonly ConnectionStrings con;
        private readonly IUmesenRepository repo;
        private readonly ILogger<UmesenController> _logger;

        public UmesenController(ILogger<UmesenController> logger, ConnectionStrings c, IUmesenRepository r)
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
