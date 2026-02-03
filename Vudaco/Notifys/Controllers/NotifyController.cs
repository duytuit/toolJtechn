using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Notifys.Dtos;
using Vudaco.Notifys.Models;
using Vudaco.Notifys.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Notifys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyController : BaseApiController
    {
        private readonly INotifyRepositories _repoNotify;
        private readonly ILogger<NotifyController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public NotifyController(ILogger<NotifyController> logger, INotifyRepositories repoNotify, VudacoDBContext context)
        {
            _logger = logger;
            _repoNotify = repoNotify;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] NotifyDto notifyDto = null)
        {
            // test
            var result = await _repoNotify.GetObjectTaskAsync(notifyDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
       [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] NotifyDto notifyDto)
        {
            var now = DateTime.Now;
            if (notifyDto == null)
            {
                return ApiResponseResult<object>(false, "Dữ liệu không hợp lệ", null);
            }
            var notify = new Notify
            {
                StorageId = notifyDto.StorageId,
                EmployeeId = notifyDto.EmployeeId,
                Title = notifyDto.Title,
                Description = notifyDto.Description,
                Status = notifyDto.Status,
                Image = notifyDto.Image,
                CreatedBy = userId,
                CreatedAt = now,
            };
            notify = await _repoNotify.CreateAsync(notify);
            return ApiResponseResult(true, "Thêm thành công", notify);
        }
        [HttpPost("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus([FromBody] NotifyDto notifyDto)
        {
              if (notifyDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var notify = _context.Notifys.Find(notifyDto.Id);
            if (notify == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            notify.Status = notifyDto.Status;
            notify.UpdatedBy = userId;
            notify.UpdatedAt = DateTime.Now;
            await _repoNotify.UpdateAsync(notify);
            return ApiResponseResult(true, "Cập nhật trạng thái thành công", notify);     
        }
       
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] NotifyDto notifyDto )
        {
            if (notifyDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Notifys.Find(notifyDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            if (entity.Status == 1 || entity.Status == 2)
            {
                return ApiResponseResult<object>(false, "Phiếu đã được duyệt, không thể xóa", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoNotify.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoNotify.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
