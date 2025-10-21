
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vudaco.ContractFiles.Dtos;
using Vudaco.ContractFiles.Models;
using Vudaco.ContractFiles.Repositories;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.Connects;

namespace Vudaco.ContractFiles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractFileController : BaseApiController
    {
        private readonly IContractFileRepository _repoContractFile;
        private readonly IContractFileDetailRepository _repoContractFileDetail;
        private readonly ILogger<ContractFileController> _logger;
        private readonly VudacoDBContext _context;
        public int userId => (int)HttpContext.Items["UserId"];
        public ContractFileController(ILogger<ContractFileController> logger,IContractFileDetailRepository repoContractFileDetail, IContractFileRepository repoContractFile, VudacoDBContext context)
        {
            _logger = logger;
            _repoContractFile = repoContractFile;
            _repoContractFileDetail = repoContractFileDetail;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FileInfoDto FileInfoDto = null)
        {
            // test
            var result = await _repoContractFile.GetObjectTaskAsync(FileInfoDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] FileInfoDto dto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrWhiteSpace(dto.FileNumber))
                    return ApiResponseResult<object>(false, "FileNumber bắt buộc", null);

                if (string.IsNullOrWhiteSpace(dto.SalesIds))
                    return ApiResponseResult<object>(false, "nhân viên sales bắt buộc", null);

                // Check trùng FileNumber trong cùng storage (bỏ qua soft-deleted)
                var fileInfos = await _context.FileInfos.AnyAsync(f =>
                    f.FileNumber == dto.FileNumber &&
                    f.StorageId == dto.StorageId);
                if (fileInfos)
                    return ApiResponseResult<object>(false, "FileNumber đã tồn tại trong kho này", null);


                var entity = new FileInfo
                {
                    PartnerDetailId = dto.PartnerDetailId,
                    StorageId = dto.StorageId,
                    FileNumber = dto.FileNumber,
                    Declaration = dto.Declaration,
                    Bill = dto.Bill,
                    Quantity = dto.Quantity,
                    ContainerCode = dto.ContainerCode,
                    SalesId = dto.SalesId,
                    Type = dto.Type,
                    Feature = dto.Feature,
                    DeclarationQuantity = dto.DeclarationQuantity,
                    DeclarationType = dto.DeclarationType,
                    Business = dto.Business,
                    Occurrence = dto.Occurrence,
                    Note = dto.Note,
                    CreatedBy = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = userId
                };
                _context.FileInfos.Add(entity);
                await _context.SaveChangesAsync();

                await tran.CommitAsync();
                return ApiResponseResult(true, "Thêm file thành công", entity);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi xóa: " + ex.Message, null);
            }
           
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] FileInfoDto dto)
        {
            var entity = await _context.FileInfos.AsTracking().FirstOrDefaultAsync(f => f.Id == dto.Id);
            if (entity == null)
                return ApiResponseResult<object>(false, "Không tìm thấy file", null);

            // Check trùng FileNumber trong cùng storage (bỏ qua soft-deleted và chính nó)
            var fileInfos = await _context.FileInfos.AnyAsync(f =>
                f.Id != dto.Id &&
                f.FileNumber == dto.FileNumber &&
                f.StorageId == dto.StorageId);
            if (fileInfos)
                return ApiResponseResult<object>(false, "FileNumber đã tồn tại trong kho này", null);

            entity.PartnerDetailId = dto.PartnerDetailId;
            entity.StorageId = dto.StorageId;
            entity.FileNumber = dto.FileNumber;
            entity.Declaration = dto.Declaration;
            entity.Bill = dto.Bill;
            entity.Quantity = dto.Quantity;
            entity.ContainerCode = dto.ContainerCode;
            entity.SalesId = dto.SalesId;
            entity.Type = dto.Type;
            entity.Feature = dto.Feature;
            entity.DeclarationQuantity = dto.DeclarationQuantity;
            entity.DeclarationType = dto.DeclarationType;
            entity.Business = dto.Business;
            entity.Occurrence = dto.Occurrence;
            entity.Note = dto.Note;
            entity.UpdatedBy = userId;
            entity.UpdatedAt = DateTime.Now;

            _context.FileInfos.Update(entity);
            await _context.SaveChangesAsync();
            return ApiResponseResult(true, "Cập nhật file thành công", entity);
        }
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var entity = await _context.FileInfos.AsTracking().FirstOrDefaultAsync(f => f.Id == id);
            if (entity == null)
                return ApiResponseResult<object>(false, "Không tìm thấy file", null);

            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            entity.UpdatedBy = userId;
            entity.UpdatedAt = DateTime.Now;

            _context.FileInfos.Update(entity);
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Xóa file thành công", null);
        }
      
    }
}
