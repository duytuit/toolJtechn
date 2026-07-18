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
using Vudaco.Depreciations.Models;
using Vudaco.Depreciations.Repositories;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Depreciations.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepreciationController : BaseApiController
    {
        private readonly IDepreciationRepositories _repoDepreciation;
        private readonly IDepreciationAllocationRepositories _repoDepreciationAllocation;
        private readonly IDepreciationAllocationDetailRepositories _repoDepreciationAllocationDetail;
        private readonly ILogger<DepreciationController> _logger;
        private readonly VudacoDBContext _context;
        public int userId => (int)HttpContext.Items["UserId"];
        public DepreciationController(ILogger<DepreciationController> logger, IDepreciationRepositories repoDepreciation, IDepreciationAllocationRepositories repoDepreciationAllocation, IDepreciationAllocationDetailRepositories repoDepreciationAllocationDetail,    VudacoDBContext context)
        {
            _logger = logger;
            _repoDepreciation = repoDepreciation;
            _repoDepreciationAllocation = repoDepreciationAllocation;
            _repoDepreciationAllocationDetail = repoDepreciationAllocationDetail;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetObjectAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DepreciationDto DepreciationDto = null)
        {
            var result = await _repoDepreciation.GetObjectTaskAsync(DepreciationDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("allocation")]
        public async Task<IActionResult> GetDepreciationAllocationAsync(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DepreciationAllocationDto DepreciationAllocationDto = null)
        {
            var result = await _repoDepreciation.GetDepreciationAllocationAsync(DepreciationAllocationDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ListDepreciationDto ListDepreciationDto)
        {

            await using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();

            try
            {
                var now = DateTime.Now;
                foreach (var item in ListDepreciationDto.Data)
                {
                    var entity = new Depreciation
                    {
                        CodeNumber = item.CodeNumber,
                        Name = item.Name,
                        Type = ListDepreciationDto.Type,
                        OriginalCost = item.OriginalCost,
                        UsefulLife = item.UsefulLife,
                        VehicleId = item.VehicleId,
                        MonthlyDepreciation = item.MonthlyDepreciation,
                        Note = item.Note,
                        CreateDate = item.CreateDate,
                        StorageId = ListDepreciationDto.StorageId,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                    };
                    _context.Depreciations.Add(entity);
                }
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
         [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] DepreciationDto DepreciationDto)
        {
            if (DepreciationDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var Depreciation = _context.Depreciations.Find(DepreciationDto.Id);
            if (Depreciation == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            // Check trùng Code
            // if (!string.IsNullOrWhiteSpace(DepreciationDto.CodeNumber) &&
            //     await _context.Depreciations.AnyAsync(p =>
            //         p.CodeNumber == DepreciationDto.CodeNumber &&
            //         p.StorageId == Depreciation.StorageId &&
            //         p.Id != DepreciationDto.Id))
            //     return ApiResponseResult<object>(false, "Code đã tồn tại trong kho này", null);
            // Check trùng Name
            if (!string.IsNullOrWhiteSpace(DepreciationDto.Name) &&
                await _context.Depreciations.AnyAsync(p =>
                    p.Name == DepreciationDto.Name &&
                    p.StorageId == Depreciation.StorageId &&
                    p.Id != DepreciationDto.Id))
                return ApiResponseResult<object>(false, "Tên đã tồn tại trong kho này", null);
            
            Depreciation.CodeNumber = DepreciationDto.CodeNumber;
            Depreciation.Name = DepreciationDto.Name;
            Depreciation.OriginalCost = DepreciationDto.OriginalCost;
            Depreciation.UsefulLife = DepreciationDto.UsefulLife;
            Depreciation.MonthlyDepreciation = DepreciationDto.MonthlyDepreciation;
            Depreciation.StorageId = DepreciationDto.StorageId;
            Depreciation.VehicleId = DepreciationDto.VehicleId;
            Depreciation.Note = DepreciationDto.Note;
            Depreciation.CreateDate = DepreciationDto.CreateDate;
            Depreciation.UpdatedBy = userId;
            Depreciation.Status = DepreciationDto.Status;
            Depreciation.UpdatedAt = DateTime.Now;
           
            Depreciation = await _repoDepreciation.UpdateAsync(Depreciation);
            return ApiResponseResult(true, "Cập nhật thành công", Depreciation);
        }
        [HttpPost("updateDepreciationAllocation")]
        public async Task<IActionResult> UpdateDepreciationAllocation([FromBody]  DepreciationAllocationDto DepreciationAllocationDto)
        {
            await using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();

            try
            {
                var now = DateTime.Now;
                var entity = await _context.DepreciationAllocations.Where(x => x.Type == DepreciationAllocationDto.Type && x.StorageId == DepreciationAllocationDto.StorageId && x.CycleName == DepreciationAllocationDto.CycleName).FirstOrDefaultAsync();
                if (entity == null)                
                {
                    entity = new DepreciationAllocation
                    {
                        Note = DepreciationAllocationDto.Note,
                        CycleName = DepreciationAllocationDto.CycleName,
                        Type = DepreciationAllocationDto.Type,  
                        AccountingDate = DepreciationAllocationDto.AccountingDate,
                        StorageId = DepreciationAllocationDto.StorageId,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now,
                    };
                    _context.DepreciationAllocations.Add(entity);
                }
                else
                {
                    entity.Note = DepreciationAllocationDto.Note;
                    entity.AccountingDate = DepreciationAllocationDto.AccountingDate;
                    entity.UpdatedBy = userId;
                    entity.UpdatedAt = now;
                    _context.DepreciationAllocations.Update(entity);
                }
                await _context.SaveChangesAsync();
                var getDepreciations = await _context.Depreciations.Where(x => x.StorageId == DepreciationAllocationDto.StorageId && x.Type == DepreciationAllocationDto.Type).ToListAsync();
                foreach (var item in getDepreciations)
                {
                    var getDepreciationAllocationDetail = await _context.DepreciationAllocationDetails.Where(x => x.StorageId == DepreciationAllocationDto.StorageId && x.DepreciationAllocationId == entity.Id && x.DepreciationId == item.Id).FirstOrDefaultAsync();
                    if (getDepreciationAllocationDetail == null)
                    {
                        getDepreciationAllocationDetail = new DepreciationAllocationDetail
                        {
                            StorageId = DepreciationAllocationDto.StorageId,
                            DepreciationAllocationId = entity.Id,
                            DepreciationId = item.Id,
                            MonthlyDepreciation = item.MonthlyDepreciation ?? 0,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now,
                        };
                        _context.DepreciationAllocationDetails.Add(getDepreciationAllocationDetail);
                    }
                    else
                    {
                        getDepreciationAllocationDetail.MonthlyDepreciation = item.MonthlyDepreciation ?? 0;
                        getDepreciationAllocationDetail.UpdatedBy = userId;
                        getDepreciationAllocationDetail.UpdatedAt = now;
                        _context.DepreciationAllocationDetails.Update(getDepreciationAllocationDetail);
                    }
                }
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
        public async Task<IActionResult> Delete([FromBody]  DepreciationDto DepreciationDto)
        {
            if (DepreciationDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Depreciations.Find(DepreciationDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoDepreciation.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpPost("deleteAllocation")]
        public async Task<IActionResult> DeleteAllocation([FromBody]  DepreciationDto DepreciationDto)
        {
            if (DepreciationDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.DepreciationAllocations.Find(DepreciationDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoDepreciationAllocation.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoDepreciation.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        [HttpGet("getDepreciationAllocationDetailByDepreciationAllocationId")]
        public async Task<IActionResult> GetDepreciationAllocationDetailByDepreciationAllocationId([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _context.DepreciationAllocationDetails.Where(i => i.DepreciationAllocationId == id)
             .Join(
                    _context.Depreciations,
                    pd => pd.DepreciationId,
                    p => p.Id,
                    (pd, p) => new
                    {
                        pd.Id,
                        pd.MonthlyDepreciation,
                        p.CodeNumber,
                        p.Name
                    }
                )
            .ToListAsync();
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
