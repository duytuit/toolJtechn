using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Controllers;
using Vudaco.Receipts.Dtos;
using Vudaco.Receipts.Models;
using Vudaco.Receipts.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Receipts.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptController:BaseApiController
    {
        private readonly IReceiptDetailRepositories _repoReceiptDetail;
        private readonly IReceiptRepositories _repoReceipt;
        private readonly ILogger<ReceiptController> _logger;
        private readonly VudacoDBContext _context;

        public int userId => (int)HttpContext.Items["UserId"];

        public ReceiptController(ILogger<ReceiptController> logger, IReceiptDetailRepositories repoReceiptDetail, IReceiptRepositories repoReceipt, VudacoDBContext context)
        {
            _logger = logger;
            _repoReceiptDetail = repoReceiptDetail;
            _repoReceipt = repoReceipt;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] ReceiptDto ReceiptDto = null)
        {
            // test
            var result = await _repoReceipt.GetObjectTaskAsync(ReceiptDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] ReceiptDto ReceiptDto)
        {
            // Check trùng Name
            var entity = await _context.Receipts.FirstOrDefaultAsync(p => p.CodeReceipt == ReceiptDto.CodeReceipt);
            if (entity != null)
                return ApiResponseResult<object>(false, "Tên dữ liệu đã tồn tại", null);
            // Check trùng Code
            entity = await _context.Receipts.FirstOrDefaultAsync(p => p.Code == ReceiptDto.Code);
            if (entity != null)
                return ApiResponseResult<object>(false, "code dữ liệu đã tồn tại", null);
                
            var Receipt = new Receipt
            {
                StorageId = ReceiptDto.StorageId,
                CreatedBy = userId,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
            };
            Receipt = await _repoReceipt.CreateAsync(Receipt);
            return ApiResponseResult(true, "Thêm thành công", Receipt);
        }
        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] ReceiptDto ReceiptDto)
        {
          
            return ApiResponseResult<object>(true, "Cập nhật thành công", null);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  ReceiptDto ReceiptDto)
        {
            if (ReceiptDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Receipts.Find(ReceiptDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedBy = userId;
            entity.DeletedAt = DateTime.Now;
            await _repoReceipt.DeleteSoftAsync(entity);
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await _repoReceipt.ShowAsync(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
    }
}
