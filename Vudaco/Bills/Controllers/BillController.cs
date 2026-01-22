using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Bills.Dtos;
using Vudaco.Bills.Models;
using Vudaco.Bills.Repositories;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Bills.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillController : BaseApiController
    {
        private readonly IBillRepositories _repoBill;
        private readonly ILogger<BillController> _logger;
        private readonly VudacoDBContext _context;
        public int userId => (int)HttpContext.Items["UserId"];
        public BillController(ILogger<BillController> logger, IBillRepositories repoBill, VudacoDBContext context)
        {
            _logger = logger;
            _repoBill = repoBill;
            _context = context;
        }
        [HttpGet("GetObjectTaskAsync")]
        public async Task<IActionResult> GetObjectTaskAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] BillDto BillDto = null)
        {
            var result = await _repoBill.GetObjectTaskAsync(BillDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] BillDto billDto)
        {

            await using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();

            try
            {
                var now = DateTime.Now;
                Bill bill;

                // ===== 1. UPDATE BILL =====
                if (billDto.Id > 0)
                {
                    bill = await _context.Bills.FirstOrDefaultAsync(x => x.Id == billDto.Id);

                    if (bill == null)
                    {
                        return ApiResponseResult<object>(false, "Không tìm thấy kỳ công nợ", null);
                    }

                    bill.AccountingDate = billDto.AccountingDate;
                    bill.expiryDate = billDto.expiryDate;
                    bill.Name = billDto.Name;
                    bill.UpdatedAt = now;
                    bill.UpdatedBy = userId;

                    _context.Bills.Update(bill);
                }
                // ===== 2. CREATE BILL =====
                else
                {
                    var billCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(
                        conn,
                        tran.GetDbTransaction(),
                        "bills",
                        "bill_code",
                        billDto.StorageId,
                        "HDKH" + billDto.AccountingDate.ToString("yyMM"),
                        4
                    );

                    bill = new Bill
                    {
                        BillCode = billCode,
                        StorageId = billDto.StorageId,
                        CustomerDetailId = billDto.CustomerDetailId,
                        Name = billDto.Name,
                        AccountingDate = billDto.AccountingDate,
                        expiryDate = billDto.expiryDate,
                        CycleName = billDto.CycleName,
                        CreatedAt = now,
                        CreatedBy = userId,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };

                    _context.Bills.Add(bill);
                }

                await _context.SaveChangesAsync(); // BẮT BUỘC để có bill.Id

                // ===== 3. GÁN DEBIT VÀO BILL =====
                var debits = await _context.Debits
                    .Where(x => billDto.DebitIds.Contains(x.Id))
                    .ToListAsync();

                foreach (var item in debits)
                {
                    item.BillId = bill.Id;
                }

                _context.Debits.UpdateRange(debits);

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
        public async Task<IActionResult> Delete([FromBody] BillDto billDto)
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
