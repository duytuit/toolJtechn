using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Categorys.Dtos;
using Vudaco.Categorys.Models;
using Vudaco.Categorys.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Categorys.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceCategoryController : BaseApiController
    {
        private readonly IServiceRepository _repoServiceCategory;
        private readonly ILogger<ServiceCategoryController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public ServiceCategoryController(ILogger<ServiceCategoryController> logger, IServiceRepository repoServiceCategory, VudacoDBContext context)
        {
            _logger = logger;
            _repoServiceCategory = repoServiceCategory;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] ServiceCategoryDto ServiceCategoryDto = null)
        {
            // test
            var result = await _repoServiceCategory.GetObjectTaskAsync(ServiceCategoryDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] ServiceCategoryDto ServiceCategoryDto)
        {
            // Check trùng Name
            var entity = await _context.ServiceCategorys.FirstOrDefaultAsync(p => p.Name == ServiceCategoryDto.Name);
            if (entity != null)
                return ApiResponseResult<object>(false, "Tên dữ liệu đã tồn tại", null);
            // Check trùng Code
            entity = await _context.ServiceCategorys.FirstOrDefaultAsync(p => p.Code == ServiceCategoryDto.Code);
            if (entity != null)
                return ApiResponseResult<object>(false, "code dữ liệu đã tồn tại", null);
                
            var ServiceCategory = new ServiceCategory
            {
                Code = ServiceCategoryDto.Code,
                Name = ServiceCategoryDto.Name,
                Type = ServiceCategoryDto.Type,
                Amount = ServiceCategoryDto.Amount,
                StorageId = ServiceCategoryDto.StorageId,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            ServiceCategory = await _repoServiceCategory.CreateAsync(ServiceCategory);
            return ApiResponseResult(true, "Thêm thành công", ServiceCategory);
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] ServiceCategoryDto ServiceCategoryDto)
        {
            if (ServiceCategoryDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var ServiceCategory = _context.ServiceCategorys.Find(ServiceCategoryDto.Id);
            if (ServiceCategory == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            // Check trùng Code
            if (!string.IsNullOrWhiteSpace(ServiceCategoryDto.Code) &&
                await _context.ServiceCategorys.AnyAsync(p =>
                    p.Code == ServiceCategoryDto.Code &&
                    p.StorageId == ServiceCategory.StorageId &&
                    p.Id != ServiceCategoryDto.Id))
                return ApiResponseResult<object>(false, "Code đã tồn tại trong kho này", null);
                  // Check trùng Name
            if (!string.IsNullOrWhiteSpace(ServiceCategoryDto.Name) &&
                await _context.ServiceCategorys.AnyAsync(p =>
                    p.Name == ServiceCategoryDto.Name &&
                    p.StorageId == ServiceCategory.StorageId &&
                    p.Id != ServiceCategoryDto.Id))
                return ApiResponseResult<object>(false, "Tên đã tồn tại trong kho này", null);
            
            ServiceCategory.Code = ServiceCategoryDto.Code;
            ServiceCategory.Name = ServiceCategoryDto.Name;
            ServiceCategory.Amount = ServiceCategoryDto.Amount;
            ServiceCategory.Type = ServiceCategoryDto.Type;
            ServiceCategory.StorageId = ServiceCategoryDto.StorageId;
            ServiceCategory.UpdatedBy = userId;
            ServiceCategory.UpdatedAt = DateTime.Now;
           
            ServiceCategory = await _repoServiceCategory.UpdateAsync(ServiceCategory);
            return ApiResponseResult(true, "Cập nhật thành công", ServiceCategory);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  ServiceCategoryDto ServiceCategoryDto)
        {
            if (ServiceCategoryDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.ServiceCategorys.Find(ServiceCategoryDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoServiceCategory.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoServiceCategory.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
