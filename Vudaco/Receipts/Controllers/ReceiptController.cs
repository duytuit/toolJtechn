using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Receipts.Dtos;
using Vudaco.Receipts.Models;
using Vudaco.Receipts.Repositories;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.Connects;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Receipts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptController:BaseApiController
    {
        private readonly IReceiptDetailRepositories _repoReceiptDetail;
        private readonly IReceiptRepositories _repoReceipt;
        private readonly ILogger<ReceiptController> _logger;
        private readonly VudacoDBContext _context;
        private readonly AdoVudacoDB _db;
        public int userId => (int)HttpContext.Items["UserId"];

        public ReceiptController(ILogger<ReceiptController> logger,AdoVudacoDB db, IReceiptDetailRepositories repoReceiptDetail, IReceiptRepositories repoReceipt, VudacoDBContext context)
        {
            _logger = logger;
            _repoReceiptDetail = repoReceiptDetail;
            _repoReceipt = repoReceipt;
            _context = context;
            _db = db;
        }
        [HttpGet]
        public async Task<IActionResult> GetReceipt(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] ReceiptDto ReceiptDto = null)
        {
            // test
            var result = await _repoReceipt.GetObjectTaskAsync(ReceiptDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("XacNhanChiPhiGiaoNhan")]
        public async Task<IActionResult> GetXacNhanChiPhiGiaoNhan(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] ReceiptDto ReceiptDto = null)
        {
            // test
            var result = await _repoReceipt.GetXacNhanChiPhiGiaoNhanAsync(ReceiptDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create/giayHoanUng")]
        public async Task<IActionResult> CreateGiayHoanUng([FromBody] ReceiptHoanUngGiaoNhanDto ReceiptHoanUngGiaoNhanDto)
        {
            if (ReceiptHoanUngGiaoNhanDto.EmployeeId == null || ReceiptHoanUngGiaoNhanDto.EmployeeId == 0)
                return ApiResponseResult<object>(false, "Nhân viên giao nhận bắt buộc", null);
            if (ReceiptHoanUngGiaoNhanDto.Amount <= 0 )
                return ApiResponseResult<object>(false, "Chưa có thông tin hoàn ứng", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                var PrefixCode = ReceiptHoanUngGiaoNhanDto.TypeReceipt == ReceiptRepositories.ChiHoanUngGiaoNhan ? "PC"+ReceiptHoanUngGiaoNhanDto.AccountingDate.ToString("yyMM"):"PT"+ReceiptHoanUngGiaoNhanDto.AccountingDate.ToString("yyMM");
                var code_receipt = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", ReceiptHoanUngGiaoNhanDto.StorageId, PrefixCode , 4);

                var entity = new Receipt
                {
                    AccountingDate = ReceiptHoanUngGiaoNhanDto.AccountingDate,
                    StorageId = ReceiptHoanUngGiaoNhanDto.StorageId,
                    CodeReceipt = code_receipt,
                    EmployeeId = ReceiptHoanUngGiaoNhanDto.EmployeeId,
                    Bill = ReceiptHoanUngGiaoNhanDto.Bill,
                    Note = ReceiptHoanUngGiaoNhanDto.Note,
                    Description =  ReceiptHoanUngGiaoNhanDto.Description,
                    FormOfPayment = ReceiptHoanUngGiaoNhanDto.FormOfPayment,
                    TypeReceipt = ReceiptHoanUngGiaoNhanDto.TypeReceipt,
                    Data = ReceiptHoanUngGiaoNhanDto.Data,
                    Status = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId,
                };

                _context.Receipts.Add(entity);
                await _context.SaveChangesAsync();
                var entity_detail = new ReceiptDetail
                {
                    ReceiptId = entity.Id,
                    StorageId = ReceiptHoanUngGiaoNhanDto.StorageId,
                    AccountingDate = ReceiptHoanUngGiaoNhanDto.AccountingDate,
                    Amount = ReceiptHoanUngGiaoNhanDto.Amount,
                    Vat = ReceiptHoanUngGiaoNhanDto.Vat,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now

                };
                _context.ReceiptDetails.Add(entity_detail);
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                if ( !string.IsNullOrEmpty(ReceiptHoanUngGiaoNhanDto.Data))
                {
                    var list = JsonSerializer.Deserialize<List<JsonElement>>(ReceiptHoanUngGiaoNhanDto.Data);
                    foreach (var item in list)
                    {
                        int fileInfoId = item.GetProperty("fileInfoId").GetInt32();
                        var file_infos_object = new
                        {
                            id = fileInfoId,
                            receipt_id = entity.Id
                        };
                        _db.UpsertFromObject("file_infos", file_infos_object,"id");
                    }
                }
                return ApiResponseResult(true, "Thêm thành công", entity);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost]
        [Route("update/HoanUngGiaoNhan")]
        public async Task<IActionResult> UpdateHoanUngGiaoNhan([FromBody] ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.Status == 1)
            {
                if (ReceiptDto.FormOfPayment == 1 && (ReceiptDto.FundId == null || ReceiptDto.FundId == 0))
                    return ApiResponseResult<object>(false, "Mã quỹ bắt buộc", null);
                if (ReceiptDto.FormOfPayment == 2 && (ReceiptDto.BankId == null || ReceiptDto.BankId == 0))
                    return ApiResponseResult<object>(false, "Mã Ngân hàng bắt buộc", null);
                if (ReceiptDto.IncomeExpenseCategoryId == null || ReceiptDto.IncomeExpenseCategoryId == 0)
                    return ApiResponseResult<object>(false, "ly do chi bắt buộc", null);
            }
            var entity = await _context.Receipts.FirstOrDefaultAsync(p => p.Id == ReceiptDto.Id);
            if (entity == null)
                return ApiResponseResult<object>(false, "Mã yêu cầu không tồn tại", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                if (ReceiptDto.Status == 1)
                {
                    entity.AccountingDate = ReceiptDto.AccountingDate;
                    entity.FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0;
                    entity.IncomeExpenseCategoryId = ReceiptDto.IncomeExpenseCategoryId;
                    entity.Note = ReceiptDto.Note;
                    entity.FormOfPayment = ReceiptDto.FormOfPayment;
                    entity.BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0;
                    entity.Status = ReceiptDto.Status;
                    entity.UpdatedAt = DateTime.Now;
                    entity.UpdatedBy = userId;
                    _context.Receipts.Update(entity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    entity.FundId = null;
                    entity.IncomeExpenseCategoryId = null;
                    entity.Note =  null;
                    entity.FormOfPayment = ReceiptDto.FormOfPayment;
                    entity.BankId = null;
                    entity.Status = ReceiptDto.Status;
                    entity.UpdatedAt = DateTime.Now;
                    entity.UpdatedBy = userId;
                    _context.Receipts.Update(entity);
                    await _context.SaveChangesAsync();
                }
              
                await tran.CommitAsync();
                return ApiResponseResult(true, "Cập nhật thành công", entity);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost]
        [Route("create/chigiaonhan")]
        public async Task<IActionResult> CreateChiGiaoNhan([FromBody] ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.EmployeeId == null || ReceiptDto.EmployeeId == 0)
                return ApiResponseResult<object>(false, "Nhân viên giao nhận bắt buộc", null);
            if (ReceiptDto.FileInfoId == null || ReceiptDto.FileInfoId == 0)
                return ApiResponseResult<object>(false, "so file bắt buộc", null);
            if (ReceiptDto.FormOfPayment == 1 && (ReceiptDto.FundId == null || ReceiptDto.FundId == 0))
                return ApiResponseResult<object>(false, "Mã quỹ bắt buộc", null);
            if (ReceiptDto.FormOfPayment == 2 && (ReceiptDto.BankId == null || ReceiptDto.BankId == 0))
                return ApiResponseResult<object>(false, "Mã Ngân hàng bắt buộc", null);
            if (ReceiptDto.IncomeExpenseCategoryId == null || ReceiptDto.IncomeExpenseCategoryId == 0)
                return ApiResponseResult<object>(false, "ly do chi bắt buộc", null);
            // Check trùng Name
            var entity = await _context.Receipts.FirstOrDefaultAsync(p => p.CodeReceipt == ReceiptDto.CodeReceipt);
            if (entity != null)
                return ApiResponseResult<object>(false, "ma phieu chi đã tồn tại", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var code_receipt = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", ReceiptDto.StorageId, "PC"+ReceiptDto.AccountingDate.ToString("yyMM"), 4);

                entity = new Receipt
                {
                    AccountingDate = ReceiptDto.AccountingDate,
                    StorageId = ReceiptDto.StorageId,
                    CodeReceipt = code_receipt,
                    FileInfoId = ReceiptDto.FileInfoId,
                    EmployeeId = ReceiptDto.EmployeeId,
                    Bill = ReceiptDto.Bill,
                    FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0,
                    IncomeExpenseCategoryId = ReceiptDto.IncomeExpenseCategoryId,
                    Note = ReceiptDto.Note,
                    FormOfPayment = ReceiptDto.FormOfPayment,
                    TypeReceipt = ReceiptRepositories.ChiGiaoNhan,
                    BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0,
                    Data = ReceiptDto.Data,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = userId,
                };

                _context.Receipts.Add(entity);
                await _context.SaveChangesAsync();
                var entity_detail = new ReceiptDetail
                {
                    ReceiptId = entity.Id,
                    StorageId = ReceiptDto.StorageId,
                    AccountingDate = ReceiptDto.AccountingDate,
                    Amount = ReceiptDto.Amount,
                    Vat = ReceiptDto.Vat,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now

                };
                _context.ReceiptDetails.Add(entity_detail);
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult(true, "Thêm thành công", entity);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost("update/chigiaonhan")]
        public async Task<IActionResult> Update([FromBody] ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.EmployeeId == null || ReceiptDto.EmployeeId == 0)
                return ApiResponseResult<object>(false, "Nhân viên giao nhận bắt buộc", null);
            if (ReceiptDto.FileInfoId == null || ReceiptDto.FileInfoId == 0)
                return ApiResponseResult<object>(false, "Số file bắt buộc", null);
            if (ReceiptDto.FormOfPayment == 1 && (ReceiptDto.FundId == null || ReceiptDto.FundId == 0))
                return ApiResponseResult<object>(false, "Mã quỹ bắt buộc", null);
            if (ReceiptDto.FormOfPayment == 2 && (ReceiptDto.BankId == null || ReceiptDto.BankId == 0))
                return ApiResponseResult<object>(false, "Mã Ngân hàng bắt buộc", null);
            if (ReceiptDto.IncomeExpenseCategoryId == null || ReceiptDto.IncomeExpenseCategoryId == 0)
                return ApiResponseResult<object>(false, "Lý do chi bắt buộc", null);

            // Tìm entity hiện có
            var entity = await _context.Receipts.FirstOrDefaultAsync(p => p.Id == ReceiptDto.Id && p.TypeReceipt == ReceiptRepositories.ChiGiaoNhan);
            if (entity == null)
                return ApiResponseResult<object>(false, "Không tìm thấy phiếu chi giao nhận", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                // Cập nhật thông tin phiếu chi
                entity.AccountingDate = ReceiptDto.AccountingDate;
                entity.StorageId = ReceiptDto.StorageId;
                entity.FileInfoId = ReceiptDto.FileInfoId;
                entity.EmployeeId = ReceiptDto.EmployeeId;
                entity.Bill = ReceiptDto.Bill;
                entity.FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0;
                entity.IncomeExpenseCategoryId = ReceiptDto.IncomeExpenseCategoryId;
                entity.Note = ReceiptDto.Note;
                entity.FormOfPayment = ReceiptDto.FormOfPayment;
                entity.BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0;
                entity.Data = ReceiptDto.Data;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = userId;

                _context.Receipts.Update(entity);
                await _context.SaveChangesAsync();

                // Cập nhật chi tiết phiếu chi
                var detail = await _context.ReceiptDetails.FirstOrDefaultAsync(d => d.ReceiptId == entity.Id);
                if (detail != null)
                {
                    detail.StorageId = ReceiptDto.StorageId;
                    detail.AccountingDate = ReceiptDto.AccountingDate;
                    detail.Amount = ReceiptDto.Amount;
                    detail.Vat = ReceiptDto.Vat;
                    detail.UpdatedAt = DateTime.Now;
                    detail.UpdatedBy = userId;

                    _context.ReceiptDetails.Update(detail);
                }
                else
                {
                    // Nếu chưa có chi tiết thì thêm mới
                    var newDetail = new ReceiptDetail
                    {
                        ReceiptId = entity.Id,
                        StorageId = ReceiptDto.StorageId,
                        AccountingDate = ReceiptDto.AccountingDate,
                        Amount = ReceiptDto.Amount,
                        Vat = ReceiptDto.Vat,
                        CreatedBy = userId,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = userId
                    };
                    _context.ReceiptDetails.Add(newDetail);
                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult(true, "Cập nhật thành công", entity);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi không xác định: " + ex.Message, null);
            }
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  ReceiptDto ReceiptDto)
        {
             if (ReceiptDto.Id <= 0)
                return ApiResponseResult<object>(false, "Id không hợp lệ", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var entity = await _context.Receipts
                    .AsTracking()
                    .FirstOrDefaultAsync(p => p.Id == ReceiptDto.Id);

                if (entity == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);

                entity.DeletedAt = DateTime.Now;
                entity.DeletedBy = userId;

                // chỉ lấy detail chưa deleted
                var entity1 = await _context.ReceiptDetails
                    .AsTracking()
                    .Where(d => d.ReceiptId == ReceiptDto.Id)
                    .ToListAsync();

                foreach (var detail in entity1)
                {
                    detail.DeletedAt = DateTime.Now;
                    detail.DeletedBy = userId;
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult<object>(true, "Xóa thành công", null);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi xóa: " + ex.Message, null);
            }
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoReceipt.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        [HttpGet("showWithDebit")]
        public async Task<IActionResult> ShowWithDebit([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoReceipt.ShowWithDebitAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
