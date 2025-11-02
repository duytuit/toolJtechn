using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Categorys.Dtos;
using Vudaco.Categorys.Models;
using Vudaco.Categorys.Repositories;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Categorys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FundCategoryController : BaseApiController
    {
        private readonly IFundCategoryRepository _repoFundCategoryRepository;
        private readonly ILogger<FundCategoryController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public FundCategoryController(ILogger<FundCategoryController> logger, IFundCategoryRepository repoFundCategoryRepository, VudacoDBContext context)
        {
            _logger = logger;
            _repoFundCategoryRepository = repoFundCategoryRepository;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] FundCategoryDto FundCategoryDto = null)
        {
            // test
            var result = await _repoFundCategoryRepository.GetObjectTaskAsync(FundCategoryDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
      
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] FundCategoryDto FundCategoryDto)
        {
            if (FundCategoryDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.FundCategorys.Find(FundCategoryDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoFundCategoryRepository.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoFundCategoryRepository.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
