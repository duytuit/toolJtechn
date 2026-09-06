using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : BaseApiController
    {
        private readonly ILogger<UploadController> _logger;
        private readonly VudacoDBContext _context;
        private readonly IWebHostEnvironment _env;
        public int userId => (int)HttpContext.Items["UserId"];
        private readonly IConfiguration _configuration;

        public UploadController(ILogger<UploadController> logger, VudacoDBContext context, IWebHostEnvironment env, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _env = env;
            _configuration = configuration;
        }
        // POST api/upload/single
        [HttpPost("single")]
        public async Task<IActionResult> UploadSingle(IFormFile files, [FromForm] string? folder)
        {
          
            var result = await Helper.ProcessFileAsync(files, _env.WebRootPath, folder);
            if (result.Success)
                return Ok(new { success = true, message = "OK", path = result.Path, fullPath =$"{_configuration["BaseUrl"]}/{result.FullPath}", fileName = result.FileName });
            else
                return BadRequest(new { success = false, message = result.Message });
        }

        // POST api/upload/multiple
       [HttpPost("multiple")]
        public async Task<IActionResult> UploadMultiple(
            [FromForm] IFormFile[] files,
            [FromForm] string? folder)
        {
            if (files == null || files.Length == 0)
                return BadRequest(new { success = false, message = "Không có file nào." });

            var results = await Task.WhenAll(
                files.Select(f => Helper.ProcessFileAsync(f, _env.WebRootPath, folder))
            );

            var successPaths = results
                .Where(r => r.Success)
                .Select(r => new { Path = r.Path, FileName = r.FileName, FullPath = $"{_configuration["BaseUrl"]}/{r.FullPath}" })
                .ToList();

            var failed = results
                .Where(r => !r.Success)
                .Select(r => r.Message)
                .ToList();

            if (failed.Any())
            {
                return StatusCode(207, new
                {
                    success = false,
                    message = "Có file upload thất bại",
                    path = successPaths,
                    errors = failed
                });
            }

            return Ok(new
            {
                success = true,
                message = "OK",
                path = successPaths
            });
        }

    }
}
