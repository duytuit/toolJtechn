using System;
using System.Linq;
using System.Threading.Tasks;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Users.Dtos;
using JtechnApi.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JtechnApi.Users.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class RoleManagementController : ControllerBase
    {
        private readonly DBJtecContext _context;

        public RoleManagementController(DBJtecContext context)
        {
            _context = context;
        }

        [HttpGet("roles")]
        [Authorize(Policy = "Permission:ROLES.VIEW")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(role => role.Name)
                .Select(role => new
                {
                    role.Id,
                    role.Code,
                    role.Name,
                    role.Description,
                    role.IsSystem,
                    role.IsActive
                })
                .ToListAsync();

            var roleIds = roles.Select(role => role.Id).ToList();
            var permissionAssignments = await _context.RolePermissions
                .AsNoTracking()
                .Where(assignment => roleIds.Contains(assignment.RoleId))
                .Join(
                    _context.Permissions.AsNoTracking(),
                    assignment => assignment.PermissionId,
                    permission => permission.Id,
                    (assignment, permission) => new
                    {
                        assignment.RoleId,
                        permission.Code
                    })
                .ToListAsync();
            var permissionsByRole = permissionAssignments
                .GroupBy(assignment => assignment.RoleId)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(assignment => assignment.Code).OrderBy(code => code).ToList());

            return Ok(roles.Select(role => new
            {
                role.Id,
                role.Code,
                role.Name,
                role.Description,
                role.IsSystem,
                role.IsActive,
                PermissionCodes = permissionsByRole.TryGetValue(role.Id, out var codes)
                    ? codes
                    : new System.Collections.Generic.List<string>()
            }));
        }

        [HttpGet("roles/{id:int}")]
        [Authorize(Policy = "Permission:ROLES.VIEW")]
        public async Task<IActionResult> GetRole(int id)
        {
            var role = await _context.Roles
                .AsNoTracking()
                .Where(item => item.Id == id)
                .Select(item => new
                {
                    item.Id,
                    item.Code,
                    item.Name,
                    item.Description,
                    item.IsSystem,
                    item.IsActive
                })
                .FirstOrDefaultAsync();
            if (role == null)
            {
                return NotFound(new { message = "Không tìm thấy vai trò." });
            }

            var permissionCodes = await _context.RolePermissions
                .Where(assignment => assignment.RoleId == id)
                .Join(
                    _context.Permissions,
                    assignment => assignment.PermissionId,
                    permission => permission.Id,
                    (assignment, permission) => permission.Code)
                .OrderBy(code => code)
                .ToListAsync();

            return Ok(new
            {
                role.Id,
                role.Code,
                role.Name,
                role.Description,
                role.IsSystem,
                role.IsActive,
                PermissionCodes = permissionCodes
            });
        }

        [HttpGet("permissions")]
        [Authorize(Policy = "Permission:PERMISSIONS.VIEW")]
        public async Task<IActionResult> GetPermissions()
        {
            var permissions = await _context.Permissions
                .AsNoTracking()
                .Where(permission => permission.IsActive)
                .OrderBy(permission => permission.Category)
                .ThenBy(permission => permission.Code)
                .Select(permission => new
                {
                    permission.Id,
                    permission.Code,
                    permission.Name,
                    permission.Category,
                    permission.Description
                })
                .ToListAsync();

            return Ok(permissions);
        }

        [HttpGet("assignable-employees")]
        [Authorize(Policy = "Permission:EMPLOYEES.VIEW")]
        public async Task<IActionResult> GetAssignableEmployees()
        {
            var employees = await _context.Employee
                .AsNoTracking()
                .Where(employee => employee.IsActive)
                .OrderBy(employee => employee.EmployeeCode)
                .Select(employee => new
                {
                    employee.Id,
                    employee.EmployeeCode,
                    employee.FullName
                })
                .ToListAsync();

            return Ok(employees);
        }

        [HttpPost("roles")]
        [Authorize(Policy = "Permission:ROLES.CREATE")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu vai trò không hợp lệ." });
            }

            var code = input.Code.Trim().ToUpperInvariant();
            if (await _context.Roles.AnyAsync(role => role.Code == code))
            {
                return Conflict(new { message = "Mã vai trò đã tồn tại." });
            }

            var role = new Role
            {
                Code = code,
                Name = input.Name.Trim(),
                Description = TrimOrNull(input.Description),
                IsSystem = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, new
            {
                role.Id,
                role.Code,
                role.Name,
                role.Description,
                role.IsSystem,
                role.IsActive
            });
        }

        [HttpPut("roles/{id:int}")]
        [Authorize(Policy = "Permission:ROLES.UPDATE")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto input)
        {
            if (input == null)
            {
                return BadRequest(new { message = "Dữ liệu vai trò không hợp lệ." });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(item => item.Id == id);
            if (role == null)
            {
                return NotFound(new { message = "Không tìm thấy vai trò." });
            }

            if (role.IsSystem && input.IsActive == false)
            {
                return BadRequest(new { message = "Không thể vô hiệu hóa vai trò hệ thống." });
            }

            role.Name = input.Name.Trim();
            role.Description = TrimOrNull(input.Description);
            role.IsActive = input.IsActive.Value;
            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                role.Id,
                role.Code,
                role.Name,
                role.Description,
                role.IsSystem,
                role.IsActive,
                role.UpdatedAt
            });
        }

        [HttpDelete("roles/{id:int}")]
        [Authorize(Policy = "Permission:ROLES.DELETE")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(item => item.Id == id);
            if (role == null)
            {
                return NotFound(new { message = "Không tìm thấy vai trò." });
            }

            if (role.IsSystem)
            {
                return Conflict(new { message = "Không thể xóa vai trò hệ thống." });
            }

            if (await _context.RoleEmployees.AnyAsync(item => item.RoleId == id))
            {
                return Conflict(new { message = "Hãy gỡ vai trò khỏi nhân viên trước khi xóa." });
            }

            var permissions = await _context.RolePermissions
                .Where(item => item.RoleId == id)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(permissions);
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("roles/{id:int}/permissions")]
        [Authorize(Policy = "Permission:PERMISSIONS.ASSIGN")]
        public async Task<IActionResult> SetRolePermissions(
            int id,
            [FromBody] AssignRolePermissionsDto input)
        {
            if (input?.PermissionCodes == null)
            {
                return BadRequest(new { message = "Danh sách mã quyền không được để trống." });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(item => item.Id == id);
            if (role == null)
            {
                return NotFound(new { message = "Không tìm thấy vai trò." });
            }

            if (!role.IsActive)
            {
                return Conflict(new { message = "Không thể gán quyền cho vai trò đã bị vô hiệu hóa." });
            }

            if (input.PermissionCodes.Any(string.IsNullOrWhiteSpace))
            {
                return BadRequest(new { message = "Mã quyền trong danh sách không được để trống." });
            }

            var permissionCodes = input.PermissionCodes
                .Select(code => code.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();
            var permissions = await _context.Permissions
                .Where(permission => permission.IsActive && permissionCodes.Contains(permission.Code))
                .ToListAsync();

            if (permissions.Count != permissionCodes.Count)
            {
                return BadRequest(new { message = "Danh sách có mã quyền không tồn tại hoặc đã bị vô hiệu hóa." });
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var currentAssignments = await _context.RolePermissions
                .Where(assignment => assignment.RoleId == id)
                .ToListAsync();
            _context.RolePermissions.RemoveRange(currentAssignments);
            _context.RolePermissions.AddRange(permissions.Select(permission => new RolePermission
            {
                RoleId = id,
                PermissionId = permission.Id,
                CreatedAt = DateTime.UtcNow
            }));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { roleId = id, permissionCodes });
        }

        [HttpGet("employees/{employeeId:int}/roles")]
        [Authorize(Policy = "Permission:ROLES.VIEW")]
        public async Task<IActionResult> GetEmployeeRoles(int employeeId)
        {
            if (!await _context.Employee.AnyAsync(employee => employee.Id == employeeId))
            {
                return NotFound(new { message = "Không tìm thấy nhân viên." });
            }

            var roles = await _context.RoleEmployees
                .Where(assignment => assignment.EmployeeId == employeeId)
                .Join(
                    _context.Roles.Where(role => role.IsActive),
                    assignment => assignment.RoleId,
                    role => role.Id,
                    (assignment, role) => new
                    {
                        role.Id,
                        role.Code,
                        role.Name
                    })
                .OrderBy(role => role.Name)
                .ToListAsync();

            return Ok(roles);
        }

        [HttpPut("employees/{employeeId:int}/roles")]
        [Authorize(Policy = "Permission:PERMISSIONS.ASSIGN")]
        public async Task<IActionResult> SetEmployeeRoles(
            int employeeId,
            [FromBody] AssignEmployeeRolesDto input)
        {
            if (input?.RoleCodes == null)
            {
                return BadRequest(new { message = "Danh sách mã vai trò không được để trống." });
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(item => item.Id == employeeId);
            if (employee == null)
            {
                return NotFound(new { message = "Không tìm thấy nhân viên." });
            }
            if (!employee.IsActive)
            {
                return Conflict(new { message = "Không thể gán vai trò cho nhân viên đã ngừng hoạt động." });
            }

            if (input.RoleCodes.Any(string.IsNullOrWhiteSpace))
            {
                return BadRequest(new { message = "Mã vai trò trong danh sách không được để trống." });
            }

            var roleCodes = input.RoleCodes
                .Select(code => code.Trim().ToUpperInvariant())
                .Distinct()
                .ToList();
            var roles = await _context.Roles
                .Where(role => role.IsActive && roleCodes.Contains(role.Code))
                .ToListAsync();

            if (roles.Count != roleCodes.Count)
            {
                return BadRequest(new { message = "Danh sách có mã vai trò không tồn tại hoặc đã bị vô hiệu hóa." });
            }

            var adminRole = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(role => role.Code == "ADMIN");
            await using var transaction = await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);
            var currentAssignments = await _context.RoleEmployees
                .Where(assignment => assignment.EmployeeId == employeeId)
                .ToListAsync();
            var currentlyHasAdmin = adminRole != null &&
                currentAssignments.Any(assignment => assignment.RoleId == adminRole.Id);
            var willHaveAdmin = adminRole != null &&
                roles.Any(role => role.Id == adminRole.Id);

            if (employee.IsActive && currentlyHasAdmin && !willHaveAdmin)
            {
                var activeAdminCount = await _context.RoleEmployees
                    .Where(assignment => assignment.RoleId == adminRole.Id)
                    .Join(
                        _context.Employee.Where(item => item.IsActive),
                        assignment => assignment.EmployeeId,
                        activeEmployee => activeEmployee.Id,
                        (assignment, activeEmployee) => activeEmployee.Id)
                    .Distinct()
                    .CountAsync();

                if (activeAdminCount <= 1)
                {
                    return Conflict(new { message = "Không thể gỡ quyền ADMIN khỏi nhân viên quản trị viên cuối cùng." });
                }
            }

            _context.RoleEmployees.RemoveRange(currentAssignments);
            _context.RoleEmployees.AddRange(roles.Select(role => new RoleEmployee
            {
                RoleId = role.Id,
                EmployeeId = employeeId,
                CreatedAt = DateTime.UtcNow
            }));
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                employeeId,
                roleCodes
            });
        }

        private static string TrimOrNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
