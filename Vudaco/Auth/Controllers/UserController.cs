
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vudaco.Auth.Dtos;
using Vudaco.Auth.Repositories;
using Vudaco.Controllers;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.Connects;

namespace Vudaco.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BaseApiController
    {
        private readonly IUserRepository repo;
        private readonly ILogger<UserController> _logger;
        private readonly VudacoDBContext _context;

        public UserController(ILogger<UserController> logger, IUserRepository r, VudacoDBContext context)
        {
            _logger = logger;
            repo = r;
            _context = context;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] UserDto UserDto = null )
        {
            // test
            var result = await repo.GetObjectTaskAsync(UserDto, page, pageSize, cancellationToken);
                if (result == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
       
        [HttpGet("task/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var required = await repo.detail(id);
            if (required == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", required);
        }
        [HttpDelete("{id}")]
        [Route("task/delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            var isDeleted = await repo.DeleteRequiredAsync(id);
            if (!isDeleted)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult<object>(true, "Xóa thành công", null);
        }
    }
}
