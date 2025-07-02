
using JtechnApi.Requireds.Models;
using JtechnApi.Requireds.Repositories;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RequiredController : ControllerBase
    {

        private readonly ConnectionStrings con;
        private readonly IRequiredRepository repo;
        private readonly ILogger<ProductionPlanController> _logger;

        public RequiredController(ILogger<ProductionPlanController> logger, ConnectionStrings c, IRequiredRepository r)
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
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromForm] RequiredDto RequiredDto)
        { 
            
        }
    }
}
