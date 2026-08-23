using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Works.Dtos;
using Vudaco.Works.Models;
using Vudaco.Works.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Works.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkController : BaseApiController
    {
        private readonly IWorkRepositories _repoWork;
        private readonly ILogger<WorkController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public WorkController(ILogger<WorkController> logger, IWorkRepositories repoWork, VudacoDBContext context)
        {
            _logger = logger;
            _repoWork = repoWork;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetWork(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] WorkDto WorkDto = null)
        {
            // test
            var result = await _repoWork.GetObjectWorkAsync(WorkDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] WorkDto WorkDto)
        {
            // Check trùng Name
            var entity = await _context.Works.FirstOrDefaultAsync(p =>p.StorageId == WorkDto.StorageId && p.Name == WorkDto.Name);
            if (entity != null)
                return ApiResponseResult<object>(false, "Tên dữ liệu đã tồn tại", null);
            // Check trùng Code
            entity = await _context.Works.FirstOrDefaultAsync(p => p.StorageId == WorkDto.StorageId && p.Code == WorkDto.Code);
            if (entity != null)
                return ApiResponseResult<object>(false, "code dữ liệu đã tồn tại", null);
                
            var Work = new Work
            {
                Code = WorkDto.Code,
                Name = WorkDto.Name,
                StorageId = WorkDto.StorageId,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            Work = await _repoWork.CreateAsync(Work);
            return ApiResponseResult(true, "Thêm thành công", Work);
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] WorkDto WorkDto)
        {
            if (WorkDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var Work = _context.Works.Find(WorkDto.Id);
            if (Work == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            // Check trùng Code
            if (!string.IsNullOrWhiteSpace(WorkDto.Code) &&
                await _context.Works.AnyAsync(p =>
                    p.Code == WorkDto.Code &&
                    p.StorageId == Work.StorageId &&
                    p.Id != WorkDto.Id))
                return ApiResponseResult<object>(false, "Code đã tồn tại trong kho này", null);
                  // Check trùng Name
            if (!string.IsNullOrWhiteSpace(WorkDto.Name) &&
                await _context.Works.AnyAsync(p =>
                    p.Name == WorkDto.Name &&
                    p.StorageId == Work.StorageId &&
                    p.Id != WorkDto.Id))
                return ApiResponseResult<object>(false, "Tên đã tồn tại trong kho này", null);
            
            Work.Code = WorkDto.Code;
            Work.Name = WorkDto.Name;
            Work.StorageId = WorkDto.StorageId;
            Work.UpdatedBy = userId;
            Work.UpdatedAt = DateTime.Now;
           
            Work = await _repoWork.UpdateAsync(Work);
            return ApiResponseResult(true, "Cập nhật thành công", Work);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  WorkDto WorkDto)
        {
            if (WorkDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Works.Find(WorkDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoWork.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoWork.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
