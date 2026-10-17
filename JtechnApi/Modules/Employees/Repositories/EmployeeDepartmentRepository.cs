using JtechnApi.Employees.Dtos;
using JtechnApi.Employees.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using static JtechnApi.Shares.RepositoryResult;

namespace JtechnApi.Employees.Repositories
{
    public class EmployeeDepartmentRepository : IEmployeeDepartmentRepository
    {
        private readonly DBJtecContext context;
        public EmployeeDepartmentRepository(DBJtecContext context) => this.context = context;

        public async Task<object> SearchEmployees(string q)
        {
            var query = context.Employee.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(e => e.EmployeeCode.Contains(term) || e.FullName.Contains(term));
            }
            return await query.OrderBy(e => e.EmployeeCode).Take(50)
                .Select(e => new { e.Id, Label = e.EmployeeCode + " - " + e.FullName + (e.IsActive ? "" : " (Đã ngừng hoạt động)") }).ToListAsync();
        }

        public async Task<RepositoryResult> Remove(int departmentId, int employeeId)
        {
            if (!await context.Departments.AnyAsync(d => d.Id == departmentId))
                return NotFound(new { message = "Bộ phận không còn tồn tại." });
            var assignment = await context.EmployeeDepartments.FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.DepartmentId == departmentId);
            if (assignment == null) return NotFound(new { message = "Nhân viên không còn thuộc bộ phận này." });
            assignment.DeletedAt = DateTime.UtcNow;
            assignment.UpdatedAt = assignment.DeletedAt;
            await context.SaveChangesAsync();
            return NoContent();
        }

        public async Task<RepositoryResult> Synchronize()
        {
            await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var departments = await context.Departments.AsNoTracking().ToListAsync();
            var candidates = departments.Select(d => (d.Id, d.Name, d.Code)).ToArray();
            var employees = await context.Employee.AsNoTracking().Select(e => new { e.Id, e.Department }).ToListAsync();
            var assignments = await context.EmployeeDepartments.IgnoreQueryFilters().ToListAsync();
            var existing = assignments.ToDictionary(a => (a.EmployeeId, a.DepartmentId));
            var skipped = new Dictionary<string, (int Count, string Reason)>();
            int added = 0, restored = 0, unchanged = 0, empty = 0;
            var now = DateTime.UtcNow;
            foreach (var employee in employees)
            {
                var key = DepartmentNameMatcher.Normalize(employee.Department);
                if (key.Length == 0) { empty++; continue; }
                var ids = DepartmentNameMatcher.Match(employee.Department, candidates);
                if (ids.Length != 1)
                {
                    var source = employee.Department.Trim();
                    var reason = ids.Length == 0 ? "Không tìm thấy bộ phận" : "Tên/mã khớp nhiều bộ phận";
                    skipped.TryGetValue(source, out var previous);
                    skipped[source] = (previous.Count + 1, reason);
                    continue;
                }
                var pair = (employee.Id, ids[0]);
                if (existing.TryGetValue(pair, out var assignment))
                {
                    if (assignment.DeletedAt == null) { unchanged++; continue; }
                    assignment.DeletedAt = null; assignment.UpdatedAt = now; restored++;
                }
                else
                {
                    context.EmployeeDepartments.Add(new EmployeeDepartment { EmployeeId = employee.Id, DepartmentId = ids[0], CreatedAt = now });
                    added++;
                }
            }
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { added, restored, unchanged, empty,
                unmatched = skipped.Select(item => new { department = item.Key, count = item.Value.Count, reason = item.Value.Reason }).ToList() });
        }

        public async Task<RepositoryResult> GetDepartmentEmployees(int departmentId)
        {
            var department = await context.Departments.AsNoTracking().Where(d => d.Id == departmentId)
                .Select(d => new { d.Id, d.Code, d.Name }).FirstOrDefaultAsync();
            if (department == null) return NotFound(new { message = "Bộ phận không còn tồn tại." });
            var employees = await (from assignment in context.EmployeeDepartments.AsNoTracking()
                join employee in context.Employee.AsNoTracking() on assignment.EmployeeId equals employee.Id
                where assignment.DepartmentId == departmentId && employee.IsActive
                orderby employee.EmployeeCode
                select new { employee.Id, employee.EmployeeCode, employee.FullName, employee.Email, employee.Phone,
                    Status = employee.IsActive ? "Đang làm việc" : "Ngừng hoạt động" }).ToListAsync();
            return Ok(new { department, employees });
        }

        public async Task<object> GetOptions() => new
        {
            Employees = await context.Employee.AsNoTracking().OrderBy(e => e.EmployeeCode)
                .Select(e => new { e.Id, e.EmployeeCode, e.FullName, e.IsActive }).ToListAsync(),
            Departments = await context.Departments.AsNoTracking().OrderBy(d => d.Code)
                .Select(d => new { d.Id, d.Code, d.Name }).ToListAsync()
        };

        public async Task<RepositoryResult> Get(int employeeId)
        {
            if (!await context.Employee.AnyAsync(e => e.Id == employeeId))
                return NotFound(new { message = "Nhân viên không còn tồn tại." });
            var departments = await (from assignment in context.EmployeeDepartments.AsNoTracking()
                join department in context.Departments.AsNoTracking() on assignment.DepartmentId equals department.Id
                where assignment.EmployeeId == employeeId
                orderby department.Code
                select new { department.Id, department.Code, department.Name }).ToListAsync();
            return Ok(new { employeeId, departments });
        }

        public async Task<RepositoryResult> Save(int employeeId, EmployeeDepartmentInput input)
        {
            if (input?.DepartmentIds == null || input.DepartmentIds.Length > 1000 || input.DepartmentIds.Any(id => id <= 0))
                return BadRequest(new { message = "Danh sách bộ phận không hợp lệ." });
            var ids = input.DepartmentIds.Distinct().ToArray();
            await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            var employee = await context.Employee.FirstOrDefaultAsync(e => e.Id == employeeId);
            if (employee == null) return NotFound(new { message = "Nhân viên không còn tồn tại." });
            if (!employee.IsActive) return Conflict(new { message = "Không thể gán bộ phận cho nhân viên đã ngừng hoạt động." });
            if (await context.Departments.CountAsync(d => ids.Contains(d.Id)) != ids.Length)
                return BadRequest(new { message = "Có bộ phận không tồn tại hoặc đã bị xóa. Hãy tải lại trang." });

            var assignments = await context.EmployeeDepartments.IgnoreQueryFilters()
                .Where(a => a.EmployeeId == employeeId).ToListAsync();
            var now = DateTime.UtcNow;
            foreach (var assignment in assignments)
            {
                var selected = ids.Contains(assignment.DepartmentId);
                if (selected && assignment.DeletedAt != null) { assignment.DeletedAt = null; assignment.UpdatedAt = now; }
                else if (!selected && assignment.DeletedAt == null) { assignment.DeletedAt = now; assignment.UpdatedAt = now; }
            }
            var existingIds = assignments.Select(a => a.DepartmentId).ToHashSet();
            foreach (var id in ids.Where(id => !existingIds.Contains(id)))
                context.EmployeeDepartments.Add(new EmployeeDepartment { EmployeeId = employeeId, DepartmentId = id, CreatedAt = now });
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { employeeId, departmentIds = ids });
        }
    }
}
