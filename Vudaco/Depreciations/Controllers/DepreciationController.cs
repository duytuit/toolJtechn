using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Depreciations.Dtos;
using Vudaco.Depreciations.Repositories;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Depreciations.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepreciationController : BaseApiController
    {
        private readonly IDepreciationRepositories _repoBill;
        private readonly ILogger<DepreciationController> _logger;
        private readonly VudacoDBContext _context;
        public int userId => (int)HttpContext.Items["UserId"];
        public DepreciationController(ILogger<DepreciationController> logger, IDepreciationRepositories repoBill, VudacoDBContext context)
        {
            _logger = logger;
            _repoBill = repoBill;
            _context = context;
        }
        [HttpGet("GetObjectTaskAsync")]
        public async Task<IActionResult> GetObjectTaskAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DepreciationDto BillDto = null)
        {
            var result = await _repoBill.GetObjectTaskAsync(BillDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DepreciationDto billDto)
        {

            await using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();

            try
            {
                var now = DateTime.Now;
               
                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.Message, null);
            }
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DepreciationDto billDto)
        {
            if (billDto == null || billDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }

            await using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await _context.Bills.FirstOrDefaultAsync(x => x.Id == billDto.Id);

                if (entity == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }

                var now = DateTime.Now;
                entity.DeletedAt = now;
                entity.DeletedBy = userId;

                var debits = await _context.Debits
                    .Where(x => x.BillId == billDto.Id)
                    .ToListAsync();

                foreach (var item in debits)
                {
                    item.BillId = null;
                }

                _context.Bills.Update(entity);
                _context.Debits.UpdateRange(debits);

                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult<object>(true, "Xóa thành công", null);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, $"Lỗi khi xóa: {ex.Message}", null);
            }
        }
    }
}
