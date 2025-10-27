using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Departments.Dtos;
using Vudaco.Departments.Models;
using Vudaco.Departments.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Departments.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : BaseApiController
    {
        private readonly IDepartmentRepositories _repoDepartment;
        private readonly ILogger<DepartmentController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public DepartmentController(ILogger<DepartmentController> logger, IDepartmentRepositories repoDepartment, VudacoDBContext context)
        {
            _logger = logger;
            _repoDepartment = repoDepartment;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DepartmentDto DepartmentDto = null)
        {
            // test
            var result = await _repoDepartment.GetObjectTaskAsync(DepartmentDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] DepartmentDto DepartmentDto)
        {
            // Check trùng Name
            var entity = await _context.Departments.FirstOrDefaultAsync(p => p.Name == DepartmentDto.Name);
            if (entity != null)
                return ApiResponseResult<object>(false, "Tên dữ liệu đã tồn tại", null);
            // Check trùng Code
            entity = await _context.Departments.FirstOrDefaultAsync(p => p.Code == DepartmentDto.Code);
            if (entity != null)
                return ApiResponseResult<object>(false, "code dữ liệu đã tồn tại", null);
                
            var Department = new Department
            {
                Code = DepartmentDto.Code,
                Name = DepartmentDto.Name,
                StorageId = DepartmentDto.StorageId,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            Department = await _repoDepartment.CreateAsync(Department);
            return ApiResponseResult(true, "Thêm thành công", Department);
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] DepartmentDto DepartmentDto)
        {
            if (DepartmentDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var Department = _context.Departments.Find(DepartmentDto.Id);
            if (Department == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            // Check trùng Code
            if (!string.IsNullOrWhiteSpace(DepartmentDto.Code) &&
                await _context.Departments.AnyAsync(p =>
                    p.Code == DepartmentDto.Code &&
                    p.StorageId == Department.StorageId &&
                    p.Id != DepartmentDto.Id))
                return ApiResponseResult<object>(false, "Code đã tồn tại trong kho này", null);
                  // Check trùng Name
            if (!string.IsNullOrWhiteSpace(DepartmentDto.Name) &&
                await _context.Departments.AnyAsync(p =>
                    p.Name == DepartmentDto.Name &&
                    p.StorageId == Department.StorageId &&
                    p.Id != DepartmentDto.Id))
                return ApiResponseResult<object>(false, "Tên đã tồn tại trong kho này", null);
            
            Department.Code = DepartmentDto.Code;
            Department.Name = DepartmentDto.Name;
            Department.StorageId = DepartmentDto.StorageId;
            Department.UpdatedBy = userId;
            Department.UpdatedAt = DateTime.Now;
           
            Department = await _repoDepartment.UpdateAsync(Department);
            return ApiResponseResult(true, "Cập nhật thành công", Department);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  DepartmentDto DepartmentDto)
        {
            if (DepartmentDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Departments.Find(DepartmentDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoDepartment.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoDepartment.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
