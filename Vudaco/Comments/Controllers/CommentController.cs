using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Comments.Dtos;
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
      
    }
}
