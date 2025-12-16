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
using Vudaco.Categorys.Repositories;
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
        private readonly IOffsetRepositories _repoOffset;
        private readonly ILogger<ReceiptController> _logger;
        private readonly VudacoDBContext _context;
        private readonly AdoVudacoDB _db;
        public int userId => (int)HttpContext.Items["UserId"];

        public ReceiptController(ILogger<ReceiptController> logger,AdoVudacoDB db, IReceiptDetailRepositories repoReceiptDetail,IOffsetRepositories repoOffset, IReceiptRepositories repoReceipt, VudacoDBContext context)
        {
            _logger = logger;
            _repoReceiptDetail = repoReceiptDetail;
            _repoReceipt = repoReceipt;
            _repoOffset = repoOffset;
            _context = context;
            _db = db;
        }
        [HttpGet("phieuthu")]
        public async Task<IActionResult> GetPhieuThu(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] ReceiptDto ReceiptDto = null)
        {
            // test
            var result = await _repoReceipt.GetPhieuThuAsync(ReceiptDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("phieuchi")]
        public async Task<IActionResult> GetPhieuChi(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] ReceiptDto ReceiptDto = null)
        {
            // test
            var result = await _repoReceipt.GetPhieuChiAsync(ReceiptDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        } 
        [HttpGet("soquy")]
        public async Task<IActionResult> GetSoQuyAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] ReceiptDto ReceiptDto = null)
        {
            // test
            var result = await _repoReceipt.GetSoQuyAsync(ReceiptDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        } [HttpGet("soquydauky")]
        public async Task<IActionResult> GetSoQuyDKAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] ReceiptDto ReceiptDto = null)
        {
            // test
            var result = await _repoReceipt.GetSoQuyDKAsync(ReceiptDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
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
        [HttpGet("GetSoDuDauKyAsync")]
        public async Task<IActionResult> GetSoDuDauKyAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] ReceiptDto ReceiptDto = null)
        {
            // test
            var result = await _repoReceipt.GetSoDuDauKyAsync(ReceiptDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetChuyenTienNoiBoAsync")]
        public async Task<IActionResult> GetChuyenTienNoiBoAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] OffsetDto OffsetDto = null)
        {
            // test
            var result = await _repoOffset.GetObjectTaskAsync(OffsetDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create/phieuthukh")]
        public async Task<IActionResult> CreatePhieuThuKH([FromBody] ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.Amount <= 0 )
                return ApiResponseResult<object>(false, "Chưa có kiểm tra lại công nợ", null);
            if (ReceiptDto.FormOfPayment == 1 && (ReceiptDto.FundId == null || ReceiptDto.FundId == 0))
                return ApiResponseResult<object>(false, "Mã quỹ bắt buộc", null);
            if (ReceiptDto.FormOfPayment == 2 && (ReceiptDto.BankId == null || ReceiptDto.BankId == 0))
                return ApiResponseResult<object>(false, "Mã Ngân hàng bắt buộc", null);
            // Kiểm tra chi tiết phiếu thu
            if (string.IsNullOrEmpty(ReceiptDto.Debits))
            {
                return ApiResponseResult<object>(false, "Không có chi tiết phiếu thu", null);
            }
            List<JsonElement> list = null;
            try
            {
                list = JsonSerializer.Deserialize<List<JsonElement>>(ReceiptDto.Debits);
            }
            catch
            {
                return ApiResponseResult<object>(false, "Dữ liệu chi tiết phiếu thu không hợp lệ", null);
            }

            if (list == null || list.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không có chi tiết phiếu thu", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                var PrefixCode = "PT"+ReceiptDto.AccountingDate.ToString("yyMM");
                var code_receipt = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", ReceiptDto.StorageId, PrefixCode , 4);

                var entity = new Receipt
                {
                    AccountingDate = ReceiptDto.AccountingDate,
                    StorageId = ReceiptDto.StorageId,
                    CodeReceipt = code_receipt,
                    Note = ReceiptDto.Note,
                    Description =  ReceiptDto.Description,
                    FormOfPayment = ReceiptDto.FormOfPayment,
                    TypeReceipt = ReceiptRepositories.ThuKH,
                    IncomeExpenseCategoryId = 24,//thu kh
                    FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0,
                    BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0,
                    Data = ReceiptDto.Data,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId,
                };

                _context.Receipts.Add(entity);
                await _context.SaveChangesAsync();
                foreach (var item in list)
                {
                    int debit_id = item.GetProperty("id").GetInt32();
                    int conlai_dv = item.GetProperty("conlai_dv").GetInt32();
                    int conlai_ch = item.GetProperty("conlai_ch").GetInt32();
                    int price = item.GetProperty("price").GetInt32();
                    int receipt_total = item.GetProperty("receipt_total").GetInt32();
                    if (price - receipt_total <= 0)
                    {
                        continue;
                    }
                    int new_price = (conlai_dv+conlai_ch) >0 ?(conlai_dv+conlai_ch) :price;
                    // ✔️ Dùng GetDateTime() vì dữ liệu là ISO-8601
                    var accountingDate = item.GetProperty("accounting_date").GetDateTime();
                    var entity_detail = new ReceiptDetail
                    {
                        ReceiptId = entity.Id,
                        DebitId = debit_id,
                        StorageId = ReceiptDto.StorageId,
                        AccountingDate = accountingDate,
                        Amount = new_price,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.ReceiptDetails.Add(entity_detail);
                }
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
        [HttpPost]
        [Route("create/phieuchincc")]
        public async Task<IActionResult> CreatePhieuChiNCC([FromBody] ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.Amount <= 0 )
                return ApiResponseResult<object>(false, "Chưa có kiểm tra lại công nợ", null);
            if (ReceiptDto.FormOfPayment == 1 && (ReceiptDto.FundId == null || ReceiptDto.FundId == 0))
                return ApiResponseResult<object>(false, "Mã quỹ bắt buộc", null);
            if (ReceiptDto.FormOfPayment == 2 && (ReceiptDto.BankId == null || ReceiptDto.BankId == 0))
                return ApiResponseResult<object>(false, "Mã Ngân hàng bắt buộc", null);
            // Kiểm tra chi tiết phiếu thu
            if (string.IsNullOrEmpty(ReceiptDto.Debits))
            {
                return ApiResponseResult<object>(false, "Không có chi tiết phiếu thu", null);
            }
            List<JsonElement> list = null;
            try
            {
                list = JsonSerializer.Deserialize<List<JsonElement>>(ReceiptDto.Debits);
            }
            catch
            {
                return ApiResponseResult<object>(false, "Dữ liệu chi tiết phiếu thu không hợp lệ", null);
            }

            if (list == null || list.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không có chi tiết phiếu thu", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                var PrefixCode = "PC"+ReceiptDto.AccountingDate.ToString("yyMM");
                var code_receipt = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", ReceiptDto.StorageId, PrefixCode , 4);

                var entity = new Receipt
                {
                    AccountingDate = ReceiptDto.AccountingDate,
                    StorageId = ReceiptDto.StorageId,
                    CodeReceipt = code_receipt,
                    Note = ReceiptDto.Note,
                    Description =  ReceiptDto.Description,
                    FormOfPayment = ReceiptDto.FormOfPayment,
                    TypeReceipt = ReceiptRepositories.ChiNCC,
                    IncomeExpenseCategoryId = 25,//Chi NCC
                    FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0,
                    BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0,
                    Data = ReceiptDto.Data,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId,
                };

                _context.Receipts.Add(entity);
                await _context.SaveChangesAsync();
                foreach (var item in list)
                {
                    int debit_id = item.GetProperty("id").GetInt32();
                    int conlai_dv = item.GetProperty("conlai_dv").GetInt32();
                    int conlai_ch = item.GetProperty("conlai_ch").GetInt32();
                    int purchase_price = item.GetProperty("purchase_price").GetInt32();
                    int receipt_total = item.GetProperty("receipt_total").GetInt32();
                    if (purchase_price - receipt_total <= 0)
                    {
                        continue;
                    }
                    int new_price = (conlai_dv+conlai_ch) >0 ?(conlai_dv+conlai_ch) :purchase_price;
                    // ✔️ Dùng GetDateTime() vì dữ liệu là ISO-8601
                    var accountingDate = item.GetProperty("accounting_date").GetDateTime();
                    var entity_detail = new ReceiptDetail
                    {
                        ReceiptId = entity.Id,
                        DebitId = debit_id,
                        StorageId = ReceiptDto.StorageId,
                        AccountingDate = accountingDate,
                        Amount = new_price,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.ReceiptDetails.Add(entity_detail);
                }
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
                    Object = ReceiptRepositories.DoiTuongNV,
                    ObjectId = ReceiptHoanUngGiaoNhanDto.EmployeeId,
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
                    CreatedAt = now,
                    UpdatedAt = now

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
        [Route("create/chinoibo")]
        public async Task<IActionResult> CreateChiNoiBo([FromBody] ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.Object == null || ReceiptDto.ObjectId == 0)
                return ApiResponseResult<object>(false, "Doi tuong bắt buộc", null);
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
                var now = DateTime.Now;
                entity = new Receipt
                {
                    AccountingDate = ReceiptDto.AccountingDate,
                    StorageId = ReceiptDto.StorageId,
                    CodeReceipt = code_receipt,
                    Object = ReceiptDto.Object,
                    ObjectId = ReceiptDto.ObjectId,
                    Bill = ReceiptDto.Bill,
                    FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0,
                    IncomeExpenseCategoryId = ReceiptDto.IncomeExpenseCategoryId,
                    Note = ReceiptDto.Note,
                    FormOfPayment = ReceiptDto.FormOfPayment,
                    TypeReceipt = ReceiptRepositories.ChiNoiBo,
                    BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0,
                    Data = ReceiptDto.Data,
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
                    StorageId = ReceiptDto.StorageId,
                    AccountingDate = ReceiptDto.AccountingDate,
                    Amount = ReceiptDto.Amount,
                    Vat = ReceiptDto.Vat,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now

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
        [HttpPost("update/chinoibo")]
        public async Task<IActionResult> UpdateChiNoiBo([FromBody] ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.ObjectId == null || ReceiptDto.Object == 0)
                return ApiResponseResult<object>(false, "Doi tuong bắt buộc", null);
            if (ReceiptDto.FormOfPayment == 1 && (ReceiptDto.FundId == null || ReceiptDto.FundId == 0))
                return ApiResponseResult<object>(false, "Mã quỹ bắt buộc", null);
            if (ReceiptDto.FormOfPayment == 2 && (ReceiptDto.BankId == null || ReceiptDto.BankId == 0))
                return ApiResponseResult<object>(false, "Mã Ngân hàng bắt buộc", null);
            if (ReceiptDto.IncomeExpenseCategoryId == null || ReceiptDto.IncomeExpenseCategoryId == 0)
                return ApiResponseResult<object>(false, "Lý do chi bắt buộc", null);

            // Tìm entity hiện có
            var entity = await _context.Receipts.FirstOrDefaultAsync(p => p.Id == ReceiptDto.Id);
            if (entity == null)
                return ApiResponseResult<object>(false, "Không tìm thấy phiếu chi giao nhận", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                // Cập nhật thông tin phiếu chi
                entity.AccountingDate = ReceiptDto.AccountingDate;
                entity.StorageId = ReceiptDto.StorageId;
                entity.Object = ReceiptDto.Object;
                entity.ObjectId = ReceiptDto.ObjectId;
                entity.Bill = ReceiptDto.Bill;
                entity.FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0;
                entity.IncomeExpenseCategoryId = ReceiptDto.IncomeExpenseCategoryId;
                entity.Note = ReceiptDto.Note;
                entity.FormOfPayment = ReceiptDto.FormOfPayment;
                entity.BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0;
                entity.Data = ReceiptDto.Data;
                entity.UpdatedAt = now;
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
                    detail.UpdatedAt = now;
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
                        CreatedAt = now,
                        UpdatedAt = now,
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
        [HttpPost]
        [Route("create/chuyentiennoibo")]
        public async Task<IActionResult> CreateChuyenTienNoiBo([FromBody] ReceiptDto ReceiptDto)
        {
            // chuyển từ là chi
            if (ReceiptDto.ChuyenTu.FormOfPayment == 1 && (ReceiptDto.ChuyenTu.FundId == null || ReceiptDto.ChuyenTu.FundId == 0))
                return ApiResponseResult<object>(false, "Chuyển từ! Mã quỹ bắt buộc", null);
            if (ReceiptDto.ChuyenTu.FormOfPayment == 2 && (ReceiptDto.ChuyenTu.BankId == null || ReceiptDto.ChuyenTu.BankId == 0))
                return ApiResponseResult<object>(false, "Chuyển từ! Mã Ngân hàng bắt buộc", null);
            // chuyển đến là thu
            if (ReceiptDto.ChuyenDen.FormOfPayment == 1 && (ReceiptDto.ChuyenDen.FundId == null || ReceiptDto.ChuyenDen.FundId == 0))
                return ApiResponseResult<object>(false, "Chuyển từ! Mã quỹ bắt buộc", null);
            if (ReceiptDto.ChuyenDen.FormOfPayment == 2 && (ReceiptDto.ChuyenDen.BankId == null || ReceiptDto.ChuyenDen.BankId == 0))
                return ApiResponseResult<object>(false, "Chuyển từ! Mã Ngân hàng bắt buộc", null);
            // Check trùng Name
            var entity = await _context.Receipts.FirstOrDefaultAsync(p => p.CodeReceipt == ReceiptDto.CodeReceipt);
            if (entity != null)
                return ApiResponseResult<object>(false, "ma phieu thu đã tồn tại", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                 var now = DateTime.Now;
                 var chuyentiennoibo = new Offset
                 {
                     AccountingDate =ReceiptDto.AccountingDate,
                     StorageId =ReceiptDto.StorageId,
                     Type = OffsetRepositories.ChuyenTienNoiBo,
                     Price = ReceiptDto.Amount,
                     Note = ReceiptDto.Note,
                     CreatedBy = userId,
                     CreatedAt = now,
                     UpdatedAt = now,
                     UpdatedBy = userId,
                 };
                 _context.Offsets.Add(chuyentiennoibo);
                await _context.SaveChangesAsync();
                var code_receipt = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", ReceiptDto.StorageId, "PCCT"+ReceiptDto.AccountingDate.ToString("yyMM"), 4);
             
                entity = new Receipt
                {
                    AccountingDate = ReceiptDto.AccountingDate,
                    StorageId = ReceiptDto.StorageId,
                    CodeReceipt = code_receipt,
                    FundId = ReceiptDto.ChuyenTu.FormOfPayment == 1 ? ReceiptDto.ChuyenTu.FundId : 0,
                    IncomeExpenseCategoryId = IncomeExpenseCategoryRepository.ChiChuyenTienNoiBo,
                    Note = ReceiptDto.Note,
                    FormOfPayment = ReceiptDto.ChuyenTu.FormOfPayment,
                    BankId = ReceiptDto.ChuyenTu.FormOfPayment == 2 ? ReceiptDto.ChuyenTu.BankId : 0,
                    Data = ReceiptDto.Data,
                    OffsetId = chuyentiennoibo.Id,
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
                    StorageId = ReceiptDto.StorageId,
                    AccountingDate = ReceiptDto.AccountingDate,
                    Amount = ReceiptDto.Amount,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _context.ReceiptDetails.Add(entity_detail);
                await _context.SaveChangesAsync();
                var code_receipt_thu = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", ReceiptDto.StorageId, "PTCT"+ReceiptDto.AccountingDate.ToString("yyMM"), 4);
                var entity_thu = new Receipt
                {
                    AccountingDate = ReceiptDto.AccountingDate,
                    StorageId = ReceiptDto.StorageId,
                    CodeReceipt = code_receipt_thu,
                    FundId = ReceiptDto.ChuyenDen.FormOfPayment == 1 ? ReceiptDto.ChuyenDen.FundId : 0,
                    IncomeExpenseCategoryId = IncomeExpenseCategoryRepository.ThuChuyenTienNoiBo,
                    Note = ReceiptDto.Note,
                    FormOfPayment = ReceiptDto.ChuyenDen.FormOfPayment,
                    BankId = ReceiptDto.ChuyenDen.FormOfPayment == 2 ? ReceiptDto.ChuyenDen.BankId : 0,
                    Data = ReceiptDto.Data,
                    OffsetId = chuyentiennoibo.Id,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId,
                };

                _context.Receipts.Add(entity_thu);
                await _context.SaveChangesAsync();
                var entity_detail_thu = new ReceiptDetail
                {
                    ReceiptId = entity_thu.Id,
                    StorageId = ReceiptDto.StorageId,
                    AccountingDate = ReceiptDto.AccountingDate,
                    Amount = ReceiptDto.Amount,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _context.ReceiptDetails.Add(entity_detail_thu);
                await _context.SaveChangesAsync();
                chuyentiennoibo.AReceiptId = entity.Id;
                chuyentiennoibo.BReceiptId = entity_thu.Id;
                _context.Offsets.Update(chuyentiennoibo);
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
        [HttpPost]
        [Route("update/chuyentiennoibo")]
        public async Task<IActionResult> UpdateChuyenTienNoiBo([FromBody] ReceiptDto ReceiptDto)
        {
            // chuyển từ là chi
            if (ReceiptDto.ChuyenTu.FormOfPayment == 1 && (ReceiptDto.ChuyenTu.FundId == null || ReceiptDto.ChuyenTu.FundId == 0))
                return ApiResponseResult<object>(false, "Chuyển từ! Mã quỹ bắt buộc", null);
            if (ReceiptDto.ChuyenTu.FormOfPayment == 2 && (ReceiptDto.ChuyenTu.BankId == null || ReceiptDto.ChuyenTu.BankId == 0))
                return ApiResponseResult<object>(false, "Chuyển từ! Mã Ngân hàng bắt buộc", null);
            // chuyển đến là thu
            if (ReceiptDto.ChuyenDen.FormOfPayment == 1 && (ReceiptDto.ChuyenDen.FundId == null || ReceiptDto.ChuyenDen.FundId == 0))
                return ApiResponseResult<object>(false, "Chuyển đến! Mã quỹ bắt buộc", null);
            if (ReceiptDto.ChuyenDen.FormOfPayment == 2 && (ReceiptDto.ChuyenDen.BankId == null || ReceiptDto.ChuyenDen.BankId == 0))
                return ApiResponseResult<object>(false, "Chuyển đến! Mã Ngân hàng bắt buộc", null);
            // Check trùng Name
            var entity = await _context.Receipts.FirstOrDefaultAsync(p => p.CodeReceipt == ReceiptDto.CodeReceipt);
            if (entity != null) return ApiResponseResult<object>(false, "ma phieu thu đã tồn tại", null);
            bool checkDel = await deleteChuyenTienNoiBo(ReceiptDto.Id);
            if (checkDel == false) return ApiResponseResult<object>(false, "có lỗi xảy ra khi xóa", null);
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                 var now = DateTime.Now;
                 var chuyentiennoibo = new Offset
                 {
                     AccountingDate =ReceiptDto.AccountingDate,
                     StorageId =ReceiptDto.StorageId,
                     Type = OffsetRepositories.ChuyenTienNoiBo,
                     Price = ReceiptDto.Amount,
                     Note = ReceiptDto.Note,
                     CreatedBy = userId,
                     CreatedAt = now,
                     UpdatedAt = now,
                     UpdatedBy = userId,
                 };
                 _context.Offsets.Add(chuyentiennoibo);
                await _context.SaveChangesAsync();
                var code_receipt = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", ReceiptDto.StorageId, "PCCT"+ReceiptDto.AccountingDate.ToString("yyMM"), 4);
             
                entity = new Receipt
                {
                    AccountingDate = ReceiptDto.AccountingDate,
                    StorageId = ReceiptDto.StorageId,
                    CodeReceipt = code_receipt,
                    FundId = ReceiptDto.ChuyenTu.FormOfPayment == 1 ? ReceiptDto.ChuyenTu.FundId : 0,
                    IncomeExpenseCategoryId = IncomeExpenseCategoryRepository.ChiChuyenTienNoiBo,
                    Note = ReceiptDto.Note,
                    FormOfPayment = ReceiptDto.ChuyenTu.FormOfPayment,
                    BankId = ReceiptDto.ChuyenTu.FormOfPayment == 2 ? ReceiptDto.ChuyenTu.BankId : 0,
                    Data = ReceiptDto.Data,
                    OffsetId = chuyentiennoibo.Id,
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
                    StorageId = ReceiptDto.StorageId,
                    AccountingDate = ReceiptDto.AccountingDate,
                    Amount = ReceiptDto.Amount,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _context.ReceiptDetails.Add(entity_detail);
                await _context.SaveChangesAsync();
                var code_receipt_thu = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", ReceiptDto.StorageId, "PTCT"+ReceiptDto.AccountingDate.ToString("yyMM"), 4);
                var entity_thu = new Receipt
                {
                    AccountingDate = ReceiptDto.AccountingDate,
                    StorageId = ReceiptDto.StorageId,
                    CodeReceipt = code_receipt_thu,
                    FundId = ReceiptDto.ChuyenDen.FormOfPayment == 1 ? ReceiptDto.ChuyenDen.FundId : 0,
                    IncomeExpenseCategoryId = IncomeExpenseCategoryRepository.ThuChuyenTienNoiBo,
                    Note = ReceiptDto.Note,
                    FormOfPayment = ReceiptDto.ChuyenDen.FormOfPayment,
                    BankId = ReceiptDto.ChuyenDen.FormOfPayment == 2 ? ReceiptDto.ChuyenDen.BankId : 0,
                    Data = ReceiptDto.Data,
                    OffsetId = chuyentiennoibo.Id,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId,
                };

                _context.Receipts.Add(entity_thu);
                await _context.SaveChangesAsync();
                var entity_detail_thu = new ReceiptDetail
                {
                    ReceiptId = entity_thu.Id,
                    StorageId = ReceiptDto.StorageId,
                    AccountingDate = ReceiptDto.AccountingDate,
                    Amount = ReceiptDto.Amount,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _context.ReceiptDetails.Add(entity_detail_thu);
                await _context.SaveChangesAsync();
                chuyentiennoibo.AReceiptId = entity.Id;
                chuyentiennoibo.BReceiptId = entity_thu.Id;
                _context.Offsets.Update(chuyentiennoibo);
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
        [HttpPost]
        [Route("create/sodudauky")]
        public async Task<IActionResult> CreateSoDuDauKy([FromBody] ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.FormOfPayment == 1 && (ReceiptDto.FundId == null || ReceiptDto.FundId == 0))
                return ApiResponseResult<object>(false, "Mã quỹ bắt buộc", null);
            if (ReceiptDto.FormOfPayment == 2 && (ReceiptDto.BankId == null || ReceiptDto.BankId == 0))
                return ApiResponseResult<object>(false, "Mã Ngân hàng bắt buộc", null);
            // Check trùng Name
            var entity = await _context.Receipts.FirstOrDefaultAsync(p => p.CodeReceipt == ReceiptDto.CodeReceipt);
            if (entity != null)
                return ApiResponseResult<object>(false, "ma phieu thu đã tồn tại", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var code_receipt = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", ReceiptDto.StorageId, "PTDK"+ReceiptDto.AccountingDate.ToString("yyMM"), 4);
                var now = DateTime.Now;
                entity = new Receipt
                {
                    AccountingDate = ReceiptDto.AccountingDate,
                    StorageId = ReceiptDto.StorageId,
                    CodeReceipt = code_receipt,
                    FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0,
                    IncomeExpenseCategoryId = IncomeExpenseCategoryRepository.ThuDK,
                    Note = ReceiptDto.Note,
                    FormOfPayment = ReceiptDto.FormOfPayment,
                    BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0,
                    Data = ReceiptDto.Data,
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
                    StorageId = ReceiptDto.StorageId,
                    AccountingDate = ReceiptDto.AccountingDate,
                    Amount = ReceiptDto.Amount,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now

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
        [HttpPost("update/sodudauky")]
        public async Task<IActionResult> UpdateSoDuDauKy([FromBody] ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.FormOfPayment == 1 && (ReceiptDto.FundId == null || ReceiptDto.FundId == 0))
                return ApiResponseResult<object>(false, "Mã quỹ bắt buộc", null);
            if (ReceiptDto.FormOfPayment == 2 && (ReceiptDto.BankId == null || ReceiptDto.BankId == 0))
                return ApiResponseResult<object>(false, "Mã Ngân hàng bắt buộc", null);

            // Tìm entity hiện có
            var entity = await _context.Receipts.FirstOrDefaultAsync(p => p.Id == ReceiptDto.Id);
            if (entity == null)
                return ApiResponseResult<object>(false, "Không tìm thấy phiếu", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                  var now = DateTime.Now;
                // Cập nhật thông tin phiếu chi
                entity.AccountingDate = ReceiptDto.AccountingDate;
                entity.StorageId = ReceiptDto.StorageId;
                entity.FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0;
                entity.Note = ReceiptDto.Note;
                entity.FormOfPayment = ReceiptDto.FormOfPayment;
                entity.BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0;
                entity.Data = ReceiptDto.Data;
                entity.UpdatedAt = now;
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
                    detail.UpdatedAt = now;
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
                        UpdatedAt = now,
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
                var now = DateTime.Now;
                entity = new Receipt
                {
                    AccountingDate = ReceiptDto.AccountingDate,
                    StorageId = ReceiptDto.StorageId,
                    CodeReceipt = code_receipt,
                    FileInfoId = ReceiptDto.FileInfoId,
                    EmployeeId = ReceiptDto.EmployeeId,
                    Object = ReceiptRepositories.DoiTuongNV,
                    ObjectId = ReceiptDto.EmployeeId,
                    Bill = ReceiptDto.Bill,
                    FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0,
                    IncomeExpenseCategoryId = ReceiptDto.IncomeExpenseCategoryId,
                    Note = ReceiptDto.Note,
                    FormOfPayment = ReceiptDto.FormOfPayment,
                    TypeReceipt = ReceiptRepositories.ChiGiaoNhan,
                    BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0,
                    Data = ReceiptDto.Data,
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
                    StorageId = ReceiptDto.StorageId,
                    AccountingDate = ReceiptDto.AccountingDate,
                    Amount = ReceiptDto.Amount,
                    Vat = ReceiptDto.Vat,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now

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
        public async Task<IActionResult> UpdateChiGiaoNhan([FromBody] ReceiptDto ReceiptDto)
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
            var entity = await _context.Receipts.FirstOrDefaultAsync(p => p.Id == ReceiptDto.Id);
            if (entity == null)
                return ApiResponseResult<object>(false, "Không tìm thấy phiếu chi giao nhận", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                // Cập nhật thông tin phiếu chi
                entity.AccountingDate = ReceiptDto.AccountingDate;
                entity.StorageId = ReceiptDto.StorageId;
                entity.FileInfoId = ReceiptDto.FileInfoId;
                entity.EmployeeId = ReceiptDto.EmployeeId;
                entity.ObjectId = ReceiptDto.EmployeeId;
                entity.Bill = ReceiptDto.Bill;
                entity.FundId = ReceiptDto.FormOfPayment == 1 ? ReceiptDto.FundId : 0;
                entity.IncomeExpenseCategoryId = ReceiptDto.IncomeExpenseCategoryId;
                entity.Note = ReceiptDto.Note;
                entity.FormOfPayment = ReceiptDto.FormOfPayment;
                entity.BankId = ReceiptDto.FormOfPayment == 2 ? ReceiptDto.BankId : 0;
                entity.Data = ReceiptDto.Data;
                entity.UpdatedAt = now;
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
                    detail.UpdatedAt = now;
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
                        CreatedAt = now,
                        UpdatedAt = now,
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
        [HttpPost("confirmCancelGiaoNhan")]
        public async Task<IActionResult> ConfirmCancelGiaoNhan([FromBody]  ReceiptDto ReceiptDto)
        {
             if (ReceiptDto.Id <= 0)
                return ApiResponseResult<object>(false, "Id không hợp lệ", null);
             var entity = await _context.Receipts
                    .AsTracking()
                    .FirstOrDefaultAsync(p => p.Id == ReceiptDto.Id);

                if (entity == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            entity.Status = 0;
            entity.Note = null;
            entity.UpdatedAt = DateTime.Now;
            entity.UpdatedBy = userId;
            await _context.SaveChangesAsync();
             return ApiResponseResult<object>(true, "Xóa thành công", null);
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
         [HttpPost("delete/chuyentiennoibo")]
        public async Task<IActionResult> DelChuyenTienNoiBo([FromBody]  ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.Id <= 0)
                return ApiResponseResult<object>(false, "Id không hợp lệ", null);
            var entity = await _context.Offsets.AsTracking().FirstOrDefaultAsync(p => p.Id == ReceiptDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            try
            {
                bool checkDel = await deleteChuyenTienNoiBo(ReceiptDto.Id);
                if (checkDel == true)
                {
                     return ApiResponseResult<object>(true, "Xóa thành công", null);
                }else
                {
                     return ApiResponseResult<object>(false, "Xóa thất bại", null);
                }
               
            }
            catch (Exception ex)
            {
                return ApiResponseResult<object>(false, "Lỗi khi xóa: " + ex.Message, null);
            }
        }
        [HttpGet("showChuyenTienNoiBo")]
        public async Task<IActionResult> ShowChuyenTienNoiBo([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoOffset.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        public async Task<bool> deleteChuyenTienNoiBo(int OffsetId)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;

                var offset = await _context.Offsets
                    .AsTracking()
                    .FirstOrDefaultAsync(x => x.Id == OffsetId && x.DeletedAt == null);

                if (offset == null) return false;

                offset.DeletedAt = now;
                offset.DeletedBy = userId;

                var receipts = await _context.Receipts
                    .AsTracking()
                    .Where(x => x.OffsetId == OffsetId && x.DeletedAt == null)
                    .ToListAsync();

                var receiptIds = receipts.Select(x => x.Id).ToList();

                var receiptDetails = await _context.ReceiptDetails
                    .AsTracking()
                    .Where(x => receiptIds.Contains(x.ReceiptId) && x.DeletedAt == null)
                    .ToListAsync();

                foreach (var r in receipts)
                {
                    r.DeletedAt = now;
                    r.DeletedBy = userId;
                }

                foreach (var d in receiptDetails)
                {
                    d.DeletedAt = now;
                    d.DeletedBy = userId;
                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return true;
            }
            catch
            {
                await tran.RollbackAsync();
                return false;
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
