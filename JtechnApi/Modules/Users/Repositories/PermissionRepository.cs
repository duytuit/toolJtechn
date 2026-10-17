using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;

namespace JtechnApi.Users.Repositories
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly DBJtecContext _context;
        public PermissionRepository(DBJtecContext context) => _context = context;

        public async Task<HashSet<string>> GetGrantedCodes(int employeeId)
        {
            if (!await _context.Employee.AnyAsync(e => e.Id == employeeId && e.IsActive)) return new(StringComparer.OrdinalIgnoreCase);
            var roles = from assignment in _context.RoleEmployees
                join role in _context.Roles on assignment.RoleId equals role.Id
                where assignment.EmployeeId == employeeId && role.IsActive select role;
            if (await roles.AnyAsync(r => r.Code == "ADMIN"))
                return JtechnApi.Configuration.SidebarMenu.Items.Select(m => m.ViewPermission).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var codes = await (from assignment in _context.RolePermissions
                join permission in _context.Permissions on assignment.PermissionId equals permission.Id
                where permission.IsActive && roles.Any(role => role.Id == assignment.RoleId)
                select permission.Code).Distinct().ToListAsync();
            return codes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        public async Task<bool> HasPermission(int employeeId, string permissionCode)
        {
            var activeEmployee = await _context.Employee
                .AsNoTracking()
                .AnyAsync(employee => employee.Id == employeeId && employee.IsActive);
            if (!activeEmployee)
            {
                return false;
            }

            var activeRoleIds = _context.RoleEmployees
                .Where(assignment => assignment.EmployeeId == employeeId)
                .Join(
                    _context.Roles.Where(role => role.IsActive),
                    assignment => assignment.RoleId,
                    role => role.Id,
                    (assignment, role) => new { role.Id, role.Code });

            var isSystemAdministrator = await activeRoleIds
                .AnyAsync(role => role.Code == "ADMIN");
            var hasPermission = isSystemAdministrator || await _context.RolePermissions
                .Where(assignment => activeRoleIds
                    .Select(role => role.Id)
                    .Contains(assignment.RoleId))
                .Join(
                    _context.Permissions.Where(permission =>
                        permission.IsActive && permission.Code == permissionCode),
                    assignment => assignment.PermissionId,
                    permission => permission.Id,
                    (assignment, permission) => assignment.Id)
                .AnyAsync();

            return isSystemAdministrator || hasPermission;
        }
    }
}
