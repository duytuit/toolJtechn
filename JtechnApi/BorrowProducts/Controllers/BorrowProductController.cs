
using JtechnApi.ProductionPlans.Dtos;
using JtechnApi.ProductionPlans.Repositories;
using JtechnApi.Shares.Connects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System;
using System.Linq;
using JtechnApi.Controllers;
using JtechnApi.BorrowProducts.Dtos;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.BorrowProducts.Models;
using JtechnApi.BorrowProducts.Repositories;

namespace JtechnApi.BorrowProducts
{
    [ApiController]
    [Route("[controller]")]
    public class BorrowProductController : BaseApiController
    {

        private readonly ConnectionStrings con;
        private readonly IBorrowProductRepository repo;
        private readonly ILogger<BorrowProductController> _logger;
         private readonly OracleConnection _oracle;
        private readonly DBContext _context;

        public BorrowProductController(ILogger<BorrowProductController> logger, DBContext context, ConnectionStrings c, IBorrowProductRepository r, OracleConnection oracle)
        {
            _logger = logger;
            con = c;
            repo = r;
            _oracle = oracle;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] BorrowProductDto BorrowProductDto = null)
        {

            var result = await repo.GetPaginatedAsync(BorrowProductDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] BorrowProductDto BorrowProductDto)
        {
            var now = DateTime.Now;

            var BorrowProduct = new BorrowProduct
            {
                Code = BorrowProductDto.Code,
                Quantity = BorrowProductDto.Quantity,
                Note = BorrowProductDto.Note,
                CreatedBy = BorrowProductDto.UserId,
                Status = BorrowProductDto.Status,
                CreatedAt = now,
            };
            await _context.BorrowProduct.AddAsync(BorrowProduct);
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Thêm thành công", BorrowProduct);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] BorrowProductDto BorrowProductDto)
        {
            if (BorrowProductDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _context.BorrowProduct.FindAsync(BorrowProductDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.UpdatedBy = BorrowProductDto.UserId;
            entity.DeletedAt = DateTime.Now;
            _context.BorrowProduct.Update(entity);
            _context.SaveChanges();
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpPost("changeStatus")]
        public async Task<IActionResult> ChangeStatus([FromBody] BorrowProductDto BorrowProductDto)
        {
            if (BorrowProductDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _context.BorrowProduct.FindAsync(BorrowProductDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            if (BorrowProductDto.Status == 0)
            {
                entity.Status = BorrowProductDto.Status;
                entity.UpdatedBy = null;
                entity.UpdatedAt = null;
            }
            else
            {
                entity.Status = BorrowProductDto.Status;
                entity.UpdatedBy = BorrowProductDto.UserId;
                entity.UpdatedAt = DateTime.Now;
            }
          
            _context.BorrowProduct.Update(entity);
            _context.SaveChanges();
            return ApiResponseResult<object>(true, "thay đổi trạng thái thành công", null);
        }
    }
}
