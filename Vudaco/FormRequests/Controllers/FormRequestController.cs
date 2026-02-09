using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.FormRequests.Dtos;
using Vudaco.FormRequests.Models;
using Vudaco.FormRequests.Repositories;
using Vudaco.Notifys.Dtos;
using Vudaco.Notifys.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.FormRequests.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FormRequestController : BaseApiController
    {
        private readonly IFormRequestRepositories _repoFormRequest;
        private readonly ILogger<FormRequestController> _logger;
        private readonly VudacoDBContext _context;
        private readonly IFcmQueue _fcmQueue;
        public int userId => (int)HttpContext.Items["UserId"];

        public FormRequestController(ILogger<FormRequestController> logger, IFormRequestRepositories repoFormRequest, VudacoDBContext context, IFcmQueue fcmQueue)
        {
            _logger = logger;
            _fcmQueue = fcmQueue;
            _repoFormRequest = repoFormRequest;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FormRequestDto formRequestDto = null)
        {
            // test
            var result = await _repoFormRequest.GetObjectTaskAsync(formRequestDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("CreateLeaveRequest")]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] FormRequestLeaveDto formRequestLeaveDto)
        {
            var now = DateTime.Now;
            if (!formRequestLeaveDto.LeaveRequestDto.Any())
            {
                return ApiResponseResult<object>(false, "Không có dữ liệu ngày nghỉ", null);
            }
            var formRequest = new FormRequest
            {
                EmployeeId = formRequestLeaveDto.EmployeeId,
                Note = formRequestLeaveDto.Note,
                Description = JsonSerializer.Serialize(formRequestLeaveDto.LeaveRequestDto),
                StorageId = formRequestLeaveDto.StorageId,
                totalDayHasPaidLeave = formRequestLeaveDto.LeaveRequestDto.Where(x => x.IsPaidLeave == 1).Sum(x => x.DurationLeave),
                totalDayNoPaidLeave = formRequestLeaveDto.LeaveRequestDto.Where(x => x.IsPaidLeave == 0).Sum(x => x.DurationLeave),
                CreatedBy = userId,
                CreatedAt = now,
                UpdatedAt = now,
            };
            formRequest = await _repoFormRequest.CreateAsync(formRequest);
            var getEmp = await _context.Employees.FirstOrDefaultAsync(x => x.Id == formRequest.EmployeeId);
            var userIds = await _context.Employees
                .Where(x => x.StorageId == formRequest.StorageId && x.Id != formRequest.EmployeeId && x.UserId.HasValue)
                .Select(x => x.UserId.Value)
                .Distinct()
                .ToListAsync();

            if (userIds.Any())
            {
                await _fcmQueue.EnqueueAsync(new FcmJobDto
                {
                    UserIds = userIds,
                    Title = "Yêu cầu nghỉ phép mới",
                    Body = "Có một yêu cầu nghỉ phép mới của " + (getEmp != null ? getEmp.FirstName + " " + getEmp.LastName : ""),
                    StorageId = formRequest.StorageId,
                    PostId = formRequest.Id,
                    Type = 0,
                    Screen = "nghiphep"
                });
            }
            return ApiResponseResult(true, "Thêm thành công", formRequest);
        }
        [HttpPost("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus([FromBody] FormRequestDto formRequestDto)
        {
              if (formRequestDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var formRequest = _context.FormRequests.Find(formRequestDto.Id);
            if (formRequest == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            formRequest.Status = formRequestDto.Status;
            formRequest.ConfirmAt = formRequestDto.Status == 1 || formRequestDto.Status == 2 ? DateTime.Now : (DateTime?)null;
            formRequest.ConfirmBy = formRequestDto.Status == 1 || formRequestDto.Status == 2 ? userId : (int?)null;
            formRequest.UpdatedBy = userId;
            formRequest.UpdatedAt = DateTime.Now;
            await _repoFormRequest.UpdateAsync(formRequest);
            var getEmp = await _context.Employees.FirstOrDefaultAsync(x => x.Id == formRequest.EmployeeId);
            var userIds = await _context.Employees
                .Where(x => x.StorageId == formRequest.StorageId && x.UserId.HasValue && x.UserId != formRequest.ConfirmBy)
                .Select(x => x.UserId.Value)
                .Distinct()
                .ToListAsync();
            if (userIds.Any())
            {   
                string statusText = formRequestDto.Status == 1 ? "đã được duyệt" : formRequestDto.Status == 2 ? "bị từ chối" : "đang chờ duyệt";
                await _fcmQueue.EnqueueAsync(new FcmJobDto
                {
                    UserIds = userIds,
                    Title = "Cập nhật trạng thái yêu cầu nghỉ phép",
                    Body = $"Yêu cầu nghỉ phép của bạn {getEmp?.FirstName} {getEmp?.LastName} {statusText}",
                    StorageId = formRequest.StorageId,
                    PostId = formRequest.Id,
                    Type = 0,
                    Screen = "nghiphep"
                });
            }
            return ApiResponseResult(true, "Cập nhật trạng thái thành công", formRequest);     
        }
        [HttpPost("UpdateLeaveRequest")]
        public async Task<IActionResult> UpdateLeaveRequest([FromBody] FormRequestLeaveDto formRequestLeaveDto)
        {
            if (formRequestLeaveDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var formRequest = _context.FormRequests.Find(formRequestLeaveDto.Id);
            if (formRequest == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            formRequest.EmployeeId = formRequestLeaveDto.EmployeeId;
            formRequest.Note = formRequestLeaveDto.Note;
            formRequest.Description = JsonSerializer.Serialize(formRequestLeaveDto.LeaveRequestDto);
            formRequest.totalDayHasPaidLeave = formRequestLeaveDto.LeaveRequestDto.Where(x => x.IsPaidLeave == 1).Sum(x => x.DurationLeave);
            formRequest.totalDayNoPaidLeave = formRequestLeaveDto.LeaveRequestDto.Where(x => x.IsPaidLeave == 0).Sum(x => x.DurationLeave);
            formRequest.StorageId = formRequestLeaveDto.StorageId;
            formRequest.UpdatedBy = userId;
            formRequest.UpdatedAt = DateTime.Now;
           
            formRequest = await _repoFormRequest.UpdateAsync(formRequest);
            return ApiResponseResult(true, "Cập nhật thành công", formRequest);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] FormRequestDto formRequestDto)
        {
            if (formRequestDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.FormRequests.Find(formRequestDto.Id);
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
            var getEmp = await _context.Employees.FirstOrDefaultAsync(x => x.Id == entity.EmployeeId);
            var userIds = await _context.Employees
                .Where(x => x.StorageId == entity.StorageId && x.UserId.HasValue && x.UserId != entity.CreatedBy)
                .Select(x => x.UserId.Value)
                .Distinct()
                .ToListAsync();
            await _repoFormRequest.DeleteSoftAsync(entity);
            await _fcmQueue.EnqueueAsync(new FcmJobDto
            {
                UserIds = userIds,
                Title = "Xóa yêu cầu nghỉ phép",
                Body = $"Bạn {getEmp?.FirstName} {getEmp?.LastName} đã xóa yêu cầu nghỉ phép",
                StorageId = entity.StorageId,
                PostId = entity.Id,
                Type = 0,
                Screen = "nghiphep"
            }); 
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoFormRequest.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
      
    }
}
