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
using Newtonsoft.Json;

namespace Vudaco.Works.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkDetailController : BaseApiController
    {
        private readonly IWorkDetailRepositories _repoWorkDetail;
        private readonly ILogger<WorkDetailController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public WorkDetailController(ILogger<WorkDetailController> logger, IWorkDetailRepositories repoWorkDetail, VudacoDBContext context)
        {
            _logger = logger;
            _repoWorkDetail = repoWorkDetail;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetWork(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] CheckListDto CheckListDto = null)
        {
            // test
            var result = await _repoWorkDetail.GetObjectTaskAsync(CheckListDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
       
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CheckListDto CheckListDto)
        {
            if (CheckListDto == null)
            {
                return ApiResponseResult<object>(false, "Dữ liệu không hợp lệ", null);
            }
            var now = DateTime.Now;
            var workDetail = new WorkDetail
            {
                WorkId = CheckListDto.WorkId,
                Name = CheckListDto.Name,
                Description = CheckListDto.Description,
                StorageId = CheckListDto.StorageId,
                Checked = CheckListDto.Checked,
                CreatedBy = userId,
                CreatedAt = now,
                UpdatedAt = now
            };
            var result = await _repoWorkDetail.CreateAsync(workDetail);
            var history = new WorkHistory
            {
                StorageId=CheckListDto.StorageId,
                Action=1, // 1: Create
                Type=0, // 0: Checklist
                ModelId=CheckListDto.Id,
                Model="Work",
                Content="Tạo checklist: " + CheckListDto.Name,
                CreatedBy=userId,
                CreatedAt=now,
                UpdatedAt=now
            };
            _context.WorkHistories.Add(history);
            await _context.SaveChangesAsync();
            return ApiResponseResult(true, "Tạo dữ liệu thành công", result);
        }
        [HttpPost("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus([FromBody] CheckListDto CheckListDto)
        {
            if (CheckListDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.WorkDetails.Find(CheckListDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.Checked = CheckListDto.Checked;
            entity.UpdatedBy = userId;
            entity.UpdatedAt = DateTime.Now;
            await _repoWorkDetail.UpdateAsync(entity);
            var history = new WorkHistory
            {
                StorageId=CheckListDto.StorageId,
                Action=2, // 2: Update
                Type=0, // 0: Checklist
                ModelId=CheckListDto.Id,
                Model="Work",
                Content="Cập nhật trạng thái checklist: " + CheckListDto.Name + " thành " + (CheckListDto.Checked ? "Đã hoàn thành" : "Chưa hoàn thành"),
                CreatedBy=userId,
                CreatedAt=DateTime.Now,
                UpdatedAt=DateTime.Now
            };
            _context.WorkHistories.Add(history);
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Cập nhật thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoWorkDetail.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
         [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  CheckListDto CheckListDto)
        {
            if (CheckListDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.WorkDetails.Find(CheckListDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoWorkDetail.DeleteSoftAsync(entity);
            var history = new WorkHistory
            {
                StorageId=CheckListDto.StorageId,
                Action=3, // 3: Delete
                Type=0, // 0: Checklist
                ModelId=CheckListDto.Id,
                Model="Work",
                Content="Xóa checklist: " + CheckListDto.Name,
                CreatedBy=userId,
                CreatedAt=DateTime.Now,
                UpdatedAt=DateTime.Now
            };
            _context.WorkHistories.Add(history);
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
    }
}
