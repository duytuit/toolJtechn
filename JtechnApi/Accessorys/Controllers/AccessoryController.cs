
using JtechnApi.Accessorys.Repositories;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JtechnApi.Accessorys.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccessoryController : ControllerBase
    {

        private readonly ConnectionStrings con;
        private readonly IAccessoryRepository repo;
        private readonly ILogger<AccessoryController> _logger;

        public AccessoryController(ILogger<AccessoryController> logger, ConnectionStrings c, IAccessoryRepository r)
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
