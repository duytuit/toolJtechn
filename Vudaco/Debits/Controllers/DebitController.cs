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

namespace Vudaco.Debits.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebitController : BaseApiController
    {
        private readonly IDebitRepositories _repoDebit;
        private readonly ILogger<DebitController> _logger;
        private readonly VudacoDBContext _context;

        private readonly IConfiguration _configuration;
        public int userId => (int)HttpContext.Items["UserId"];

        public DebitController(ILogger<DebitController> logger, IConfiguration configuration, IDebitRepositories repoDebit, VudacoDBContext context)
        {
            _logger = logger;
            _repoDebit = repoDebit;
            _context = context;
            _configuration = configuration;
        }
        [HttpGet("dispatch")]
        public async Task<IActionResult> GetTaskDispatch(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DebitDto DebitDto = null)
        {
            // test
            var result = await _repoDebit.GetObjectDebitDispatchAsync(DebitDto, page, pageSize, cancellationToken);
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
                    CustomerDetailId = DebitDto.CustomerDetailId,
                    SupplierDetailId = DebitDto.SupplierDetailId,
                    EmployeeDriverId = DebitDto.EmployeeDriverId,
                    EmployeeStaffId = DebitDto.EmployeeStaffId,
                    FileInfoId = DebitDto.FileInfoId,
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
                    var confirm_file = await _context.ConfirmFiles.FirstOrDefaultAsync(x => x.FileInfoId == ConfirmFileDto.FileInfoId && x.PartnerDetailId == ConfirmFileDto.PartnerDetailId && x.Status == ContractFileRepository.statusFileGia && x.DebitId == debit.Id); // tạo phần duyệt file giá
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
