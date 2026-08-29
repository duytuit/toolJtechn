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
            var result = await _repoWork.GetObjectTaskAsync(WorkDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
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
