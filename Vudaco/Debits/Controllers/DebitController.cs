using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Bills.Models;
using Vudaco.Controllers;
using Vudaco.Debits.Dtos;
using Vudaco.Debits.Models;
using Vudaco.Debits.Repositories;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;
using System.Text.Json;
using Vudaco.ContractFiles.Repositories;
using Vudaco.ContractFiles.Dtos;
using Vudaco.Receipts.Models;
using Vudaco.Receipts.Repositories;

namespace Vudaco.Debits.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebitController : BaseApiController
    {
        private readonly IDebitRepositories _repoDebit;
        private readonly IContractFileDetailRepository _repoContractFileDetail;
        private readonly ILogger<DebitController> _logger;
        private readonly VudacoDBContext _context;

        private readonly IConfiguration _configuration;
        public int userId => (int)HttpContext.Items["UserId"];

        public DebitController(ILogger<DebitController> logger,  IContractFileDetailRepository repoContractFileDetail,IConfiguration configuration, IDebitRepositories repoDebit, VudacoDBContext context)
        {
            _logger = logger;
            _repoDebit = repoDebit;
            _context = context;
            _configuration = configuration;
            _repoContractFileDetail = repoContractFileDetail;
        }
        [HttpGet("noDebitNoFileDispatchKH")]
        public async Task<IActionResult> GetNoDebitNoFileDispatchKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectNoDebitDispatchNoFileKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("hasDebitNoFileDispatchKH")]
        public async Task<IActionResult> GetHasDebitNoFileDispatchKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectHasDebitDispatchNoFileKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("congnochitietkh")]
        public async Task<IActionResult> GetCongNoChiTietKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitChiTietKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
         [HttpGet("congnochitietncc")]
        public async Task<IActionResult> GetCongNoChiTietNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitChiTietNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("dispatch")]
        public async Task<IActionResult> GetTaskDispatch(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDetailDto FileInfoDetailDto = null)
        {
            // test
            var result = await _repoContractFileDetail.GetObjectFileHasDispatchAsync(FileInfoDetailDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create/muaban")]
        public async Task<IActionResult> CreateMuaBan([FromBody] DebitMuaBanDto DebitMuaBanDto)
        {
            if (DebitMuaBanDto.FormOfPayment == 1 && (DebitMuaBanDto.FundId == null || DebitMuaBanDto.FundId == 0))
                return ApiResponseResult<object>(false, "Mã quỹ bắt buộc", null);
            if (DebitMuaBanDto.FormOfPayment == 2 && (DebitMuaBanDto.BankId == null || DebitMuaBanDto.BankId == 0))
                return ApiResponseResult<object>(false, "Mã Ngân hàng bắt buộc", null);
            if (DebitMuaBanDto.IncomeExpenseCategoryId == null || DebitMuaBanDto.IncomeExpenseCategoryId == 0)
                return ApiResponseResult<object>(false, "ly do bắt buộc", null);
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitMuaBanDto.AccountingDate.ToString("MMyyyy"));
                var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.CustomerDetailId == DebitMuaBanDto.CustomerDetailId);
                if (bill_Partner == null)
                {
                    var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitMuaBanDto.StorageId, "HD"+DebitMuaBanDto.AccountingDate.ToString("yyMM"),4);
                    bill_Partner = new Bill
                    {
                        BillCode = BillCodePartner,
                        StorageId = DebitMuaBanDto.StorageId,
                        CustomerDetailId =  DebitMuaBanDto.CustomerDetailId,
                        Name = CycleName.ToString(),
                        AccountingDate = DebitMuaBanDto.AccountingDate,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Bills.Add(bill_Partner);
                    await _context.SaveChangesAsync();  // phải có
                }
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn,tran.GetDbTransaction(), "debits", "dispatch_code", DebitMuaBanDto.StorageId, "BHKH"+DebitMuaBanDto.AccountingDate.ToString("yyMM"),4);

                var debit = new Debit
                {
                    BillId = bill_Partner.Id,
                    CustomerDetailId = DebitMuaBanDto.CustomerDetailId,
                    StorageId = DebitMuaBanDto.StorageId,
                    Type = DebitRepositories.BanHangKH,
                    DispatchCode = DispatchCode,
                    Name = DebitMuaBanDto.Note,
                    AccountingDate = DebitMuaBanDto.AccountingDate,
                    PurchasePrice = DebitMuaBanDto.Price,
                    Price = DebitMuaBanDto.Price,
                    Data = DebitMuaBanDto.Data,
                    Note = DebitMuaBanDto.Note,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    StorageId = DebitMuaBanDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitMuaBanDto.CustomerDetailId,
                    Status = ContractFileRepository.statusDebit,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
                await _context.SaveChangesAsync();

                 var code_receipt = await SqlServerHelpers.GenerateCodeEfAsync(conn, tran.GetDbTransaction(), "receipts", "code_receipt", DebitMuaBanDto.StorageId, "PT"+DebitMuaBanDto.AccountingDate.ToString("yyMM"), 4);

                var receipt = new Receipt
                {
                    AccountingDate = DebitMuaBanDto.AccountingDate,
                    StorageId = DebitMuaBanDto.StorageId,
                    CodeReceipt = code_receipt,
                    FundId = DebitMuaBanDto.FormOfPayment == 1 ? DebitMuaBanDto.FundId : 0,
                    IncomeExpenseCategoryId = DebitMuaBanDto.IncomeExpenseCategoryId,
                    Note = DebitMuaBanDto.Note,
                    FormOfPayment = DebitMuaBanDto.FormOfPayment,
                    TypeReceipt = ReceiptRepositories.ThuBanHangKH,
                    BankId = DebitMuaBanDto.FormOfPayment == 2 ? DebitMuaBanDto.BankId : 0,
                    Data = DebitMuaBanDto.Data,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId,
                };

                _context.Receipts.Add(receipt);
                await _context.SaveChangesAsync();
                var entity_detail = new ReceiptDetail
                {
                    ReceiptId = receipt.Id,
                    StorageId = DebitMuaBanDto.StorageId,
                    DebitId = debit.Id,
                    AccountingDate = DebitMuaBanDto.AccountingDate,
                    Amount = DebitMuaBanDto.Price,
                    Vat = DebitMuaBanDto.Vat,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId,

                };
                _context.ReceiptDetails.Add(entity_detail);
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
        [HttpGet("muaban")]
        public async Task<IActionResult> GetTaskMuaBan(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitMuaBanAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("cuoctamthu")]
        public async Task<IActionResult> GetTaskCuocTamThu(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitCuocTamThuAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("muahangNCC")]
        public async Task<IActionResult> GetTaskMuaHangNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectMuaHangNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("banhangKH")]
        public async Task<IActionResult> GetTaskBanHangKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectBanHangKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("daukykh")]
        public async Task<IActionResult> GetTaskDauKyKH(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitDauKyKHAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("daukyncc")]
        public async Task<IActionResult> GetTaskDauKyNCC(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitDauKyNCCAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectTaskAsync(DebitDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] DebitDto DebitDto)
        {
            //if (string.IsNullOrEmpty(DebitDto.VehicleNumber))
            //{
            //    return ApiResponseResult<object>(false, "Chưa nhập biển số xe", null);
            //}
            if (!DebitDto.CustomerDetailId.HasValue || DebitDto.CustomerDetailId <= 0)
            {
                return ApiResponseResult<object>(false, "Không được để trống khách hàng", null);
            }
            //if (!DebitDto.EmployeeDriverId.HasValue || DebitDto.EmployeeDriverId <= 0)
            //{
            //    return ApiResponseResult<object>(false, "Không được để trống lai xe", null);
            //}
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.CustomerDetailId == DebitDto.CustomerDetailId);
                if (bill_Partner == null)
                {
                    var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD"+DebitDto.AccountingDate.ToString("yyMM"),4);
                    bill_Partner = new Bill
                    {
                        BillCode = BillCodePartner,
                        StorageId = DebitDto.StorageId,
                        CustomerDetailId =  DebitDto.CustomerDetailId,
                        Name = CycleName.ToString(),
                        AccountingDate = DebitDto.AccountingDate,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Bills.Add(bill_Partner);
                    await _context.SaveChangesAsync();  // phải có
                }
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn,tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "KS"+DebitDto.AccountingDate.ToString("yyMM"),4);

                var debit = new Debit
                {
                    BillId = bill_Partner.Id,
                    VehicleId = DebitDto.VehicleId,
                    VehicleNumber = DebitDto.VehicleNumber,
                    CustomerDetailId = DebitDto.CustomerDetailId,
                    SupplierDetailId = DebitDto.SupplierDetailId,
                    EmployeeDriverId = DebitDto.EmployeeDriverId,
                    EmployeeStaffId = DebitDto.EmployeeStaffId,
                    StorageId = DebitDto.StorageId,
                    Type = DebitRepositories.PhiVanChuyen,
                    DispatchCode = DispatchCode,
                    Name = DebitDto.Route,
                    AccountingDate = DebitDto.AccountingDate,
                    PurchasePrice = DebitDto.PurchasePrice,
                    Price = DebitDto.Price,
                    Vat = DebitDto.Vat,
                    DriverFee = DebitDto.DriverFee,
                    MealFee = DebitDto.MealFee,
                    TicketFee = DebitDto.TicketFee,
                    OvernightFee = DebitDto.OvernightFee,
                    PenaltyFee = DebitDto.PenaltyFee,
                    GoodsFee = DebitDto.GoodsFee,
                    Data = DebitDto.Data,
                    Note = DebitDto.Note,
                    CustomerVehicleType = DebitDto.CustomerVehicleType,
                    SupplierVehicleType = DebitDto.SupplierVehicleType,
                    PurchaseStatus = DebitDto.PurchaseStatus,
                    PurchaseVat = DebitDto.PurchaseVat,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                if (DebitDto.FileInfoId > 0)debit.FileInfoId = DebitDto.FileInfoId;
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    FileInfoId = DebitDto.FileInfoId,
                    StorageId = DebitDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitDto.CustomerDetailId,
                    Status = ContractFileRepository.statusDichVu,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
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
        [HttpPost]
        [Route("create/muahang")]
        public async Task<IActionResult> CreateMuaHang([FromBody] DebitDto DebitDto)
        {

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.SupplierDetailId == DebitDto.SupplierDetailId);
                if (bill_Partner == null)
                {
                    var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD" + DebitDto.AccountingDate.ToString("yyMM"), 4);
                    bill_Partner = new Bill
                    {
                        BillCode = BillCodePartner,
                        StorageId = DebitDto.StorageId,
                        SupplierDetailId = DebitDto.SupplierDetailId,
                        Name = CycleName.ToString(),
                        AccountingDate = DebitDto.AccountingDate,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Bills.Add(bill_Partner);
                    await _context.SaveChangesAsync();  // phải có
                }
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "MH" + DebitDto.AccountingDate.ToString("yyMM"), 4);

                var debit = new Debit
                {
                    BillId = bill_Partner.Id,
                    SupplierDetailId = DebitDto.SupplierDetailId,
                    StorageId = DebitDto.StorageId,
                    Type = DebitRepositories.MuaHangNCC,
                    DispatchCode = DispatchCode,
                    Name = DebitDto.Note,
                    AccountingDate = DebitDto.AccountingDate,
                    PurchasePrice = DebitDto.Price,
                    Price = DebitDto.Price,
                    Vat = DebitDto.Vat,
                    Status = ContractFileRepository.statusDebit,
                    Data = DebitDto.Data,
                    Note = DebitDto.Note,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    StorageId = DebitDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitDto.SupplierDetailId,
                    Status = ContractFileRepository.statusDebit,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
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
        [HttpPost]
        [Route("create/banhang")]
        public async Task<IActionResult> CreateBanHang([FromBody] DebitDto DebitDto)
        {

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.CustomerDetailId == DebitDto.CustomerDetailId);
                if (bill_Partner == null)
                {
                    var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD" + DebitDto.AccountingDate.ToString("yyMM"), 4);
                    bill_Partner = new Bill
                    {
                        BillCode = BillCodePartner,
                        StorageId = DebitDto.StorageId,
                        CustomerDetailId = DebitDto.CustomerDetailId,
                        Name = CycleName.ToString(),
                        AccountingDate = DebitDto.AccountingDate,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Bills.Add(bill_Partner);
                    await _context.SaveChangesAsync();  // phải có
                }
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "BH" + DebitDto.AccountingDate.ToString("yyMM"), 4);

                var debit = new Debit
                {
                    BillId = bill_Partner.Id,
                    CustomerDetailId = DebitDto.CustomerDetailId,
                    StorageId = DebitDto.StorageId,
                    Type = DebitRepositories.BanHangKH,
                    DispatchCode = DispatchCode,
                    Name = DebitDto.Note,
                    AccountingDate = DebitDto.AccountingDate,
                    PurchasePrice = DebitDto.Price,
                    Price = DebitDto.Price,
                    Vat = DebitDto.Vat,
                    Status = ContractFileRepository.statusDebit,
                    Data = DebitDto.Data,
                    Note = DebitDto.Note,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    StorageId = DebitDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitDto.CustomerDetailId,
                    Status = ContractFileRepository.statusDebit,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
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
        [HttpPost]
        [Route("create/daukykh")]
        public async Task<IActionResult> CreateDauKyKH([FromBody] DebitDto DebitDto)
        {

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.CustomerDetailId == DebitDto.CustomerDetailId);
                if (bill_Partner == null)
                {
                    var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD"+DebitDto.AccountingDate.ToString("yyMM"),4);
                    bill_Partner = new Bill
                    {
                        BillCode = BillCodePartner,
                        StorageId = DebitDto.StorageId,
                        CustomerDetailId =  DebitDto.CustomerDetailId,
                        Name = CycleName.ToString(),
                        AccountingDate = DebitDto.AccountingDate,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Bills.Add(bill_Partner);
                    await _context.SaveChangesAsync();  // phải có
                }
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn,tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "DKKH"+DebitDto.AccountingDate.ToString("yyMM"),4);

                var debit = new Debit
                {
                    BillId = bill_Partner.Id,
                    CustomerDetailId = DebitDto.CustomerDetailId,
                    StorageId = DebitDto.StorageId,
                    Type = DebitDto.Type,
                    DispatchCode = DispatchCode,
                    Name = DebitDto.Name,
                    AccountingDate = DebitDto.AccountingDate,
                    PurchasePrice = DebitDto.Price,
                    Price = DebitDto.Price,
                    Status = ContractFileRepository.statusDebit,
                    Data = DebitDto.Data,
                    Note = DebitDto.Note,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    StorageId = DebitDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitDto.CustomerDetailId,
                    Status = ContractFileRepository.statusDebit,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
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
        [HttpPost]
        [Route("create/daukyncc")]
        public async Task<IActionResult> CreateDauKyNCC([FromBody] DebitDto DebitDto)
        {

            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.SupplierDetailId == DebitDto.SupplierDetailId);
                if (bill_Partner == null)
                {
                    var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD"+DebitDto.AccountingDate.ToString("yyMM"),4);
                    bill_Partner = new Bill
                    {
                        BillCode = BillCodePartner,
                        StorageId = DebitDto.StorageId,
                        SupplierDetailId =  DebitDto.SupplierDetailId,
                        Name = CycleName.ToString(),
                        AccountingDate = DebitDto.AccountingDate,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Bills.Add(bill_Partner);
                    await _context.SaveChangesAsync();  // phải có
                }
                var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn,tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "DKNCC"+DebitDto.AccountingDate.ToString("yyMM"),4);

                var debit = new Debit
                {
                    BillId = bill_Partner.Id,
                    SupplierDetailId = DebitDto.SupplierDetailId,
                    StorageId = DebitDto.StorageId,
                    Type = DebitDto.Type,
                    DispatchCode = DispatchCode,
                    Name = DebitDto.Name,
                    AccountingDate = DebitDto.AccountingDate,
                    PurchasePrice = DebitDto.Price,
                    Price = DebitDto.Price,
                    Status = ContractFileRepository.statusDebit,
                    Data = DebitDto.Data,
                    Note = DebitDto.Note,
                    CreatedBy = userId,
                    CreatedAt = now,
                    UpdatedAt = now,
                    UpdatedBy = userId
                };
                _context.Debits.Add(debit);
                await _context.SaveChangesAsync();  // phải có
                var entity = new ConfirmFile
                {
                    StorageId = DebitDto.StorageId,
                    DebitId = debit.Id,
                    PartnerDetailId = DebitDto.SupplierDetailId,
                    Status = ContractFileRepository.statusDebit,
                    StatusConfirm = 0,
                    CreatedBy = userId,
                    CreatedAt = now,
                };
                _context.ConfirmFiles.Add(entity);
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
        [HttpPost]
        [Route("update/daukyvamuaban")]
        public async Task<IActionResult> UpdateDauKy([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.CustomerDetailId == DebitDto.CustomerDetailId);
                if (bill_Partner == null)
                {
                    var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD" + DebitDto.AccountingDate.ToString("yyMM"), 4);
                    bill_Partner = new Bill
                    {
                        BillCode = BillCodePartner,
                        StorageId = DebitDto.StorageId,
                        CustomerDetailId = DebitDto.CustomerDetailId,
                        Name = CycleName.ToString(),
                        AccountingDate = DebitDto.AccountingDate,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Bills.Add(bill_Partner);
                    await _context.SaveChangesAsync();  // phải có
                }
                entity.BillId = bill_Partner.Id;
                entity.Name = DebitDto.Name;
                entity.Type = DebitDto.Type;
                entity.AccountingDate = DebitDto.AccountingDate;
                entity.PurchasePrice = DebitDto.Price;
                entity.Price = DebitDto.Price;
                entity.Data = DebitDto.Data;
                entity.UpdatedAt = now;
                entity.UpdatedBy = userId;
                _context.Debits.Update(entity);
                await _context.SaveChangesAsync();  // phải có
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost]
        [Route("update/muahang")]
        public async Task<IActionResult> UpdateMuaHang([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.SupplierDetailId == DebitDto.SupplierDetailId);
                if (bill_Partner == null)
                {
                    var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD" + DebitDto.AccountingDate.ToString("yyMM"), 4);
                    bill_Partner = new Bill
                    {
                        BillCode = BillCodePartner,
                        StorageId = DebitDto.StorageId,
                        SupplierDetailId = DebitDto.SupplierDetailId,
                        Name = CycleName.ToString(),
                        AccountingDate = DebitDto.AccountingDate,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Bills.Add(bill_Partner);
                    await _context.SaveChangesAsync();  // phải có
                }
                entity.BillId = bill_Partner.Id;
                entity.Name = DebitDto.Note;
                entity.AccountingDate = DebitDto.AccountingDate;
                entity.PurchasePrice = DebitDto.Price;
                entity.Price = DebitDto.Price;
                entity.Vat = DebitDto.Vat;
                entity.Data = DebitDto.Data;
                entity.UpdatedAt = now;
                entity.UpdatedBy = userId;
                _context.Debits.Update(entity);
                await _context.SaveChangesAsync();  // phải có
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost]
        [Route("update/banhang")]
        public async Task<IActionResult> UpdateBanHang([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.CustomerDetailId == DebitDto.CustomerDetailId);
                if (bill_Partner == null)
                {
                    var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD" + DebitDto.AccountingDate.ToString("yyMM"), 4);
                    bill_Partner = new Bill
                    {
                        BillCode = BillCodePartner,
                        StorageId = DebitDto.StorageId,
                        CustomerDetailId = DebitDto.CustomerDetailId,
                        Name = CycleName.ToString(),
                        AccountingDate = DebitDto.AccountingDate,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Bills.Add(bill_Partner);
                    await _context.SaveChangesAsync();  // phải có
                }
                entity.BillId = bill_Partner.Id;
                entity.Name = DebitDto.Note;
                entity.AccountingDate = DebitDto.AccountingDate;
                entity.PurchasePrice = DebitDto.Price;
                entity.Price = DebitDto.Price;
                entity.Vat = DebitDto.Vat;
                entity.Data = DebitDto.Data;
                entity.UpdatedAt = now;
                entity.UpdatedBy = userId;
                _context.Debits.Update(entity);
                await _context.SaveChangesAsync();  // phải có
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost]
        [Route("service/create")]
        public async Task<IActionResult> ServiceCreate([FromBody] DebitDto DebitDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            var BillCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD" + DebitDto.AccountingDate.ToString("yyMM"), 4);
            try
            {
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var now = DateTime.Now;
                if (DebitDto.CustomerDetailId > 0)
                {
                    var bill_Partner = await _context.Bills.AsNoTracking().FirstOrDefaultAsync(x => x.CycleName == CycleName && x.CustomerDetailId == DebitDto.CustomerDetailId);
                    if (bill_Partner == null)
                    {
                        bill_Partner = new Bill
                        {
                            BillCode = BillCode,
                            StorageId = DebitDto.StorageId,
                            Name = CycleName.ToString(),
                            AccountingDate = DebitDto.AccountingDate,
                            CustomerDetailId = DebitDto.CustomerDetailId,
                            CycleName = CycleName,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Bills.Add(bill_Partner);
                        await _context.SaveChangesAsync();

                    }
                    foreach (var item in DebitDto.productChiho)
                    {
                        var debit = new Debit
                        {
                            BillId = bill_Partner.Id,
                            CustomerDetailId = DebitDto.CustomerDetailId,
                            ServiceId = item.ServiceId,
                            Bill = item.Bill,
                            LinkBill = item.LinkBill,
                            CodeBill = item.CodeBill,
                            EmployeeStaffId = DebitDto.EmployeeStaffId,
                            DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "CH" + DebitDto.AccountingDate.ToString("yyMM"), 4),
                            FileInfoId = DebitDto.FileInfoId,
                            StorageId = DebitDto.StorageId,
                            Type = DebitRepositories.PhiChiHo,
                            Name = item.Name,
                            AccountingDate = DebitDto.AccountingDate,
                            PurchasePrice = item.PurchasePrice,
                            Price = item.PurchasePrice,
                            Data = DebitDto.Data,
                            Note = DebitDto.Note,
                            ServiceDetail =  JsonSerializer.Serialize(new[] { item }),
                            PurchaseStatus = DebitDto.PurchaseStatus,
                            PurchaseVat = DebitDto.PurchaseVat,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();
                        var entity = new ConfirmFile
                        {
                            FileInfoId = DebitDto.FileInfoId,
                            DebitId = debit.Id,
                            StorageId = DebitDto.StorageId,
                            PartnerDetailId = DebitDto.CustomerDetailId,
                            Status = ContractFileRepository.statusDichVu,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                    if (DebitDto.productHaiquan.Count > 0)
                    {
                        var debit = new Debit
                        {
                            BillId = bill_Partner.Id,
                            CustomerDetailId = DebitDto.CustomerDetailId,
                            EmployeeStaffId = DebitDto.EmployeeStaffId,
                            FileInfoId = DebitDto.FileInfoId,
                            DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "HQ" + DebitDto.AccountingDate.ToString("yyMM"), 4),
                            StorageId = DebitDto.StorageId,
                            Type = DebitRepositories.PhiHaiQuan,
                            Name = "Chi phí hải quan",
                            AccountingDate = DebitDto.AccountingDate,
                            PurchasePrice = DebitDto.productHaiquan.Sum(x => x.PurchasePrice),
                            Data = DebitDto.Data,
                            Note = DebitDto.Note,
                            ServiceDetail = JsonSerializer.Serialize(DebitDto.productHaiquan),
                            PurchaseStatus = DebitDto.PurchaseStatus,
                            PurchaseVat = DebitDto.PurchaseVat,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();
                        var entity = new ConfirmFile
                        {
                            FileInfoId = DebitDto.FileInfoId,
                            StorageId = DebitDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = DebitDto.CustomerDetailId,
                            Status = ContractFileRepository.statusDichVu,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                }
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }

        }
        [HttpPost]
        [Route("nangha/create")]
        public async Task<IActionResult> NangHaCreate([FromBody] DebitDto DebitDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            var BillCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", DebitDto.StorageId, "HD"+DebitDto.AccountingDate.ToString("yyMM"), 4);
            try
            {
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                var now = DateTime.Now;
                if (DebitDto.CustomerDetailId > 0)
                {
                    var bill_Partner = await _context.Bills.AsNoTracking().FirstOrDefaultAsync(x => x.CycleName == CycleName && x.CustomerDetailId == DebitDto.CustomerDetailId);
                    if (bill_Partner == null)
                    {
                        bill_Partner = new Bill
                        {
                            BillCode = BillCode,
                            StorageId = DebitDto.StorageId,
                            Name = CycleName.ToString(),
                            AccountingDate = DebitDto.AccountingDate,
                            CustomerDetailId = DebitDto.CustomerDetailId,
                            CycleName = CycleName,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Bills.Add(bill_Partner);
                         await _context.SaveChangesAsync();
                     
                    }
                    foreach (var item in DebitDto.productNangha) 
                    {
                        var debit = new Debit
                        {
                            BillId = bill_Partner.Id,
                            CustomerDetailId = DebitDto.CustomerDetailId,
                            ServiceId = item.ServiceId,
                            Bill = item.Bill,
                            LinkBill = item.LinkBill,
                            CodeBill = item.CodeBill,
                            EmployeeStaffId = DebitDto.EmployeeStaffId,
                            DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", DebitDto.StorageId, "NH" + DebitDto.AccountingDate.ToString("yyMM"), 4),
                            FileInfoId = DebitDto.FileInfoId,
                            StorageId = DebitDto.StorageId,
                            Type = DebitRepositories.PhiNangHa,
                            Name = item.Name,
                            AccountingDate = DebitDto.AccountingDate,
                            PurchasePrice = item.PurchasePrice,
                            Price = item.PurchasePrice,
                            Data = DebitDto.Data,
                            Note = DebitDto.Note,
                            PurchaseStatus = DebitDto.PurchaseStatus,
                            PurchaseVat = DebitDto.PurchaseVat,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        debit.SupplierDetailId = (item.SupplierDetailId > 0) ? item.SupplierDetailId : null;
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();
                        var entity = new ConfirmFile
                        {
                            FileInfoId = DebitDto.FileInfoId,
                            DebitId = debit.Id,
                            StorageId = DebitDto.StorageId,
                            PartnerDetailId = DebitDto.CustomerDetailId,
                            Status = ContractFileRepository.statusDichVu,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                  
                }
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Thêm thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }

        }
        [HttpPost("confirmChiPhiHaiQuan")]
        public async Task<IActionResult> ConfirmChiPhiHaiQuan([FromBody] DebitDto DebitDto)
        {
             using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == DebitDto.Id);
                if (debit == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu file giá", null);
                var confirm_file = await _context.ConfirmFiles
                .Where(x =>
                    x.FileInfoId == DebitDto.FileInfoId &&
                    x.PartnerDetailId == DebitDto.CustomerDetailId &&
                    x.DebitId == debit.Id
                ).FirstOrDefaultAsync();
                if (confirm_file == null) return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu xác nhận", null);
                debit.PurchasePrice = DebitDto.productHaiquan.Sum(x => x.PurchasePrice);
                debit.ServiceDetail = JsonSerializer.Serialize(DebitDto.productHaiquan);
                debit.UpdatedBy = userId;
                debit.UpdatedAt = now;
                if (confirm_file.Status == 0 || confirm_file.Status == 1)
                {
                    confirm_file.Status = 1;
                    confirm_file.StatusConfirm = 1;
                    confirm_file.UpdatedBy = userId;
                    confirm_file.UpdatedAt = now;
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("importDauKy")]
        public async Task<IActionResult> ImportDauKy([FromBody] ImportDauKyDto ImportDauKyDto)
        {
            // Kiểm tra chi tiết phiếu thu
            if (string.IsNullOrEmpty(ImportDauKyDto.Data))
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            List<JsonElement> list = null;
            try
            {
                list = JsonSerializer.Deserialize<List<JsonElement>>(ImportDauKyDto.Data);
            }
            catch
            {
                return ApiResponseResult<object>(false, "Dữ liệu chi tiết không hợp lệ", null);
            }

            if (list == null || list.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                foreach (var item in list)
                {
                    string ten_kh = item.GetProperty("ten_kh").GetString();
                    int tien_dv = item.GetProperty("tien_dv").GetInt32();
                    int tien_ch = item.GetProperty("tien_ch").GetInt32();
                    string noi_dung = item.GetProperty("noi_dung").GetString();
                    DateTime ngay = Convert.ToDateTime(item.GetProperty("ngay").GetString());
                    var _kh = await _context.Partners.Where(x => x.Abbreviation.Contains(ten_kh)).FirstOrDefaultAsync();
                    if (_kh == null) continue;
                    var _kh_detail = await _context.PartnerDetails.Where(x => x.PartnerId == _kh.Id && x.Status == 1).FirstOrDefaultAsync();
                    if (_kh_detail == null) continue;
                    int CycleName = int.Parse(ngay.ToString("MMyyyy"));
                    var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.CustomerDetailId == _kh_detail.Id);
                    if (bill_Partner == null)
                    {
                        var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", ImportDauKyDto.StorageId, "HD" + ngay.ToString("yyMM"), 4);
                        bill_Partner = new Bill
                        {
                            BillCode = BillCodePartner,
                            StorageId = ImportDauKyDto.StorageId,
                            CustomerDetailId = _kh_detail.Id,
                            Name = CycleName.ToString(),
                            AccountingDate = ngay,
                            CycleName = CycleName,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Bills.Add(bill_Partner);
                        await _context.SaveChangesAsync();  // phải có
                    }
                    var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", ImportDauKyDto.StorageId, "DKKH" + ngay.ToString("yyMM"), 4);

                    if (tien_dv > 0)
                    {
                        var debit = new Debit
                        {
                            BillId = bill_Partner.Id,
                            CustomerDetailId = _kh_detail.Id,
                            StorageId = ImportDauKyDto.StorageId,
                            Type = 5,
                            DispatchCode = DispatchCode,
                            Name = noi_dung,
                            AccountingDate = ngay,
                            Price = tien_dv,
                            Status = ContractFileRepository.statusDebit,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();  // phải có
                        var entity = new ConfirmFile
                        {
                            StorageId = ImportDauKyDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = _kh_detail.Id,
                            Status = ContractFileRepository.statusDebit,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                    if (tien_ch > 0)
                    {
                        var debit = new Debit
                        {
                            BillId = bill_Partner.Id,
                            CustomerDetailId = _kh_detail.Id,
                            StorageId = ImportDauKyDto.StorageId,
                            Type = 6,
                            DispatchCode = DispatchCode,
                            Name = noi_dung,
                            AccountingDate = ngay,
                            Price = tien_ch,
                            Status = ContractFileRepository.statusDebit,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();  // phải có
                        var entity = new ConfirmFile
                        {
                            StorageId = ImportDauKyDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = _kh_detail.Id,
                            Status = ContractFileRepository.statusDebit,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }

                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
         [HttpPost("importDauKyNCC")]
        public async Task<IActionResult> ImportDauKyNCC([FromBody] ImportDauKyDto ImportDauKyDto)
        {
            // Kiểm tra chi tiết phiếu thu
            if (string.IsNullOrEmpty(ImportDauKyDto.Data))
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            List<JsonElement> list = null;
            try
            {
                list = JsonSerializer.Deserialize<List<JsonElement>>(ImportDauKyDto.Data);
            }
            catch
            {
                return ApiResponseResult<object>(false, "Dữ liệu chi tiết không hợp lệ", null);
            }

            if (list == null || list.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                foreach (var item in list)
                {
                    string ten_ncc = item.GetProperty("ten_ncc").GetString();
                    int tien_dv = item.GetProperty("tien_dv").GetInt32();
                    int tien_ch = item.GetProperty("tien_ch").GetInt32();
                    string noi_dung = item.GetProperty("noi_dung").GetString();
                    DateTime ngay = Convert.ToDateTime(item.GetProperty("ngay").GetString());
                    var _ncc = await _context.Partners.Where(x => x.Abbreviation.Contains(ten_ncc)).FirstOrDefaultAsync();
                    if (_ncc == null) continue;
                    var _ncc_detail = await _context.PartnerDetails.Where(x => x.PartnerId == _ncc.Id && x.Status == 2).FirstOrDefaultAsync();
                    if (_ncc_detail == null) continue;
                    int CycleName = int.Parse(ngay.ToString("MMyyyy"));
                    var bill_Partner = await _context.Bills.FirstOrDefaultAsync(x => x.CycleName == CycleName && x.SupplierDetailId == _ncc_detail.Id);
                    if (bill_Partner == null)
                    {
                        var BillCodePartner = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", ImportDauKyDto.StorageId, "HD" + ngay.ToString("yyMM"), 4);
                        bill_Partner = new Bill
                        {
                            BillCode = BillCodePartner,
                            StorageId = ImportDauKyDto.StorageId,
                            SupplierDetailId = _ncc_detail.Id,
                            Name = CycleName.ToString(),
                            AccountingDate = ngay,
                            CycleName = CycleName,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Bills.Add(bill_Partner);
                        await _context.SaveChangesAsync();  // phải có
                    }
                    var DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", ImportDauKyDto.StorageId, "DKNCC" + ngay.ToString("yyMM"), 4);

                    if (tien_dv > 0)
                    {
                        var debit = new Debit
                        {
                            BillId = bill_Partner.Id,
                            SupplierDetailId = _ncc_detail.Id,
                            StorageId = ImportDauKyDto.StorageId,
                            Type = 10,
                            DispatchCode = DispatchCode,
                            Name = noi_dung,
                            AccountingDate = ngay,
                            Price = tien_dv,
                            Status = ContractFileRepository.statusDebit,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();  // phải có
                        var entity = new ConfirmFile
                        {
                            StorageId = ImportDauKyDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = _ncc_detail.Id,
                            Status = ContractFileRepository.statusDebit,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }
                    if (tien_ch > 0)
                    {
                        var debit = new Debit
                        {
                            BillId = bill_Partner.Id,
                            SupplierDetailId = _ncc_detail.Id,
                            StorageId = ImportDauKyDto.StorageId,
                            Type = 11,
                            DispatchCode = DispatchCode,
                            Name = noi_dung,
                            AccountingDate = ngay,
                            Price = tien_ch,
                            Status = ContractFileRepository.statusDebit,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();  // phải có
                        var entity = new ConfirmFile
                        {
                            StorageId = ImportDauKyDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = _ncc_detail.Id,
                            Status = ContractFileRepository.statusDebit,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                        await _context.SaveChangesAsync();
                    }

                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("confirmDebitNoFileDispatchKH")]
        public async Task<IActionResult> ConfirmDebitNoFileDispatchKH([FromBody] ConfirmDebitNoFileDto ConfirmDebitNoFileDto)
        {
            // Kiểm tra chi tiết phiếu thu
            if (string.IsNullOrEmpty(ConfirmDebitNoFileDto.Data))
            {
                return ApiResponseResult<object>(false, "Không có chi tiết", null);
            }
            List<JsonElement> list = null;
            try
            {
                list = JsonSerializer.Deserialize<List<JsonElement>>(ConfirmDebitNoFileDto.Data);
            }
            catch
            {
                return ApiResponseResult<object>(false, "Dữ liệu chi tiết không hợp lệ", null);
            }

            if (list == null || list.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không có chi tiết cong no", null);
            }
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                foreach (var item in list)
                {
                    int debit_id = item.GetProperty("id").GetInt32();
                    int vat = item.GetProperty("vat").GetInt32();
                    var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == debit_id);
                    if (debit == null) continue;
                    var confirm_file = await _context.ConfirmFiles.FirstOrDefaultAsync(x => x.DebitId == debit.Id); // duyệt file giá

                    if (confirm_file.Status < 2)
                    {
                        debit.AccountingDate = ConfirmDebitNoFileDto.AccountingDate;
                        debit.Vat = vat;
                        debit.Status = ContractFileRepository.statusDebit;
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                        confirm_file.Status = ContractFileRepository.statusDebit;
                        confirm_file.StatusConfirm = 0;
                        confirm_file.UpdatedBy = userId;
                        confirm_file.UpdatedAt = now;
                    }
                    // cập nhat hoa don debit
                    if (ConfirmDebitNoFileDto.Type == 1)
                    {
                        debit.CusBillDate = ConfirmDebitNoFileDto.AccountingDate;
                        debit.CusBill = ConfirmDebitNoFileDto.Bill;
                        debit.UpdatedBy = userId;
                        debit.UpdatedAt = now;
                    }

                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("confirmFileGia")]
        public async Task<IActionResult> ConfirmFileGia([FromBody] ConfirmFileDto ConfirmFileDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                foreach (var item in ConfirmFileDto.DebitDtos)
                {
                    var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == item.Id);
                    if (debit == null) continue;
                    if (item.Type == 0 && item.Price == 0)
                    {
                        await tran.RollbackAsync();
                        return ApiResponseResult<object>(false, "Chưa nhập giá bán cho phí hải quan.", null);
                    }
                    var confirm_file = await _context.ConfirmFiles.FirstOrDefaultAsync(x => x.FileInfoId == item.FileInfoId && x.PartnerDetailId == item.CustomerDetailId && x.DebitId == debit.Id); // duyệt file giá
                    if (confirm_file == null)
                    {
                        await tran.RollbackAsync();
                        return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu xác nhận chi phí. Hãy duyệt chi phí hải quan" +debit.Id, null);
                    }
                    debit.Status = ContractFileRepository.statusDebit; 
                    debit.UpdatedBy = userId;
                    debit.UpdatedAt = now;
                    if (confirm_file.Status == 1 || confirm_file.Status == 2)
                    {
                        confirm_file.Status = ContractFileRepository.statusDebit;
                        confirm_file.StatusConfirm = ConfirmFileDto.StatusConfirm;
                        confirm_file.UpdatedBy = userId;
                        confirm_file.UpdatedAt = now;
                    }

                }
               
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("updateFileGia")]
        public async Task<IActionResult> UpdateFileGia([FromBody] ConfirmFileDto ConfirmFileDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;

                foreach (var item in ConfirmFileDto.DebitDtos)
                {
                    var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == item.Id);
                    if (debit == null) continue;
                    // Chỉ update Price nếu Type == 0
                    if (item.Type == 0 && item.Price == 0)
                    {
                        await tran.RollbackAsync();
                        return ApiResponseResult<object>(false, "Chưa nhập giá bán cho phí hải quan.", null);
                    }
                    if (item.Type == 0)
                    {
                        debit.Price = item.Price;
                    }
                    debit.AccountingDate = ConfirmFileDto.AccountingDate;
                    debit.Vat = item.Vat;
                    debit.Status =  ContractFileRepository.statusFileGia; 
                    debit.Bill = item.Bill;
                    debit.UpdatedBy = userId;
                    debit.UpdatedAt = now;
                    var confirm_file = await _context.ConfirmFiles.FirstOrDefaultAsync(x => x.FileInfoId == ConfirmFileDto.FileInfoId && x.PartnerDetailId == ConfirmFileDto.PartnerDetailId && x.DebitId == debit.Id); // tạo phần duyệt file giá
                    if (confirm_file == null)
                    {
                        var entity = new ConfirmFile
                        {
                            FileInfoId = ConfirmFileDto.FileInfoId,
                            StorageId = ConfirmFileDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = ConfirmFileDto.PartnerDetailId,
                            Status = ContractFileRepository.statusFileGia,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                    }
                    else
                    {
                        if (confirm_file.Status == 0 || confirm_file.Status == 1)
                        {
                            confirm_file.FileInfoId = ConfirmFileDto.FileInfoId;
                            confirm_file.StorageId = ConfirmFileDto.StorageId;
                            confirm_file.PartnerDetailId = ConfirmFileDto.PartnerDetailId;
                            confirm_file.Status = ContractFileRepository.statusFileGia;
                            confirm_file.StatusConfirm = 0;
                            confirm_file.UpdatedAt = null;
                            confirm_file.UpdatedBy = null;
                            _context.ConfirmFiles.Update(confirm_file);
                        }
                      
                    }
                    if (item.Type == 2) // duyệt luôn phần chi hộ
                    {
                         confirm_file.StatusConfirm = 1;
                        _context.ConfirmFiles.Update(confirm_file);
                    }

                }
                int CycleName = int.Parse(ConfirmFileDto.AccountingDate.ToString("MMyyyy"));

                var bill_Partner = await _context.Bills.AsNoTracking().FirstOrDefaultAsync(x => x.CycleName == CycleName && x.CustomerDetailId == ConfirmFileDto.PartnerDetailId);
                if (bill_Partner == null)
                {
                    var BillCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "bills", "bill_code", ConfirmFileDto.StorageId, "HD"+ConfirmFileDto.AccountingDate.ToString("yyMM"), 4);
                    bill_Partner = new Bill
                    {
                        BillCode = BillCode,
                        StorageId = ConfirmFileDto.StorageId,
                        Name = CycleName.ToString(),
                        AccountingDate = ConfirmFileDto.AccountingDate,
                        CustomerDetailId = ConfirmFileDto.PartnerDetailId,
                        CycleName = CycleName,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };
                    _context.Bills.Add(bill_Partner);
                    await _context.SaveChangesAsync();
                    
                }
                foreach (var item in ConfirmFileDto.Chiphikhac)
                {
                     var debit = new Debit
                        {
                            BillId = bill_Partner.Id,
                            CustomerDetailId = ConfirmFileDto.PartnerDetailId,
                            FileInfoId = ConfirmFileDto.FileInfoId,
                            DispatchCode = await SqlServerHelpers.GenerateSoChungTuEfAsync(conn, tran.GetDbTransaction(), "debits", "dispatch_code", ConfirmFileDto.StorageId, "PK" + ConfirmFileDto.AccountingDate.ToString("yyMM"), 4),
                            StorageId = ConfirmFileDto.StorageId,
                            Type = DebitRepositories.PhiKhac,
                            Name = item.Name,
                            AccountingDate = ConfirmFileDto.AccountingDate,
                            PurchasePrice = item.PurchasePrice,
                            Price = item.Price,
                            Vat = item.Vat,
                            Status = ContractFileRepository.statusFileGia,
                            Data =  JsonSerializer.Serialize(new{fileNumber=ConfirmFileDto.FileNumber}),
                            Note = item.Note,
                            ServiceDetail = JsonSerializer.Serialize(new []{item}),
                            PurchaseStatus = 0,
                            PurchaseVat = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };
                        _context.Debits.Add(debit);
                        await _context.SaveChangesAsync();
                        var entity = new ConfirmFile
                        {
                            FileInfoId = ConfirmFileDto.FileInfoId,
                            StorageId = ConfirmFileDto.StorageId,
                            DebitId = debit.Id,
                            PartnerDetailId = ConfirmFileDto.PartnerDetailId,
                            Status = ContractFileRepository.statusFileGia,
                            StatusConfirm = 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                        };
                        _context.ConfirmFiles.Add(entity);
                }
               
                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult<object>(true, "Cập nhật thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.InnerException?.Message, null);
            }
        }
        [HttpPost("updateDebitToStatusDichVu")]
        public async Task<IActionResult> UpdateDebitToStatusDichVu([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            var confirm_file = await _context.ConfirmFiles.FirstOrDefaultAsync(x => x.DebitId == entity.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu debit confirm", null);
            }
            var now = DateTime.Now;
            entity.Vat = 0;
            entity.Status = ContractFileRepository.statusDichVu;
            entity.UpdatedBy = userId;
            entity.UpdatedAt = now;
            confirm_file.Status = ContractFileRepository.statusDichVu;
            confirm_file.StatusConfirm = 0;
            confirm_file.UpdatedBy = userId;
            confirm_file.UpdatedAt = now;
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Debits.Find(DebitDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoDebit.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpPost("delete/multiDebit")]
        public async Task<IActionResult> DeleteMultiDebit([FromBody] DebitDeleteMultiDto DebitDeleteMultiDto)
        {
            if (DebitDeleteMultiDto.Ids == null || !DebitDeleteMultiDto.Ids.Any())
            {
                return ApiResponseResult<object>(false, "Danh sách Id không tồn tại", null);
            }

            // Lấy danh sách entity theo Ids
            var entities = await _context.Debits
                .Where(d => DebitDeleteMultiDto.Ids.Contains(d.Id))
                .ToListAsync();

            if (entities.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu tương ứng với các Id đã gửi", null);
            }
            // Cập nhật thông tin xóa mềm
            var now = DateTime.Now;
            foreach (var item in entities)
            {
                item.DeletedBy = userId;
                item.DeletedAt = now;
            }
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpPost("delete/multi")]
        public async Task<IActionResult> DeleteMulti([FromBody] DebitDeleteMultiDto DebitDeleteMultiDto)
        {
            if (DebitDeleteMultiDto.Ids == null || !DebitDeleteMultiDto.Ids.Any())
            {
                return ApiResponseResult<object>(false, "Danh sách Id không tồn tại", null);
            }

            // Lấy danh sách entity theo Ids
            var entities = await _context.Debits
                .Where(d => DebitDeleteMultiDto.Ids.Contains(d.Id))
                .ToListAsync();
            var entitie_confirms = await _context.ConfirmFiles
                .Where(d => DebitDeleteMultiDto.Ids.Contains(d.DebitId))
                .ToListAsync();

            if (entities.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu tương ứng với các Id đã gửi", null);
            }
            // Cập nhật thông tin xóa mềm
            var now = DateTime.Now;
            foreach (var item in entities)
            {
                if (item.Type == 0)
                {
                    item.Price = 0;
                }
                if (item.Type == 4)
                {
                    item.Price = 0;
                    item.DeletedAt = now;
                    item.DeletedBy = userId;
                }
                item.Status = 0;
                item.Vat = 0;
                item.UpdatedAt = now;
                item.UpdatedBy = userId;
            }
            foreach (var item in entitie_confirms)
            {
                item.Status = 0;
                item.StatusConfirm = 0;
                item.UpdatedAt = now;
                item.UpdatedBy = userId;
            } 
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpPost("showWithIds")]
        public async Task<IActionResult> ShowWithIds([FromBody] DebitDeleteMultiDto DebitDeleteMultiDto)
        {
            if (DebitDeleteMultiDto.Ids == null || !DebitDeleteMultiDto.Ids.Any())
            {
                return ApiResponseResult<object>(false, "Danh sách Id không tồn tại", null);
            }

            // Lấy danh sách entity theo Ids
            var entities = await _context.Debits
                .Where(d => DebitDeleteMultiDto.Ids.Contains(d.Id))
                .ToListAsync();

            if (entities.Count == 0)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu tương ứng với các Id đã gửi", null);
            }
            return ApiResponseResult<object>(true, "lấy dữ liệu thành công", entities);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _repoDebit.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        [HttpGet("show/byFileId")]
        public async Task<IActionResult> ShowByFileId([FromQuery] int FileId)
        {
            if (FileId <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _repoDebit.ShowByFileIdAsync(FileId);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
