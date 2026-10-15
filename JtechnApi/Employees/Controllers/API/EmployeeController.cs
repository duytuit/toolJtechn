using System;
using System.Linq;
using System.Threading.Tasks;
using JtechnApi.Employees.Dtos;
using JtechnApi.Employees.Models;
using JtechnApi.Shares.BaseRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JtechnApi.Employees.Controllers.API
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeeController : ControllerBase
    {
        private readonly DBJtecContext _context;
        private readonly PasswordHasher<Employee> _passwordHasher = new PasswordHasher<Employee>();

        public EmployeeController(DBJtecContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Policy = "Permission:EMPLOYEES.VIEW")]
        public async Task<IActionResult> Get()
        {
            var entities = await _context.Employee
                .AsNoTracking()
                .OrderBy(employee => employee.Id)
                .ToListAsync();

            return Ok(entities.Select(ToDto).ToList());
        }

        [HttpGet("{id:int}")]
        [Authorize(Policy = "Permission:EMPLOYEES.VIEW")]
        public async Task<IActionResult> GetById(int id)
        {
            var employee = await _context.Employee
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            return employee == null ? NotFound() : Ok(ToDto(employee));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:EMPLOYEES.CREATE")]
        public async Task<IActionResult> Create([FromBody] EmployeeDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu nhân viên không hợp lệ." });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var validationError = ValidateInput(input);
            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

            var employeeCode = input.EmployeeCode.Trim();
            var username = TrimOrNull(input.Username) ?? employeeCode;
            if (await _context.Employee.AnyAsync(item =>
                    item.EmployeeCode == employeeCode || item.Username == username))
            {
                return Conflict(new { message = "Mã nhân viên hoặc tên đăng nhập đã tồn tại." });
            }

            var employee = new Employee
            {
                EmployeeCode = employeeCode,
                Username = username,
                FullName = input.FullName.Trim(),
                Email = TrimOrNull(input.Email),
                Phone = TrimOrNull(input.Phone),
                IsActive = input.IsActive,
                Department = TrimOrNull(input.Department),
                DateOfBirth = input.DateOfBirth,
                Gender = TrimOrNull(input.Gender),
                Position = TrimOrNull(input.Position),
                JoinDate = input.JoinDate,
                ProbationEndDate = input.ProbationEndDate,
                LeaveDate = input.LeaveDate,
                CreatedAt = DateTime.UtcNow
            };
            employee.PasswordHash = string.IsNullOrWhiteSpace(input.Password)
                ? "PASSWORDLESS_LOGIN_DISABLED_PASSWORD_AUTH"
                : _passwordHasher.HashPassword(employee, input.Password);

            _context.Employee.Add(employee);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = employee.Id }, ToDto(employee));
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "Permission:EMPLOYEES.UPDATE")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu nhân viên không hợp lệ." });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var validationError = ValidateInput(input);
            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

            var employee = await _context.Employee.FirstOrDefaultAsync(item => item.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);
            if (employee.IsActive && !input.IsActive &&
                await IsLastActiveAdministratorAsync(employee.Id))
            {
                return Conflict(new { message = "Không thể vô hiệu hóa quản trị viên đang hoạt động cuối cùng." });
            }

            var employeeCode = input.EmployeeCode.Trim();
            var username = TrimOrNull(input.Username);
            if (await _context.Employee.AnyAsync(item =>
                    item.Id != id &&
                    (item.EmployeeCode == employeeCode ||
                     (username != null && item.Username == username))))
            {
                return Conflict(new { message = "Mã nhân viên hoặc tên đăng nhập đã tồn tại." });
            }

            employee.EmployeeCode = employeeCode;
            if (input.Username != null)
            {
                employee.Username = username;
            }
            employee.FullName = input.FullName.Trim();
            if (input.Email != null)
            {
                employee.Email = TrimOrNull(input.Email);
            }
            employee.Phone = TrimOrNull(input.Phone);
            employee.IsActive = input.IsActive;
            employee.Department = TrimOrNull(input.Department);
            employee.DateOfBirth = input.DateOfBirth;
            employee.Gender = TrimOrNull(input.Gender);
            employee.Position = TrimOrNull(input.Position);
            employee.JoinDate = input.JoinDate;
            employee.ProbationEndDate = input.ProbationEndDate;
            employee.LeaveDate = input.LeaveDate;
            employee.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(input.Password))
            {
                employee.PasswordHash = _passwordHasher.HashPassword(employee, input.Password);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(ToDto(employee));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = "Permission:EMPLOYEES.DELETE")]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employee.FirstOrDefaultAsync(item => item.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);
            if (await IsLastActiveAdministratorAsync(employee.Id))
            {
                return Conflict(new { message = "Không thể xóa quản trị viên đang hoạt động cuối cùng." });
            }

            employee.DeletedAt = DateTime.UtcNow;
            employee.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return NoContent();
        }

        private async Task<bool> IsLastActiveAdministratorAsync(int employeeId)
        {
            var adminRoleId = await _context.Roles
                .Where(role => role.Code == "ADMIN" && role.IsActive)
                .Select(role => (int?)role.Id)
                .FirstOrDefaultAsync();
            if (!adminRoleId.HasValue ||
                !await _context.RoleEmployees.AnyAsync(assignment =>
                    assignment.RoleId == adminRoleId.Value &&
                    assignment.EmployeeId == employeeId))
            {
                return false;
            }

            var activeAdministratorCount = await _context.RoleEmployees
                .Where(assignment => assignment.RoleId == adminRoleId.Value)
                .Join(
                    _context.Employee.Where(item => item.IsActive),
                    assignment => assignment.EmployeeId,
                    employee => employee.Id,
                    (assignment, employee) => employee.Id)
                .Distinct()
                .CountAsync();

            return activeAdministratorCount <= 1;
        }

        private static string ValidateInput(EmployeeDto input)
        {
            if (string.IsNullOrWhiteSpace(input.EmployeeCode))
            {
                return "Vui lòng nhập mã nhân viên.";
            }

            if (string.IsNullOrWhiteSpace(input.FullName))
            {
                return "Vui lòng nhập họ và tên.";
            }

            if (!string.IsNullOrWhiteSpace(input.Password) && input.Password.Length < 8)
            {
                return "Mật khẩu phải có ít nhất 8 ký tự.";
            }

            return null;
        }

        private static string TrimOrNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static EmployeeResponseDto ToDto(Employee employee)
        {
            return new EmployeeResponseDto
            {
                Id = employee.Id,
                EmployeeCode = employee.EmployeeCode,
                Username = employee.Username,
                FullName = employee.FullName,
                Email = employee.Email,
                Phone = employee.Phone,
                IsActive = employee.IsActive,
                Department = employee.Department,
                DateOfBirth = employee.DateOfBirth,
                Gender = employee.Gender,
                Position = employee.Position,
                JoinDate = employee.JoinDate,
                ProbationEndDate = employee.ProbationEndDate,
                LeaveDate = employee.LeaveDate,
                CreatedAt = employee.CreatedAt,
                UpdatedAt = employee.UpdatedAt
            };
        }
    }
}
