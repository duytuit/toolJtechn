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
        [Route("createOrUpdate")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] BankDto bankDto)
        {
            var now = DateTime.Now;

            // CHECK TRÙNG AccountNumber (trừ chính nó khi update)
            var existed = await _context.Banks
                .FirstOrDefaultAsync(x => 
                    x.AccountNumber == bankDto.AccountNumber 
                    && x.Id != bankDto.Id);

            if (existed != null)
                return ApiResponseResult<object>(false, "Số tài khoản đã tồn tại", null);

            Bank entity;

            if (bankDto.Id > 0)
            {
                // UPDATE
                entity = await _context.Banks.FindAsync(bankDto.Id);
                if (entity == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);

                entity.AccountNumber = bankDto.AccountNumber;
                entity.AccountHolder = bankDto.AccountHolder;
                entity.BankName = bankDto.BankName;
                entity.BranchName = bankDto.BranchName;
                entity.StorageId = bankDto.StorageId;
                entity.UpdatedAt = now;
                entity.UpdatedBy = userId; // nếu có

                await _context.SaveChangesAsync();

                return ApiResponseResult(true, "Cập nhật thành công", entity);
            }
            else
            {
                // CREATE
                entity = new Bank
                {
                    AccountNumber = bankDto.AccountNumber,
                    AccountHolder = bankDto.AccountHolder,
                    BankName = bankDto.BankName,
                    BranchName = bankDto.BranchName,
                    StorageId = bankDto.StorageId,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                await _context.Banks.AddAsync(entity);
                await _context.SaveChangesAsync();

                return ApiResponseResult(true, "Thêm thành công", entity);
            }
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
