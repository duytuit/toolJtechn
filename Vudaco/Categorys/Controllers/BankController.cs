using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Categorys.Dtos;
using Vudaco.Categorys.Models;
using Vudaco.Categorys.Repositories;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Categorys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankController : BaseApiController
    {
        private readonly IBankRepository _repoBankRepository;
        private readonly ILogger<BankController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public BankController(ILogger<BankController> logger, IBankRepository repoBankRepository, VudacoDBContext context)
        {
            _logger = logger;
            _repoBankRepository = repoBankRepository;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] BankDto BankDto = null)
        {
            // test
            var result = await _repoBankRepository.GetObjectTaskAsync(BankDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] BankDto BankDto)
        {
            // Check trùng Name
            var entity = await _context.Banks.FirstOrDefaultAsync(p => p.AccountNumber == BankDto.AccountNumber);
            if (entity != null)
                return ApiResponseResult<object>(false, "Tên dữ liệu đã tồn tại", null);
            var Bank = new Bank
            {
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            entity = await _repoBankRepository.CreateAsync(Bank);
            return ApiResponseResult(true, "Thêm thành công", entity);
        }
       
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  BankDto BankDto)
        {
            if (BankDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Banks.Find(BankDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoBankRepository.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoBankRepository.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
