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
    public class IncomeExpenseController : BaseApiController
    {
        private readonly IIncomeExpenseCategoryRepository _repoIncomeExpenseCategory;
        private readonly ILogger<IncomeExpenseController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public IncomeExpenseController(ILogger<IncomeExpenseController> logger, IIncomeExpenseCategoryRepository repoIncomeExpenseCategory, VudacoDBContext context)
        {
            _logger = logger;
            _repoIncomeExpenseCategory = repoIncomeExpenseCategory;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] IncomeExpenseCategoryDto IncomeExpenseCategoryDto = null)
        {
            // test
            var result = await _repoIncomeExpenseCategory.GetObjectTaskAsync(IncomeExpenseCategoryDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] IncomeExpenseCategoryDto IncomeExpenseCategoryDto)
        {
            // Check trùng Name
            var entity = await _context.IncomeExpenseCategorys.FirstOrDefaultAsync(p => p.Name == IncomeExpenseCategoryDto.Name);
            if (entity != null)
                return ApiResponseResult<object>(false, "Tên dữ liệu đã tồn tại", null);
            var IncomeExpenseCategory = new IncomeExpenseCategory
            {
                Code = IncomeExpenseCategoryDto.Code,
                Name = IncomeExpenseCategoryDto.Name,
                Type = IncomeExpenseCategoryDto.Type,
                StorageId = IncomeExpenseCategoryDto.StorageId,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            IncomeExpenseCategory = await _repoIncomeExpenseCategory.CreateAsync(IncomeExpenseCategory);
            return ApiResponseResult(true, "Thêm thành công", IncomeExpenseCategory);
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] IncomeExpenseCategoryDto IncomeExpenseCategoryDto)
        {
            if (IncomeExpenseCategoryDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var IncomeExpenseCategory = _context.IncomeExpenseCategorys.Find(IncomeExpenseCategoryDto.Id);
            if (IncomeExpenseCategory == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
                  // Check trùng Name
            if (!string.IsNullOrWhiteSpace(IncomeExpenseCategoryDto.Name) &&
                await _context.IncomeExpenseCategorys.AnyAsync(p =>
                    p.Name == IncomeExpenseCategoryDto.Name &&
                    p.StorageId == IncomeExpenseCategory.StorageId &&
                    p.Id != IncomeExpenseCategoryDto.Id))
                return ApiResponseResult<object>(false, "Tên đã tồn tại trong kho này", null);
            
            IncomeExpenseCategory.Code = IncomeExpenseCategoryDto.Code;
            IncomeExpenseCategory.Name = IncomeExpenseCategoryDto.Name;
            IncomeExpenseCategory.Type = IncomeExpenseCategoryDto.Type;
            IncomeExpenseCategory.StorageId = IncomeExpenseCategoryDto.StorageId;
            IncomeExpenseCategory.UpdatedBy = userId;
            IncomeExpenseCategory.UpdatedAt = DateTime.Now;
           
            IncomeExpenseCategory = await _repoIncomeExpenseCategory.UpdateAsync(IncomeExpenseCategory);
            return ApiResponseResult(true, "Cập nhật thành công", IncomeExpenseCategory);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  IncomeExpenseCategoryDto IncomeExpenseCategoryDto)
        {
            if (IncomeExpenseCategoryDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.IncomeExpenseCategorys.Find(IncomeExpenseCategoryDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoIncomeExpenseCategory.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoIncomeExpenseCategory.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
