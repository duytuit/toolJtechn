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
    public class WorkCommentController : BaseApiController
    {
        private readonly ICommentRepositories _repoWorkComment;
        private readonly ILogger<WorkCommentController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public WorkCommentController(ILogger<WorkCommentController> logger, ICommentRepositories repoWorkComment, VudacoDBContext context)
        {
            _logger = logger;
            _repoWorkComment = repoWorkComment;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetWork(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] WorkCommentDto WorkCommentDto = null)
        {
            // test
            var result = await _repoWorkComment.GetObjectTaskAsync(WorkCommentDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
       
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] WorkCommentDto WorkCommentDto)
        {
            if (WorkCommentDto == null)
            {
                return ApiResponseResult<object>(false, "Dữ liệu không hợp lệ", null);
            }
            var now = DateTime.Now;
            var workComment = new WorkComment
            {
                ParentId = WorkCommentDto.ParentId,
                StorageId = WorkCommentDto.StorageId,
                Model = WorkCommentDto.Model,
                ModelId = WorkCommentDto.ModelId,
                Content = WorkCommentDto.Content,
                Type = 0,
                CreatedBy = userId,
                CreatedAt = now,
                UpdatedAt = now
            };
            var result = await _repoWorkComment.CreateAsync(workComment);
            return ApiResponseResult(true, "Tạo dữ liệu thành công", result);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoWorkComment.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
         [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  WorkCommentDto WorkCommentDto)
        {
            if (WorkCommentDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.WorkComments.Find(WorkCommentDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoWorkComment.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
    }
}
