using Microsoft.AspNetCore.Identity;
using static JtechnApi.Shares.RepositoryResult;
using JtechnApi.Employees.Dtos;
using JtechnApi.Employees.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace JtechnApi.Employees.Repositories
{
    public class EmployeeRepository : JtecBaseRepository<Employee>, IEmployeeRepository
    {
        private readonly DBJtecContext _context;
        private readonly PasswordHasher<Employee> _passwordHasher = new PasswordHasher<Employee>();
        public EmployeeRepository(DBJtecContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Employee>> GetPaginatedAsync(int page, int pageSize)
        {
            var totalItems = await _context.Employee.CountAsync();

            var items = await _context.Employee
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Employee>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = items
            };
        }

        public Task<Employee> GetByCode(string code)
        {
            return _context.Employee
                .Where(e => e.EmployeeCode == code)
                .Select(e => new Employee
                {
                    Id = e.Id,
                    EmployeeCode = e.EmployeeCode,
                    FullName = e.FullName,
                })
                .FirstOrDefaultAsync();
        }
        public async Task<RepositoryResult> Get()
        {
            var entities = await _context.Employee
                .AsNoTracking()
                .OrderBy(employee => employee.Id)
                .ToListAsync();

            return Ok(entities.Select(ToDto).ToList());
        }

        public async Task<RepositoryResult> GetById(int id)
        {
            var employee = await _context.Employee
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            return employee == null ? NotFound() : Ok(ToDto(employee));
        }

        public async Task<RepositoryResult> Create(EmployeeDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu nhân viên không hợp lệ." });
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

        public async Task<RepositoryResult> Update(int id, EmployeeDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu nhân viên không hợp lệ." });
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

        public async Task<RepositoryResult> Delete(int id)
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
