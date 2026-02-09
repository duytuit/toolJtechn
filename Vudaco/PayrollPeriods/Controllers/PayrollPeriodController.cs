using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Comments.Dtos;
using Vudaco.Controllers;
using Vudaco.PayrollPeriods.Dtos;
using Vudaco.PayrollPeriods.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.PayrollPeriods.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayrollPeriodController : BaseApiController
    {
        private readonly IPayrollPeriodRepositories _repoPayrollPeriod;
        private readonly ILogger<PayrollPeriodController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public PayrollPeriodController(ILogger<PayrollPeriodController> logger, IPayrollPeriodRepositories repoPayrollPeriod, VudacoDBContext context)
        {
            _logger = logger;
            _repoPayrollPeriod = repoPayrollPeriod;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] CommentDto CommentDto = null)
        {
            // test
            var result = await _repoPayrollPeriod.GetObjectTaskAsync(null, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost("salary")]
        public IActionResult GetSalary([FromQuery] PayrollPeriodDto payrollPeriodDto = null)
        {
             var fileUrl = "https://admin.vudaco.online/salary/SalaryByCycleName";

             return ApiResponseResult(true, "Lấy dữ liệu thành công", new { fileUrl });
        }
        [HttpPost("SalaryByCycleName")]
        public IActionResult SalaryByCycleName([FromQuery] PayrollPeriodDto payrollPeriodDto = null)
        {
             var fileUrl = "https://admin.vudaco.online/salary/SalaryByCycleName";

             return ApiResponseResult(true, "Lấy dữ liệu thành công", new { fileUrl });
        }
      
    }
}
