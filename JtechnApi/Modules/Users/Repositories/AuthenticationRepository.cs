using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JtechnApi.Users.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly DBJtecContext _employeeContext;
        public AuthenticationRepository(DBJtecContext context) => _employeeContext = context;
        public async Task<List<Claim>> GetLoginClaims(string username)
        {
            var normalizedUsername = username.Trim();
            var employee = await _employeeContext.Employee
                .AsNoTracking()
                .FirstOrDefaultAsync(item =>
                    item.Username == normalizedUsername && item.IsActive);

            if (employee == null) return null;
            var roles = await (
                from assignment in _employeeContext.RoleEmployees.AsNoTracking()
                join role in _employeeContext.Roles.AsNoTracking()
                    on assignment.RoleId equals role.Id
                where assignment.EmployeeId == employee.Id && role.IsActive
                select role.Code)
                .Distinct()
                .ToListAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, employee.Username),
                new Claim("FullName", employee.FullName ?? employee.Username)
            };
            claims.AddRange(roles.Select(roleCode => new Claim(ClaimTypes.Role, roleCode)));

            return claims;
        }
    }
}
