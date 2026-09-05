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
        public async Task<IActionResult> GetWork(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] WorkListDto WorkDto = null)
        {
            // test
            var result = await _repoWork.GetObjectTaskAsync(WorkDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
       
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CreateWorkRequest CreateWorkRequest)
        {
            using var tran = await _context.Database.BeginTransactionAsync();
            var conn = _context.Database.GetDbConnection();
            try
            {
                var now = DateTime.Now;
                if (CreateWorkRequest == null || CreateWorkRequest.CongViec == null || !CreateWorkRequest.CongViec.Any())
                {
                    throw new Exception("Danh sách công việc trống");
                }
                foreach (var workDto in CreateWorkRequest.CongViec)
                {
                    // =============================================
                    // 1. TẠO CÔNG VIỆC CHA
                    // =============================================
                    var parentWork = new Work
                    {
                        Name = workDto.TieuDe,
                        Description = null,
                        Type = workDto.LoaiCongViec,
                        Group = workDto.NhomCongViec,
                        ParentId = null,
                        StorageId = CreateWorkRequest.StorageId,
                        Status = 0,
                        AssigneeIds = null,
                        Attachments = workDto.FileList != null && workDto.FileList.Any()? JsonConvert.SerializeObject(workDto.FileList) : null,
                        Priority = 0,
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.Works.Add(parentWork);
                    // Save để lấy parentWork.Id
                    await _context.SaveChangesAsync();
                    // =============================================
                    // 2. DUYỆT DANH SÁCH CÔNG VIỆC CHI TIẾT
                    // =============================================
                    if (workDto.ChiTiet == null || !workDto.ChiTiet.Any())
                    {
                        continue;
                    }
                    foreach (var detailDto in workDto.ChiTiet)
                    {
                        if (string.IsNullOrWhiteSpace(detailDto.TenCongViec))
                        {
                            continue;
                        }
                        // =============================================
                        // 3. TẠO CÔNG VIỆC CON
                        // =============================================
                        var childWork = new Work
                        {
                            Name = detailDto.TenCongViec.Trim(),
                            Description = detailDto.MoTaCongViec,
                            Group = false,
                            Type = 0,
                            ParentId = parentWork.Id,
                            StorageId = CreateWorkRequest.StorageId,
                            AssigneeIds =detailDto.NguoiPhuTrach != null && detailDto.NguoiPhuTrach.Any()? JsonConvert.SerializeObject(detailDto.NguoiPhuTrach) : null,
                            DueDate = detailDto.HanHoanThanh,
                            CompletedDate = null,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                        _context.Works.Add(childWork);
                        // Save để lấy childWork.Id
                        await _context.SaveChangesAsync();
                        // =============================================
                        // 4. TẠO CHECKLIST
                        // =============================================
                        if (detailDto.Checklist == null || !detailDto.Checklist.Any())
                        {
                            continue;
                        }
                        foreach (var checklistItem in detailDto.Checklist)
                        {
                            if (string.IsNullOrWhiteSpace(checklistItem))
                            {
                                continue;
                            }
                            var workDetail = new WorkDetail
                            {
                                WorkId = childWork.Id,
                                Name = checklistItem.Trim(),
                                Description = null,
                                StorageId = CreateWorkRequest.StorageId,
                                Checked = false,
                                CreatedBy = userId,
                                CreatedAt = now,
                                UpdatedAt = now
                            };
                            _context.WorkDetails.Add(workDetail);
                        }
                    }
                    // =====================================================
                    // 1. CÔNG VIỆC LẶP LẠI
                    // =====================================================

                    if (workDto.LoaiCongViec == 1)
                    {
                        // =============================================
                        // 1. TẠO CÔNG VIỆC CHA
                        // =============================================
                        var parentWorkTemplate = new WorkTemplate
                        {
                            Name = workDto.TieuDe,
                            Description = null,
                            Type = 0,
                            WorkId=parentWork.Id,
                            Group = workDto.NhomCongViec,
                            StorageId = CreateWorkRequest.StorageId,
                            Attachments = workDto.FileList != null && workDto.FileList.Any()? JsonConvert.SerializeObject(workDto.FileList) : null,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                        _context.WorkTemplates.Add(parentWorkTemplate);
                        // Save để lấy parentWorkTemplate.Id
                        await _context.SaveChangesAsync();
                        // =============================================
                        // 2. DUYỆT DANH SÁCH CÔNG VIỆC CHI TIẾT
                        // =============================================
                        if (workDto.ChiTiet == null || !workDto.ChiTiet.Any())
                        {
                            continue;
                        }
                        foreach (var detailDto in workDto.ChiTiet)
                        {
                            if (string.IsNullOrWhiteSpace(detailDto.TenCongViec))
                            {
                                continue;
                            }
                            // =============================================
                            // 3. TẠO CÔNG VIỆC CON
                            // =============================================
                            var childWorkTemplate = new WorkTemplate
                            {
                                Name = detailDto.TenCongViec.Trim(),
                                Description = detailDto.MoTaCongViec,
                                Group = false,
                                Type = 0,
                                ParentId = parentWorkTemplate.Id,
                                StorageId = CreateWorkRequest.StorageId,
                                AssigneeIds =detailDto.NguoiPhuTrach != null && detailDto.NguoiPhuTrach.Any()? JsonConvert.SerializeObject(detailDto.NguoiPhuTrach) : null,
                                DueDate = detailDto.HanHoanThanh,
                                CreatedBy = userId,
                                CreatedAt = now,
                                UpdatedAt = now
                            };
                            _context.WorkTemplates.Add(childWorkTemplate);
                            // Save để lấy childWorkTemplate.Id
                            await _context.SaveChangesAsync();
                            // =============================================
                            // 4. TẠO CHECKLIST
                            // =============================================
                            if (detailDto.Checklist == null || !detailDto.Checklist.Any())
                            {
                                continue;
                            }
                            foreach (var checklistItem in detailDto.Checklist)
                            {
                                if (string.IsNullOrWhiteSpace(checklistItem))
                                {
                                    continue;
                                }
                                var workTemplateDetail = new WorkTemplateDetail
                                {
                                    WorkTemplateId = childWorkTemplate.Id,
                                    Name = checklistItem.Trim(),
                                    Description = null,
                                    StorageId = CreateWorkRequest.StorageId,
                                    CreatedBy = userId,
                                    CreatedAt = now,
                                    UpdatedAt = now
                                };
                                _context.WorkTemplateDetails.Add(workTemplateDetail);
                            }
                        }
                        var cronJob = new WorkCronJob
                        {
                            ModelId = parentWorkTemplate.Id,
                            Model = "WorkTemplate",
                            StorageId = CreateWorkRequest.StorageId,
                            StartDate = workDto.ThoiGianLap,
                            EndDate = workDto.ThoiGianKetThucLap,
                            CreatedBy = userId,
                            CreatedAt = now,
                            UpdatedAt = now
                        };
                        _context.WorkCronJobs.Add(cronJob);
                    }
                    var history = new WorkHistory
                    {
                        StorageId = CreateWorkRequest.StorageId,
                        Action = 1, // 1: Create
                        Type = 0, // 0: Work
                        ModelId = parentWork.Id,
                        Model = "Work",
                        Content = "Tạo công việc",
                        CreatedBy = userId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.WorkHistories.Add(history);
                    await _context.SaveChangesAsync();
                }
              
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Tạo công việc thành công", null);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
      
        // [HttpPost("delete")]
        // public async Task<IActionResult> Delete([FromBody]  WorkDto WorkDto)
        // {
            // if (WorkDto.Id <= 0)
            // {
            //     return ApiResponseResult<object>(false, "Id không tồn tại", null);
            // }
            // var entity = _context.Works.Find(WorkDto.Id);
            // if (entity == null)
            // {
            //     return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            // }
            // entity.DeletedBy = userId;
            // entity.DeletedAt = DateTime.Now;
            // await _repoWork.DeleteSoftAsync(entity);
            // return ApiResponseResult<object>(true, "Xóa thành công", null);
        // }
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
