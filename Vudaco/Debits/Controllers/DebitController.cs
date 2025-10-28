using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Bills.Models;
using Vudaco.Controllers;
using Vudaco.Debits.Dtos;
using Vudaco.Debits.Models;
using Vudaco.Debits.Repositories;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

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
            try
            {
                int CycleName = int.Parse(DebitDto.AccountingDate.ToString("MMyyyy"));
                if (DebitDto.PartnerDetailId > 0)
                {
                    var bill_Partner = await _context.Bills.AsNoTracking().FirstOrDefaultAsync(x => x.CycleName == CycleName && x.PartnerDetailId == DebitDto.PartnerDetailId);
                    if (bill_Partner == null)
                    {
                        bill_Partner = new Bill
                        {
                            BillCode = SqlServerHelpers.GenerateSoChungTu(_configuration.GetConnectionString("DefaultConnection"), "bills", "bill_code", "HDK", 8),
                            StorageId = DebitDto.StorageId,
                            Name = CycleName.ToString(),
                            AccountingDate = DebitDto.AccountingDate,
                            PartnerDetailId = DebitDto.PartnerDetailId,
                            CycleName = CycleName,
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Bills.Add(bill_Partner);
                        await _context.SaveChangesAsync();
                    }
                    var debit = new Debit
                    {
                        BillId = bill_Partner.Id,
                        VehicleId = DebitDto.VehicleId,
                        PartnerDetailId = DebitDto.PartnerDetailId,
                        EmployeeDriverId = DebitDto.EmployeeDriverId,
                        EmployeeStaffId = DebitDto.EmployeeStaffId, 
                        FileInfoId = DebitDto.FileInfoId,
                        StorageId = DebitDto.StorageId,
                        Type = DebitDto.Type,
                        DispatchCode = DebitDto.DispatchCode,
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
                        Status = DebitDto.Status,
                        Data = DebitDto.Data,
                        Note = DebitDto.Note,
                        CustomerVehicleType = DebitDto.CustomerVehicleType,
                        SupplierVehicleType = DebitDto.SupplierVehicleType,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    debit = await _repoDebit.CreateAsync(debit);
                }
                if (DebitDto.SupplierPartnerDetailId > 0)
                {
                    var bill_Supplier = await _context.Bills.AsNoTracking().FirstOrDefaultAsync(x => x.CycleName == CycleName && x.PartnerDetailId == DebitDto.SupplierPartnerDetailId);
                    if (bill_Supplier == null)
                    {
                        bill_Supplier = new Bill
                        {
                            BillCode = SqlServerHelpers.GenerateSoChungTu(_configuration.GetConnectionString("DefaultConnection"), "bills", "bill_code", "HDN", 8),
                            StorageId = DebitDto.StorageId,
                            Name = CycleName.ToString(),
                            AccountingDate = DebitDto.AccountingDate,
                            PartnerDetailId = DebitDto.SupplierPartnerDetailId,
                            CycleName = CycleName,
                            CreatedBy = userId,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.Bills.Add(bill_Supplier);
                        await _context.SaveChangesAsync();
                    }
                    var debit = new Debit
                    {
                        BillId = bill_Supplier.Id,
                        VehicleId = DebitDto.VehicleId,
                        PartnerDetailId = DebitDto.SupplierPartnerDetailId,
                        EmployeeDriverId = DebitDto.EmployeeDriverId,
                        EmployeeStaffId = DebitDto.EmployeeStaffId,     
                        FileInfoId = DebitDto.FileInfoId,
                        StorageId = DebitDto.StorageId,
                        Type = DebitDto.Type,
                        DispatchCode = DebitDto.DispatchCode,
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
                        Status = DebitDto.Status,
                        Data = DebitDto.Data,
                        Note = DebitDto.Note,
                        CustomerVehicleType = DebitDto.CustomerVehicleType,
                        SupplierVehicleType = DebitDto.SupplierVehicleType,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    debit = await _repoDebit.CreateAsync(debit);
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
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] DebitDto DebitDto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var debit = await _context.Debits.FirstOrDefaultAsync(x => x.Id == DebitDto.Id);
                if (debit == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy debit", null);

                // cập nhật các field theo DebitDto
                debit.VehicleId = DebitDto.VehicleId;
                debit.PartnerDetailId = DebitDto.PartnerDetailId;
                debit.EmployeeDriverId = DebitDto.EmployeeDriverId;
                debit.EmployeeStaffId = DebitDto.EmployeeStaffId;   
                debit.FileInfoId = DebitDto.FileInfoId;
                debit.StorageId = DebitDto.StorageId;
                debit.Type = DebitDto.Type;
                debit.DispatchCode = DebitDto.DispatchCode;
                debit.Name = DebitDto.Name;
                debit.AccountingDate = DebitDto.AccountingDate;
                debit.PurchasePrice = DebitDto.PurchasePrice;
                debit.Price = DebitDto.Price;
                debit.Vat = DebitDto.Vat;
                debit.DriverFee = DebitDto.DriverFee;
                debit.MealFee = DebitDto.MealFee;
                debit.TicketFee = DebitDto.TicketFee;
                debit.OvernightFee = DebitDto.OvernightFee;
                debit.PenaltyFee = DebitDto.PenaltyFee;
                debit.GoodsFee = DebitDto.GoodsFee;
                debit.Status = DebitDto.Status;
                debit.Data = JsonSerializer.Serialize(DebitDto);
                debit.Note = DebitDto.Note;
                debit.CustomerVehicleType = DebitDto.CustomerVehicleType;
                debit.SupplierVehicleType = DebitDto.SupplierVehicleType;
                debit.UpdatedBy = userId;
                debit.UpdatedAt = DateTime.UtcNow;

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
    }
}
