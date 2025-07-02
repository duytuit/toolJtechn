
using JtechnApi.ProductionPlans.Repositories;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductionPlanController : ControllerBase
    {

        private readonly ConnectionStrings con;
        private readonly IProductionPlanRepository repo;
        private readonly ILogger<ProductionPlanController> _logger;

        public ProductionPlanController(ILogger<ProductionPlanController> logger, ConnectionStrings c, IProductionPlanRepository r)
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
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var result = await repo.GetPaginatedAsync(page, pageSize);
            return Ok(result);
        }
    }
}
