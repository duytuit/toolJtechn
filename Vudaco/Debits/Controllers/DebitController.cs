using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Debits.Dtos;
using Vudaco.Debits.Models;
using Vudaco.Debits.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Debits.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DebitController : BaseApiController
    {
        private readonly IDebitRepositories _repoDebit;
        private readonly ILogger<DebitController> _logger;
        private readonly VudacoDBContext _context;
        public int userId => (int)HttpContext.Items["UserId"];

        public DebitController(ILogger<DebitController> logger, IDebitRepositories repoDebit, VudacoDBContext context)
        {
            _logger = logger;
            _repoDebit = repoDebit;
            _context = context;
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
          
            var Debit = new Debit
            {
                BillId = DebitDto.BillId,
                VehicleDispatchId = DebitDto.VehicleDispatchId,
                PartnerDetailId = DebitDto.PartnerDetailId,
                FileInfoId = DebitDto.FileInfoId,
                StorageId = DebitDto.StorageId,
                Type = DebitDto.Type,
                Name = DebitDto.Name,
                AccountingDate = DebitDto.AccountingDate,
                PurchasePrice = DebitDto.PurchasePrice,
                Price = DebitDto.Price,
                Vat = DebitDto.Vat,
                Status = DebitDto.Status,
                Note = DebitDto.Note,
                Data = DebitDto.Data,
                ApprovedByUser = DebitDto.ApprovedByUser,
                ApprovalTime = DebitDto.ApprovalTime,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            Debit = await _repoDebit.CreateAsync(Debit);
            return ApiResponseResult(true, "Thêm thành công", Debit);
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] DebitDto DebitDto)
        {
            if (DebitDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var Debit = _context.Debits.Find(DebitDto.Id);
            if (Debit == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            Debit.BillId = DebitDto.BillId;
            Debit.VehicleDispatchId = DebitDto.VehicleDispatchId;
            Debit.PartnerDetailId = DebitDto.PartnerDetailId;
            Debit.FileInfoId = DebitDto.FileInfoId;
            Debit.StorageId = DebitDto.StorageId;
            Debit.Type = DebitDto.Type;
            Debit.Name = DebitDto.Name;
            Debit.AccountingDate = DebitDto.AccountingDate;
            Debit.PurchasePrice = DebitDto.PurchasePrice;
            Debit.Price = DebitDto.Price;
            Debit.Vat = DebitDto.Vat;
            Debit.Status = DebitDto.Status;
            Debit.Note = DebitDto.Note;
            Debit.Data = DebitDto.Data;
            Debit.ApprovedByUser = DebitDto.ApprovedByUser;
            Debit.ApprovalTime = DebitDto.ApprovalTime;
            Debit.UpdatedBy = userId;
            Debit.UpdatedAt = DateTime.Now;
            Debit = await _repoDebit.UpdateAsync(Debit);
            return ApiResponseResult(true, "Cập nhật thành công", Debit);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  DebitDto DebitDto)
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
            var entity =  await _repoDebit.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
