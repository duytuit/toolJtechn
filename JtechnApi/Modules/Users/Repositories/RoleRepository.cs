using System;
using System.Linq;
using System.Threading.Tasks;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Users.Dtos;
using JtechnApi.Users.Models;

using Microsoft.EntityFrameworkCore;

using JtechnApi.Shares;
using static JtechnApi.Shares.RepositoryResult;

namespace JtechnApi.Users.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly DBJtecContext _context;

        public RoleRepository(DBJtecContext context)
        {
            _context = context;
        }

        public async Task<RepositoryResult> SynchronizePermissions()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var existing = (await _context.Permissions.Select(p => p.Code).ToListAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var added = new List<string>();
            foreach (var permission in JtechnApi.Configuration.SidebarMenu.RequiredPermissions())
            {
                if (!existing.Add(permission.Code)) continue;
                _context.Permissions.Add(new JtechnApi.Users.Models.Permission
                {
                    Code = permission.Code, Name = permission.Name, Category = permission.Category,
                    Description = permission.Name, IsActive = true, CreatedAt = DateTime.UtcNow
                });
                added.Add(permission.Code);
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { added = added.Count, codes = added });
        }

        public async Task<RepositoryResult> GetRoles()
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

        public async Task<RepositoryResult> GetRole(int id)
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

        public async Task<RepositoryResult> GetPermissions()
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

        public async Task<RepositoryResult> GetAssignableEmployees(string q = null)
        {
            if (q?.Length > 150) return BadRequest(new { message = "Từ khóa tối đa 150 ký tự." });
            var query = _context.Employee.AsNoTracking().Where(employee => employee.IsActive);
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(employee => employee.EmployeeCode.Contains(term) || employee.FullName.Contains(term));
            }
            if (q != null) query = query.OrderBy(employee => employee.EmployeeCode).Take(50);
            var employees = await query
                .OrderBy(employee => employee.EmployeeCode)
                .Select(employee => new
                {
                    employee.Id,
                    employee.EmployeeCode,
                    employee.FullName,
                    employee.IsActive,
                    Label = employee.EmployeeCode + " - " + employee.FullName
                })
                .ToListAsync();

            return Ok(employees);
        }

        public async Task<RepositoryResult> CreateRole(CreateRoleDto input)
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

        public async Task<RepositoryResult> UpdateRole(int id, UpdateRoleDto input)
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

        public async Task<RepositoryResult> DeleteRole(int id)
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

        public async Task<RepositoryResult> SetRolePermissions(
            int id,
            AssignRolePermissionsDto input)
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

        public async Task<RepositoryResult> GetEmployeeRoles(int employeeId)
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

        public async Task<RepositoryResult> SetEmployeeRoles(
            int employeeId,
            AssignEmployeeRolesDto input)
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

        public async Task<RepositoryResult> AddDepartmentRoles(int departmentId, AssignEmployeeRolesDto input)
        {
            if (input?.RoleCodes == null || input.RoleCodes.Count == 0 || input.RoleCodes.Any(string.IsNullOrWhiteSpace))
                return BadRequest(new { message = "Vui lòng chọn ít nhất một vai trò hợp lệ." });
            var codes = input.RoleCodes.Select(c => c.Trim().ToUpperInvariant()).Distinct().ToList();
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            if (!await _context.Departments.AnyAsync(d => d.Id == departmentId))
                return NotFound(new { message = "Bộ phận không tồn tại." });
            var roleIds = await _context.Roles.Where(r => r.IsActive && codes.Contains(r.Code)).Select(r => r.Id).ToListAsync();
            if (roleIds.Count != codes.Count)
                return BadRequest(new { message = "Vai trò không tồn tại hoặc đã ngừng hoạt động." });
            var employeeIds = await _context.Employee.Where(e => e.IsActive &&
                _context.EmployeeDepartments.Any(d => d.DepartmentId == departmentId && d.EmployeeId == e.Id))
                .Select(e => e.Id).ToListAsync();
            if (employeeIds.Count == 0)
                return Conflict(new { message = "Bộ phận chưa có nhân viên đang hoạt động." });
            var existing = await _context.RoleEmployees.Where(a => roleIds.Contains(a.RoleId) &&
                _context.Employee.Any(e => e.Id == a.EmployeeId && e.IsActive &&
                    _context.EmployeeDepartments.Any(d => d.DepartmentId == departmentId && d.EmployeeId == e.Id)))
                .Select(a => new { a.EmployeeId, a.RoleId }).ToListAsync();
            var assigned = existing.Select(a => (a.EmployeeId, a.RoleId)).ToHashSet();
            var added = 0;
            foreach (var employeeId in employeeIds)
                foreach (var roleId in roleIds)
                    if (assigned.Add((employeeId, roleId)))
                    {
                        _context.RoleEmployees.Add(new RoleEmployee { EmployeeId = employeeId, RoleId = roleId, CreatedAt = DateTime.UtcNow });
                        added++;
                    }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { departmentId, employeeCount = employeeIds.Count, addedAssignments = added });
        }

    }
}
