
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vudaco.Auth.Models;
using Vudaco.ContractFiles.Dtos;
using Vudaco.Controllers;
using Vudaco.Employees.Dtos;
using Vudaco.Employees.Models;
using Vudaco.Employees.Repositories;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Employees.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : BaseApiController
    {
        private readonly IEmployeeRepository _repoEmployee;
        private readonly IEmployeeDepartmentRepository _repoEmployeeDepartmentRepository;
        private readonly ILogger<EmployeeController> _logger;
        private readonly VudacoDBContext _context;
        public int userId => (int)HttpContext.Items["UserId"];
        private readonly IConfiguration _configuration;

        public EmployeeController(ILogger<EmployeeController> logger, IConfiguration configuration,IEmployeeDepartmentRepository repoEmployeeDepartmentRepository,IEmployeeRepository repoEmployee, VudacoDBContext context)
        {
            _logger = logger;
            _repoEmployee = repoEmployee;
            _repoEmployeeDepartmentRepository = repoEmployeeDepartmentRepository;
            _context = context;
            _configuration = configuration;
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
        [HttpGet("InfoEmployee")]
        public async Task<IActionResult> InfoEmployee(CancellationToken cancellationToken)
        {
            var result = await _repoEmployee.InfoEmployeeAsync(userId);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("drivers")]
        public async Task<IActionResult> DriversEmployee(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] EmployeeDto EmployeeDto = null )
        {
            var result = await _context.Employees
                    .Where(x => x.StorageId == EmployeeDto.StorageId)
                    .Join(
                        _context.EmployeeDepartments.Where(ed => ed.DepartmentId == 1),
                        pd => pd.Id,
                        p => p.EmployeeId,
                        (pd, p) => pd
                    ).ToListAsync();
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
            if (string.IsNullOrWhiteSpace(dto.StorageId.ToString()))
            {
                return ApiResponseResult<object>(false, "Vui lòng chọn kho làm việc", null);
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Phone);

            var employee = await _context.Employees.FirstOrDefaultAsync(u => u.StorageId == dto.StorageId && u.Phone == dto.Phone);

            if (employee != null) return ApiResponseResult<object>(false, "Số điện thoại nguoi dung đã tồn tại", null);   

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                // ====== CREATE USER ======
                if (user == null)
                {
                    user = new User
                    {
                        Username = dto.Phone,
                        Password = !string.IsNullOrWhiteSpace(dto.Password) ? BCrypt.Net.BCrypt.HashPassword(dto.Password) : BCrypt.Net.BCrypt.HashPassword(dto.Phone),
                        Email = dto.Email,
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = userId
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync(); // cần để lấy user.Id
                }

                // ====== CREATE EMPLOYEE ======
                if (employee == null)
                {
                    employee = new Employee
                    {
                        Code = SqlServerHelpers.GenerateSoChungTu(_configuration.GetConnectionString("DefaultConnection"), "employees", "code", dto.StorageId, "NV", 4),
                        FirstName = dto.FirstName,
                        LastName = dto.LastName,
                        StorageId = dto.StorageId,
                        CreatedBy = userId,
                        BeginDateCompany = DateTime.Now,
                        BaseSalary = dto.BaseSalary,
                        Phone = dto.Phone,
                        Email = dto.Email,
                        UserId = user.Id,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = userId
                    };
                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();
                }
              
                if (dto.departmentIds != null && dto.departmentIds.Length > 0)
                {
                    foreach (var deptId in dto.departmentIds)
                    {
                        var employeeDepartment = await _context.EmployeeDepartments.FirstOrDefaultAsync(u => u.StorageId == dto.StorageId && u.EmployeeId == employee.Id && u.DepartmentId == deptId);
                        if (employeeDepartment == null)
                        {
                            employeeDepartment = new EmployeeDepartment
                            {
                                EmployeeId = employee.Id,
                                DepartmentId = deptId,
                                StorageId = employee.StorageId,
                            };
                            _context.EmployeeDepartments.Add(employeeDepartment);
                        }
                    }
                }
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult(true, "Thêm thành công", employee);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException.Message, null);
            }
        }
        [HttpPost("update")]
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
                employee.BaseSalary= dto.BaseSalary;
                employee.Phone     = dto.Phone;
                employee.Email     = dto.Email;
                employee.UpdatedAt = DateTime.Now;
                employee.UpdatedBy = userId;

                if (dto.departmentIds != null && dto.departmentIds.Length > 0)
                {
                    var check_departments = await _context.EmployeeDepartments
                        .Where(ed => ed.EmployeeId == employee.Id)
                        .Where(ed => dto.departmentIds.Contains(ed.DepartmentId))
                        .ToListAsync();
                    if (check_departments.Count != dto.departmentIds.Length)
                    {
                        // Xóa các phòng ban hiện tại của nhân viên
                        var existingDepartments = await _context.EmployeeDepartments.Where(ed => ed.EmployeeId == employee.Id).ToListAsync();
                        foreach (var dept in existingDepartments)
                        {
                            dept.DeletedAt = DateTime.Now;
                            dept.DeletedBy = userId;
                            _context.EmployeeDepartments.Update(dept);    
                        }

                        // Thêm các phòng ban mới
                        foreach (var deptId in dto.departmentIds)
                        {
                            var employeeDepartment = new EmployeeDepartment
                            {
                                EmployeeId = employee.Id,
                                DepartmentId = deptId,
                                StorageId = employee.StorageId,
                            };
                            _context.EmployeeDepartments.Add(employeeDepartment);
                        }
                    }
                }
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
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] EmployeeChangePasswordDto dto)
        {
            
            if (dto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var employee = await _context.Employees.FindAsync(dto.Id);
            if (employee == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            var user = await _context.Users.FindAsync(employee.UserId);
            if (user == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu user", null);
            }
            if (dto.NewPassword != dto.ConfirmPassword)
            {
                return ApiResponseResult<object>(false, "Mật khẩu xác nhận không khớp", null);  
            }
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return ApiResponseResult<object>(true, "Đổi mật khẩu thành công", null);    
        }
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] EmployeeDto dto)
        {
            if (dto.Id <= 0)
                return ApiResponseResult<object>(false, "Id không tồn tại", null);

            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                // Lấy employee
                var emp = await _context.Employees.FindAsync(dto.Id);
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
                return ApiResponseResult<object>(true, "Xóa thành công", null);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi xóa: " + ex.Message, null);
            }
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var storage =  await _repoEmployee.ShowAsync(id);
            if (storage == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", storage);
        }
    }
}
