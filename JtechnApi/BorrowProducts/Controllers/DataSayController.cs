
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
using Microsoft.EntityFrameworkCore;

namespace JtechnApi.BorrowProducts
{
    [ApiController]
    [Route("[controller]")]
    public class DataSayController : BaseApiController
    {

        private readonly ConnectionStrings con;
        private readonly IDataSayRepository repo;
        private readonly ILogger<DataSayController> _logger;
         private readonly OracleConnection _oracle;
        private readonly DBContext _context;

        public DataSayController(ILogger<DataSayController> logger, DBContext context, ConnectionStrings c, IDataSayRepository r, OracleConnection oracle)
        {
            _logger = logger;
            con = c;
            repo = r;
            _oracle = oracle;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken, [FromQuery] int page = 1, int pageSize = 50, [FromQuery] DataSayDto DataSayDto = null)
        {

            var result = await repo.GetPaginatedAsync(DataSayDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody] DataSayDto DataSayDto)
        {
            var now = DateTime.Now;

            var DataSay = new DataSay
            {
                Lot = DataSayDto.Lot,
                Code = DataSayDto.Code,
                Content= DataSayDto.Content,
                Type = 2,
                Date = DataSayDto.Date,
                Note= DataSayDto.Note,
                UserBy= DataSayDto.UserBy,
                CreatedAt = now,
                UpdatedAt = now
            };
            await _context.DataSay.AddAsync(DataSay);
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Thêm thành công", DataSay);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DataSayDto DataSayDto)
        {
            if (DataSayDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _context.DataSay.FirstOrDefaultAsync(x=>x.Id == DataSayDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            entity.DeletedAt = DateTime.Now;
            _context.DataSay.Update(entity);
            _context.SaveChanges();
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
        [HttpPost("changeStatus")]
        public async Task<IActionResult> ChangeStatus([FromBody] DataSayDto DataSayDto)
        {
            if (DataSayDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = await _context.DataSay.FindAsync(DataSayDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
         
          
            _context.DataSay.Update(entity);
            _context.SaveChanges();
            return ApiResponseResult<object>(true, "thay đổi trạng thái thành công", null);
        }
    }
}
