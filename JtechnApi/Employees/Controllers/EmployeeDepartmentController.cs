
using JtechnApi.Employees.Repositories;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeDepartmentController : ControllerBase
    {

        private readonly ConnectionStrings con;
        private readonly IEmployeeDepartmentRepository repo;
        private readonly ILogger<EmployeeDepartmentController> _logger;

        public EmployeeDepartmentController(ILogger<EmployeeDepartmentController> logger, ConnectionStrings c, IEmployeeDepartmentRepository r)
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
