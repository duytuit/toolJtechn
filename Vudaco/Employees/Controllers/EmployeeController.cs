
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vudaco.Auth.Models;
using Vudaco.ContractFiles.Dtos;
using Vudaco.Controllers;
using Vudaco.Employees.Dtos;
using Vudaco.Employees.Models;
using Vudaco.Employees.Repositories;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Employees.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : BaseApiController
    {
        private readonly IEmployeeRepository _repoEmployee;
        private readonly ILogger<EmployeeController> _logger;
        private readonly VudacoDBContext _context;
        public int userId => (int)HttpContext.Items["UserId"];

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeRepository repoEmployee, VudacoDBContext context)
        {
            _logger = logger;
            _repoEmployee = repoEmployee;
            _context = context;
        }

        /// <summary>
        /// List users
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] EmployeeDto EmployeeDto = null )
        {
            // test
            var result = await _repoEmployee.GetObjectTaskAsync(EmployeeDto, page, pageSize, cancellationToken);
                if (result == null)
                {
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
                }
                return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] EmployeeDto dto)
        {
            // ====== VALIDATE ======
            if (await _context.Users.AnyAsync(u => u.Username == dto.Phone))
                return ApiResponseResult<object>(false, "Số điện thoại đã được dùng làm username", null);

            if (await _context.Employees.AnyAsync(e => e.Phone == dto.Phone))
                return ApiResponseResult<object>(false, "Số điện thoại đã tồn tại", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                // ====== CREATE USER ======
                var user = new User
                {
                    Username  = dto.Phone,
                    Password  = BCrypt.Net.BCrypt.HashPassword(dto.Phone),
                    Email     = dto.Email,
                    FirstName = dto.FirstName,
                    LastName  = dto.LastName,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = userId
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // cần để lấy user.Id

                // ====== CREATE EMPLOYEE ======
                var employee = new Employee
                {
                    Code      = dto.Code,
                    FirstName = dto.FirstName,
                    LastName  = dto.LastName,
                    StorageId = dto.StorageId,
                    CreatedBy = userId,
                    Phone     = dto.Phone,
                    Email     = dto.Email,
                    UserId    = user.Id,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = userId
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                await tran.CommitAsync();
                return ApiResponseResult(true, "Thêm thành công", employee);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.Message, null);
            }
        }
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] EmployeeDto dto)
        {
            if (dto.Id <= 0)
                return ApiResponseResult<object>(false, "Id không hợp lệ", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                var employee = await _context.Employees.AsTracking()
                    .FirstOrDefaultAsync(e => e.Id == dto.Id);
                if (employee == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy nhân viên", null);

                var user = await _context.Users.AsTracking()
                    .FirstOrDefaultAsync(u => u.Id == employee.UserId);
                if (user == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy user của nhân viên", null);


                // ✅ Check trùng phone/username (trừ bản thân nó)
                if (await _context.Users.AnyAsync(u => u.Username == dto.Phone && u.Id != user.Id))
                    return ApiResponseResult<object>(false, "Số điện thoại đã được dùng làm username của user khác", null);

                if (await _context.Employees.AnyAsync(e => e.Phone == dto.Phone && e.Id != employee.Id))
                    return ApiResponseResult<object>(false, "Số điện thoại đã tồn tại ở nhân viên khác", null);


                // ====== UPDATE USER ======
                if (!string.IsNullOrWhiteSpace(dto.Password))
                    user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                user.Username  = dto.Phone; // nếu bạn dùng phone làm username
                user.FirstName = dto.FirstName;
                user.LastName  = dto.LastName;
                user.Email     = dto.Email;
                user.UpdatedAt = DateTime.Now;
                user.UpdatedBy = userId;


                // ====== UPDATE EMPLOYEE ======
                employee.Code      = dto.Code;
                employee.FirstName = dto.FirstName;
                employee.LastName  = dto.LastName;
                employee.StorageId = dto.StorageId;
                employee.Phone     = dto.Phone;
                employee.Email     = dto.Email;
                employee.UpdatedAt = DateTime.Now;
                employee.UpdatedBy = userId;


                await _context.SaveChangesAsync();
                await tran.CommitAsync();

                return ApiResponseResult(true, "Cập nhật thành công", employee);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi: " + ex.Message, null);
            }
        }
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery] int id)
        {
            if (id <= 0)
                return ApiResponseResult<object>(false, "Id không tồn tại", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy employee
                var emp = await _context.Employees.FindAsync(id);
                if (emp == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);

                // Xóa mềm employee thông qua repo
                emp.DeletedAt = DateTime.Now;
                emp.DeletedBy = userId;
                _context.Employees.Update(emp);

               var countUseUser = await _context.Employees
                    .Where(p => p.UserId == emp.UserId && p.Id != emp.Id)
                    .CountAsync();

                if (countUseUser == 0)
                {
                    var user = await _context.Users
                        .AsTracking()
                        .FirstOrDefaultAsync(u => u.Id == emp.UserId);
                    if (user != null)
                    {
                        user.DeletedAt = DateTime.Now;
                        user.UpdatedAt = DateTime.Now;
                        user.UpdatedBy = userId;
                    }
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult<object>(true, "Xóa mềm thành công", null);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi xóa: " + ex.Message, null);
            }
        }
    }
}
