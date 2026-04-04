using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Auth.Models;
using Vudaco.Controllers;
using Vudaco.Partners.Dtos;
using Vudaco.Partners.Models;
using Vudaco.Partners.Repositories;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Partners.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartnerController : BaseApiController
    {
        private readonly IPartnerDetailRepository _repoPartnerDetail;
        private readonly IPartnerRepository _repoPartner;
        private readonly ILogger<PartnerController> _logger;
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public int userId => (int)HttpContext.Items["UserId"];
        public PartnerController(ILogger<PartnerController> logger,RedisService redis,IConfiguration configuration, IPartnerRepository repoPartner, IPartnerDetailRepository repoPartnerDetail, VudacoDBContext context)
        {
            _logger = logger;
            _repoPartnerDetail = repoPartnerDetail;
            _repoPartner = repoPartner;
            _context = context;
            _redis = redis;
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] PartnerDto PartnerDto = null)
        {
            // test
            var result = await _repoPartner.GetObjectTaskAsync(PartnerDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("PartnerDetail")]
        public async Task<IActionResult> GetPartnerDetail(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] PartnerDetailDto PartnerDetailDto = null )
        {
            // test
            var result = await _repoPartner.GetPartnerDetail(PartnerDetailDto, page, pageSize, cancellationToken);
                if (result == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetPartnerKHAndNCCDetail")]
        public async Task<IActionResult> GetPartnerKHAndNCCDetail(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] PartnerDetailDto PartnerDetailDto = null )
        {
            // test
            var result = await _repoPartner.GetPartnerKHAndNCCDetail(PartnerDetailDto, page, pageSize, cancellationToken);
                if (result == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("GetPartnerWithDebitNoBill")]
        public async Task<IActionResult> GetPartnerWithDebitNoBill(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] PartnerDetailDto PartnerDetailDto = null )
        {
            var result = await _repoPartner.GetPartnerWithDebitNoBill(PartnerDetailDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] PartnerDto dto)
        {
            // ====== VALIDATE ======
          
            // Check trùng Name trong cùng storage
            if (await _context.Partners.AnyAsync(p =>
                p.Name == dto.Name &&
                p.StorageId == dto.StorageId))
                return ApiResponseResult<object>(false, "Tên đối tác đã tồn tại trong kho này", null);
            // Check trùng tên viết tắt trong cùng storage
            if (await _context.Partners.AnyAsync(p =>
                p.Abbreviation == dto.Abbreviation &&
                p.StorageId == dto.StorageId))
                return ApiResponseResult<object>(false, "Tên viết tắt đối tác đã tồn tại trong kho này", null);

            // Check trùng TaxCode nếu có nhập
            if (!string.IsNullOrWhiteSpace(dto.TaxCode))
            {
                if (await _context.Partners.AnyAsync(p =>
                    p.TaxCode == dto.TaxCode &&
                    p.StorageId == dto.StorageId))
                    return ApiResponseResult<object>(false, "Mã số thuế đã tồn tại trong kho này", null);
            }

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                //  var user = await _context.Users
                // .AsTracking()
                // .FirstOrDefaultAsync(p => p.Username == dto.Phone);
                // if (user == null)
                // {
                //        // ====== CREATE USER ======
                //     user = new User
                //     {
                //         Username  = dto.Phone,
                //         Password  = !string.IsNullOrWhiteSpace(dto.Password) ? BCrypt.Net.BCrypt.HashPassword(dto.Password) : BCrypt.Net.BCrypt.HashPassword(dto.Phone),
                //         Email     = dto.Email,
                //         LastName  = dto.Abbreviation,
                //         CreatedAt = DateTime.Now,
                //         UpdatedAt = DateTime.Now,
                //         UpdatedBy = userId
                //     };
                //     _context.Users.Add(user);
                //      await _context.SaveChangesAsync(); // cần để lấy user.Id
                // }
                // ====== CREATE PARTNER ======
                var partner = new Partner
                {
                    //Code = dto.Code,
                    Name = dto.Name,
                    StorageId = dto.StorageId,
                    Address = dto.Address,
                    TaxCode = dto.TaxCode,
                    // Phone = dto.Phone,
                    // Email = dto.Email,
                    //BankAccount = dto.BankAccount,
                    //AllowedDebtDays = dto.AllowedDebtDays, // null allowed
                    //MaxDebt = dto.MaxDebt,                 // null allowed
                    //Note = dto.Note,
                    Abbreviation = dto.Abbreviation,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    // UserId = user.Id
                };
                _context.Partners.Add(partner);
                await _context.SaveChangesAsync();   
                var partnerDetail = new PartnerDetail
                {
                    PartnerId = partner.Id,
                    Status = 0,
                    Code = SqlServerHelpers.GenerateSoChungTu( _configuration.GetConnectionString("DefaultConnection"),"partner_details","code",dto.StorageId,"KH", 4),
                    StorageId = dto.StorageId,
                    CustomerCreditLimit = dto.CustomerCreditLimit,
                    CustomerCreditLimitMonth = dto.CustomerCreditLimitMonth,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.PartnerDetails.Add(partnerDetail);
                partnerDetail = new PartnerDetail
                {
                    PartnerId = partner.Id,
                    Status = 0,
                    Code = SqlServerHelpers.GenerateSoChungTu( _configuration.GetConnectionString("DefaultConnection"),"partner_details","code",dto.StorageId,"NCC", 4),
                    StorageId = dto.StorageId,
                    SupplierCreditLimit = dto.SupplierCreditLimit,
                    SupplierCreditLimitMonth = dto.SupplierCreditLimitMonth,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.PartnerDetails.Add(partnerDetail);
                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult(true, "Thêm đối tác thành công", partner);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
       [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] PartnerDto dto)
        {
            if (dto.Id <= 0)
                return ApiResponseResult<object>(false, "Id không hợp lệ", null);

            var partner = await _context.Partners
                .AsTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.Id);
            if (partner == null)
                return ApiResponseResult<object>(false, "Không tìm thấy đối tác", null);

            // var user = await _context.Users
            //     .AsTracking()
            //     .FirstOrDefaultAsync(p => p.Id == partner.UserId);

            // if (user == null)
            //     return ApiResponseResult<object>(false, "Không tìm thấy tài khoản đối tác", null);

            // ========== VALIDATE giống Create ==========

             // Check Name trong Storage
            if (!string.IsNullOrWhiteSpace(dto.Abbreviation) &&
                await _context.Partners.AnyAsync(p =>
                    p.Abbreviation == dto.Abbreviation &&
                    p.StorageId == partner.StorageId && // dùng storage của partner
                    p.Id != dto.Id))
                return ApiResponseResult<object>(false, "Mã đối tác đã tồn tại trong kho này", null);

            // Check Name
            if (!string.IsNullOrWhiteSpace(dto.Name) &&
                await _context.Partners.AnyAsync(p =>
                    p.Name == dto.Name &&
                    p.StorageId == partner.StorageId &&
                    p.Id != dto.Id))
                return ApiResponseResult<object>(false, "Tên đối tác đã tồn tại trong kho này", null);

            // Check TaxCode (trong Partner + trong User)
            if (!string.IsNullOrWhiteSpace(dto.TaxCode))
            {
                if (await _context.Partners.AnyAsync(p =>
                    p.TaxCode == dto.TaxCode &&
                    p.StorageId == partner.StorageId &&
                    p.Id != dto.Id))
                    return ApiResponseResult<object>(false, "Mã số thuế đã tồn tại trong kho này", null);

                // if (await _context.Users.AnyAsync(u =>
                //     u.Username == dto.Phone &&
                //     u.Id != partner.UserId))
                //     return ApiResponseResult<object>(false, "Số điện thoại đã được dùng làm username", null);
            }

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                // ========== UPDATE PARTNER ==========
                // partner.Code          = dto.Code;
                partner.Name          = dto.Name;
                partner.Address       = dto.Address;
                partner.TaxCode       = dto.TaxCode;
                // partner.Phone         = dto.Phone;
                // partner.Email         = dto.Email;
                partner.BankAccount   = dto.BankAccount;
                partner.Abbreviation  = dto.Abbreviation;
                partner.AllowedDebtDays = dto.AllowedDebtDays;
                partner.MaxDebt       = dto.MaxDebt;
                partner.Note          = dto.Note;
                partner.UpdatedAt     = DateTime.Now;
                partner.UpdatedBy     = userId;

                // ========== UPDATE USER nếu đổi phone/email ==========

                // if (!string.IsNullOrWhiteSpace(dto.Password))
                //     user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                    
                // user.Username = dto.Phone;  
                // user.Email = dto.Email;
                // user.LastName = dto.Abbreviation;
                // user.UpdatedAt = DateTime.Now;
                // user.UpdatedBy = userId;

                if (dto.CustomerId > 0)
                {
                    var cus = await _context.PartnerDetails.AsTracking().FirstOrDefaultAsync(x=>x.Id == dto.CustomerId);
                    if (cus != null)
                    {
                        cus.CustomerCreditLimit = dto.CustomerCreditLimit;
                        cus.CustomerCreditLimitMonth = dto.CustomerCreditLimitMonth;
                    }

                }
                if (dto.SupplierId > 0)
                {
                    var sup = await _context.PartnerDetails.AsTracking().FirstOrDefaultAsync(x=>x.Id == dto.SupplierId);
                    if (sup != null)
                    {
                        sup.SupplierCreditLimit = dto.SupplierCreditLimit;
                        sup.SupplierCreditLimitMonth = dto.SupplierCreditLimitMonth;
                    }

                }

                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                var pattern = $"GetPartnerInfoById*";
                var keys = _redis.GetKeysByPattern(pattern);
                foreach (var key in keys)
                {
                    await _redis.RemoveAsync(key);
                }
                return ApiResponseResult(true, "Cập nhật đối tác thành công", partner);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi cập nhật: " + ex.Message, null);
            }
        }
        [HttpPost("PartnerDetail/change-status")]
        public async Task<IActionResult> ChangeStatus([FromBody] PartnerDetailDto dto)
        {
            if (dto.Id <= 0)
                return ApiResponseResult<object>(false, "Id không hợp lệ", null);

            var partnerDetail = await _context.PartnerDetails
                .AsTracking()
                .FirstOrDefaultAsync(d => d.Id == dto.Id);

            if (partnerDetail == null)
                return ApiResponseResult<object>(false, "Không tìm thấy chi tiết đối tác", null);
            // ====== UPDATE STATUS ======
            //partnerDetail.Code = null;
            //if (dto.Status == 1)
            //{
            //    partnerDetail.Code = SqlServerHelpers.GenerateSoChungTu(_configuration.GetConnectionString("DefaultConnection"), "partner_details", "code", "KH", 4);
            //}
            //if (dto.Status == 2)
            //{
            //     partnerDetail.Code = SqlServerHelpers.GenerateSoChungTu(_configuration.GetConnectionString("DefaultConnection"), "partner_details", "code", "NCC", 4);
            //}
            partnerDetail.Status = dto.Status;
            partnerDetail.UpdatedAt = DateTime.Now;
            partnerDetail.UpdatedBy = userId;


            await _context.SaveChangesAsync();

            return ApiResponseResult(true, "Cập nhật trạng thái thành công", partnerDetail);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] PartnerDetailDto dto)
        {
            if (dto.Id <= 0)
                return ApiResponseResult<object>(false, "Id không hợp lệ", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var partner = await _context.Partners
                    .AsTracking()
                    .FirstOrDefaultAsync(p => p.Id == dto.Id);

                if (partner == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy đối tác", null);

                partner.DeletedAt = DateTime.Now;
                partner.DeletedBy = userId;
                partner.UpdatedAt = DateTime.Now;
                partner.UpdatedBy = userId;

                // chỉ lấy detail chưa deleted
                var partnerDetails = await _context.PartnerDetails
                    .AsTracking()
                    .Where(d => d.PartnerId == dto.Id)
                    .ToListAsync();

                foreach (var detail in partnerDetails)
                {
                    detail.DeletedAt = DateTime.Now;
                    detail.DeletedBy = userId;
                    detail.UpdatedAt = DateTime.Now;
                    detail.UpdatedBy = userId;
                }

               var countUseUser = await _context.Partners
                    .Where(p => p.UserId == partner.UserId && p.Id != partner.Id)
                    .CountAsync();

                if (countUseUser == 0)
                {
                    var user = await _context.Users
                        .AsTracking()
                        .FirstOrDefaultAsync(u => u.Id == partner.UserId);

                    if (user != null)
                    {
                        user.DeletedAt = DateTime.Now;
                        user.UpdatedAt = DateTime.Now;
                        user.UpdatedBy = userId;
                    }
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult<object>(true, "Xóa đối tác thành công", null);
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
            var entity =  await _repoPartner.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
