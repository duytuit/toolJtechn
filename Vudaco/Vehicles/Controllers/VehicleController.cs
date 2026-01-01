using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;
using Vudaco.Vehicles.Repositories;
using Vudaco.Vehicles.Dtos;
using Vudaco.Vehicles.Models;

namespace Vudaco.Vehicles.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : BaseApiController
    {
        private readonly IVehicleRepositories _repoVehicle;
        private readonly IVehicleDispatchRepositories _repoVehicleDispatch;
        private readonly ILogger<VehicleController> _logger;
        private readonly VudacoDBContext _context;
         public int userId => (int)HttpContext.Items["UserId"];

        public VehicleController(ILogger<VehicleController> logger, IVehicleDispatchRepositories repoVehicleDispatch, IVehicleRepositories repoVehicle, VudacoDBContext context)
        {
            _logger = logger;
            _repoVehicle = repoVehicle;
            _repoVehicleDispatch = repoVehicleDispatch;
            _context = context;
        }
         [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] VehicleDto VehicleDto = null)
        {
            // test
            var result = await _repoVehicle.GetObjectTaskAsync(VehicleDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] VehicleDto VehicleDto)
        {
            // Check trùng Name
            var entity = await _context.Vehicles.FirstOrDefaultAsync(p =>p.StorageId == VehicleDto.StorageId && p.NumberCode == VehicleDto.NumberCode);
            if (entity != null)
                return ApiResponseResult<object>(false, "Tên dữ liệu đã tồn tại", null);
                
            var Vehicle = new Vehicle
            {
                NumberCode = VehicleDto.NumberCode,
                IsExternalDriver = VehicleDto.IsExternalDriver,
                Note = VehicleDto.Note,
                StorageId=VehicleDto.StorageId,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            Vehicle = await _repoVehicle.CreateAsync(Vehicle);
            return ApiResponseResult(true, "Thêm thành công", Vehicle);
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] VehicleDto VehicleDto)
        {
            if (VehicleDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var vehicle = _context.Vehicles.Find(VehicleDto.Id);
            if (vehicle == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            // Check trùng NumberCode
            if (!string.IsNullOrWhiteSpace(VehicleDto.NumberCode) &&
                await _context.Vehicles.AnyAsync(p =>
                    p.NumberCode == VehicleDto.NumberCode &&
                    p.StorageId == vehicle.StorageId &&
                    p.Id != VehicleDto.Id))
                return ApiResponseResult<object>(false, "Tên đối tác đã tồn tại trong kho này", null);
            
            vehicle.NumberCode = VehicleDto.NumberCode;
            vehicle.IsExternalDriver = VehicleDto.IsExternalDriver;
            vehicle.Note = VehicleDto.Note;
            vehicle.UpdatedBy = userId;
            vehicle.UpdatedAt = DateTime.Now;
           
            vehicle = await _repoVehicle.UpdateAsync(vehicle);
            return ApiResponseResult(true, "Cập nhật thành công", vehicle);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  VehicleDto VehicleDto)
        {
            if (VehicleDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Vehicles.Find(VehicleDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoVehicle.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoVehicle.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
