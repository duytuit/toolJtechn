using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;
using Vudaco.Storages.Dtos;
using Vudaco.Storages.Models;
using Vudaco.Storages.Repositories;

namespace Vudaco.Storages.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StogaresController : BaseApiController
    {
        private readonly IStorageRepository _repoStogare;
        private readonly IUserStorageRepository _repoUserStorage;
        private readonly ILogger<StogaresController> _logger;
        private readonly VudacoDBContext _context;
        public int userId => (int)HttpContext.Items["UserId"];

        public StogaresController(ILogger<StogaresController> logger, IUserStorageRepository repoUserStorage, IStorageRepository repoStogare, VudacoDBContext context)
        {
            _logger = logger;
            _repoStogare = repoStogare;
            _repoUserStorage = repoUserStorage;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] StorageDto StorageDto = null)
        {
            // test
            var result = await _repoStogare.GetObjectTaskAsync(StorageDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] StorageDto StorageDto)
        {
            var storage = new Storage
            {
                 Code = StorageDto.Code,
                 Name = StorageDto.Name,
                 Note = StorageDto.Note,
                 Address = StorageDto.Address,
                 CreatedBy = userId,
                 CreatedAt = DateTime.Now,
                 UpdatedAt = DateTime.Now,
            };
            storage = await _repoStogare.CreateAsync(storage);
            return ApiResponseResult(true, "Thêm thành công", storage);
        }
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] StorageDto StorageDto)
        {
            if (StorageDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var storage = _context.Storages.Find(StorageDto.Id);
            if (storage == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            storage.Code = StorageDto.Code;
            storage.Name = StorageDto.Name;
            storage.Note = StorageDto.Note;
            storage.Address = StorageDto.Address;
            storage.UpdatedBy = userId;
            storage.UpdatedAt = DateTime.Now;
           
            storage = await _repoStogare.UpdateAsync(storage);
            return ApiResponseResult(true, "Cập nhật thành công", storage);
        }
        [HttpDelete("{id}")]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var storage = _context.Storages.Find(id);
            if (storage == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            storage.DeletedBy = userId;
            storage.DeletedAt = DateTime.Now;
            await _repoStogare.DeleteSoftAsync(storage);
            return ApiResponseResult<object>(true, "Xóa thành công",null);
        }
    }
}
