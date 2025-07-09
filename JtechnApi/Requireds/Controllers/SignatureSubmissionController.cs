
using JtechnApi.Requireds.Models;
using JtechnApi.Requireds.Repositories;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq.Dynamic.Core;
using System.Text.Json;
using System.Threading.Tasks;

namespace JtechnApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SignatureSubmissionController : ControllerBase
    {

        private readonly ConnectionStrings con;
        private readonly ISignatureSubmissionRepository repo;
        private readonly ILogger<SignatureSubmissionController> _logger;
         private readonly DBContext _context;

        public SignatureSubmissionController(ILogger<SignatureSubmissionController> logger, ConnectionStrings c, ISignatureSubmissionRepository r, DBContext context)
        {
            _logger = logger;
            con = c;
            repo = r;
            _context = context;
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
