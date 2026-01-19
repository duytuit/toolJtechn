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
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] BillDto BillDto)
        {

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                string CycleName = BillDto.AccountingDate.ToString("MMyyyy");
                var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", BillDto.StorageId, "HDKH" + BillDto.AccountingDate.ToString("yyMM"), 4);
                var  bill_Partner = new Bill
                    {
                        BillCode = BillCodePartner,
                        StorageId = BillDto.StorageId,
                        CustomerDetailId = BillDto.CustomerDetailId,
                        Name = CycleName.ToString(),
                        AccountingDate = BillDto.AccountingDate,
                        expiryDate = BillDto.expiryDate,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                _context.Bills.Add(bill_Partner);
                await _context.SaveChangesAsync();  // phải có
                var entities = await _context.Debits
                    .Where(x => BillDto.Ids.Contains(x.Id))
                    .ToListAsync();

                foreach (var item in entities)
                {
                    item.BillId = bill_Partner.Id;
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
    }
}
