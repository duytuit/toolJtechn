using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Comments.Dtos;
using Vudaco.Comments.Models;
using Vudaco.Comments.Repositories;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Comments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : BaseApiController
    {
        private readonly ICommentRepositories _repoComment;
        private readonly ILogger<CommentController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public CommentController(ILogger<CommentController> logger, ICommentRepositories repoComment, VudacoDBContext context)
        {
            _logger = logger;
            _repoComment = repoComment;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] CommentDto CommentDto = null)
        {
            // test
            var result = await _repoComment.GetObjectTaskAsync(CommentDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] CommentDto commentDto)
        {
            var now = DateTime.Now;
            if (commentDto == null)
            {
                return ApiResponseResult<object>(false, "Không có dữ liệu comment", null);
            }
            var comment = new Comment
            {
                EmployeeId = commentDto.EmployeeId,
                Message = commentDto.Message,
                Attach = JsonSerializer.Serialize(commentDto.AttachmentInfo),
                StorageId = commentDto.StorageId,
                PostId = commentDto.PostId,
                Type = commentDto.Type,
                ParentId = commentDto.ParentId,
                CreatedBy = userId,
                CreatedAt = now,
                UpdatedAt = now,
            };
            comment = await _repoComment.CreateAsync(comment);
            return ApiResponseResult(true, "Thêm thành công", comment);
        }
        [HttpPost("ChangeStatus")]
        public async Task<IActionResult> ChangeStatus([FromBody] CommentDto commentDto)
        {
              if (commentDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var comment = _context.Comments.Find(commentDto.Id);
            if (comment == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            comment.UpdatedBy = userId;
            comment.UpdatedAt = DateTime.Now;
            await _repoComment.UpdateAsync(comment);
            return ApiResponseResult(true, "Cập nhật trạng thái thành công", comment);     
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] CommentDto commentDto)
        {
            if (commentDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var comment = _context.Comments.Find(commentDto.Id);
            if (comment == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            comment.EmployeeId = commentDto.EmployeeId;
            comment.Message = commentDto.Message;
            comment.Attach = JsonSerializer.Serialize(commentDto.AttachmentInfo);   
            comment.UpdatedBy = userId;
            comment.UpdatedAt = DateTime.Now;
            comment = await _repoComment.UpdateAsync(comment);
            return ApiResponseResult(true, "Cập nhật thành công", comment);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] CommentDto commentDto)
        {
            if (commentDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Comments.Find(commentDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoComment.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoComment.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
