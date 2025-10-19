
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vudaco.ContractFiles.Dtos;
using Vudaco.Controllers;
using Vudaco.Employees.Dtos;
using Vudaco.Employees.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Employees.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : BaseApiController
    {
        private readonly IEmployeeRepository _repo;
        private readonly ILogger<EmployeeController> _logger;
        private readonly VudacoDBContext _context;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeRepository repo, VudacoDBContext context)
        {
            _logger = logger;
            _repo = repo;
            _context = context;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] EmployeeDto EmployeeDto = null )
        {
            // test
            var result = await _repo.GetObjectTaskAsync(EmployeeDto, page, pageSize, cancellationToken);
                if (result == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
       //[HttpPost]
       //[Route("create")]
       //public async Task<IActionResult> Create([FromForm] FileInfoDto FileInfoDto)
       //{ 
       //}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] FileInfoDto FileInfoDto)
        {
            return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            //if (tempRequired == null || tempRequired.Id != id)
            //{
            //    return BadRequest("TempRequired cannot be null and ID must match");
            //}
            //
            //var updatedTempRequired = await repo.UpdateTempRequiredAsync(tempRequired);
            //if (updatedTempRequired == null)
            //{
            //    return NotFound();
            //}
            //return Ok(updatedTempRequired);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
           // var required = await repo.detail(id);
           // if (required == null)
           // {
               return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
           // }
           // return ApiResponseResult(true, "Lấy dữ liệu thành công", required);
        }
        [HttpDelete("{id}")]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            // var isDeleted = await repo.DeleteRequiredAsync(id);
            // if (!isDeleted)
            // {
            return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
          // }
          // return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
    }
}
