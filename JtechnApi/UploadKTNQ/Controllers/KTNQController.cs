
using JtechnApi.Departments.Repositories;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JtechnApi.UploadKTNQ
{
    [ApiController]
    [Route("[controller]")]
    public class KTNQController : ControllerBase
    {

        private readonly ConnectionStrings con;
        private readonly IDepartmentRepository repo;
        private readonly ILogger<KTNQController> _logger;

        public KTNQController(ILogger<KTNQController> logger, ConnectionStrings c, IDepartmentRepository r)
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
