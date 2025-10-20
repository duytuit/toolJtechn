
using System;
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
         [HttpPost]
         [Route("create")]
         public async Task<IActionResult> Create([FromBody] EmployeeDto EmployeeDto)
        {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // ✅ Check username trùng
                    if (await _context.Users.AnyAsync(u => u.Username == EmployeeDto.Username))
                        return ApiResponseResult<object>(false, "Tên đăng nhập đã tồn tại", null);

                    // ✅ Tạo user
                    var user = new User
                    {
                        Username = EmployeeDto.Username,
                        Password = BCrypt.Net.BCrypt.HashPassword(EmployeeDto.Password),
                        Email = EmployeeDto.Email,
                        FirstName = EmployeeDto.FirstName,
                        LastName = EmployeeDto.LastName,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = userId
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync(); // cần để lấy user.Id

                    // ✅ Tạo employee
                    var employee = new Employee
                    {
                        Code = EmployeeDto.Code,
                        FirstName = EmployeeDto.FirstName,
                        LastName = EmployeeDto.LastName,
                        StorageId = EmployeeDto.StorageId,
                        CreatedBy = userId,
                        Phone = EmployeeDto.Phone,
                        Email = EmployeeDto.Email,
                        UserId = user.Id
                    };

                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync(); // ✅ Commit
                    return ApiResponseResult(true, "Thêm thành công", employee);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); // ✅ Rollback khi lỗi
                    return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.Message, null);
                }
         }
         [HttpPut]
         [Route("update")]
         public async Task<IActionResult> Update([FromBody] EmployeeDto EmployeeDto)
         {
            if (EmployeeDto.Id <= 0)
                return ApiResponseResult<object>(false, "Id không hợp lệ", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy employee theo Id
                var employee = await _context.Employees
                    .AsTracking()
                    .FirstOrDefaultAsync(e => e.Id == EmployeeDto.Id);

                if (employee == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy nhân viên", null);

                // Lấy user theo userId của employee
                var user = await _context.Users
                    .AsTracking()
                    .FirstOrDefaultAsync(u => u.Id == employee.UserId);

                if (user == null)
                    return ApiResponseResult<object>(false, "Không tìm thấy user của nhân viên", null);

            
                // ✅ Chỉ cập nhật password nếu có nhập
                if (!string.IsNullOrWhiteSpace(EmployeeDto.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(EmployeeDto.Password);
                }
                user.FirstName = EmployeeDto.FirstName;
                user.LastName  = EmployeeDto.LastName;
                user.Email     = EmployeeDto.Email;
                user.UpdatedAt = DateTime.Now;
                user.UpdatedBy = userId;

                // ====== CẬP NHẬT EMPLOYEE ======
                employee.Code       = EmployeeDto.Code;
                employee.FirstName  = EmployeeDto.FirstName;
                employee.LastName   = EmployeeDto.LastName;
                employee.StorageId  = EmployeeDto.StorageId;
                employee.Phone      = EmployeeDto.Phone;
                employee.Email      = EmployeeDto.Email;
                employee.UpdatedAt  = DateTime.Now;
                employee.UpdatedBy  = userId;

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

                // Xóa mềm user nếu có
                if (emp.UserId.HasValue)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == emp.UserId.Value);
                    if (user != null)
                    {
                        user.DeletedAt = DateTime.Now;
                        user.UpdatedBy = userId;
                        _context.Users.Update(user);
                        await _context.SaveChangesAsync();
                    }
                }

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
