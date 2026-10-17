using JtechnApi.Departments.Dtos;
using Microsoft.Data.SqlClient;
using static JtechnApi.Shares.RepositoryResult;
using JtechnApi.Departments.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;

namespace JtechnApi.Departments.Repositories
{
    public class DepartmentRepository : JtecBaseRepository<Department>, IDepartmentRepository
    {
        private readonly DBJtecContext _context;
        public DepartmentRepository(DBJtecContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Department>> GetPaginatedAsync(int page, int pageSize)
        {
            var totalItems = await _context.Departments.CountAsync();

            var items = await _context.Departments
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Department>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = items
            };
        }
        public async Task<object> GetAllAsync()
        {
            return  await _context.Departments.AsNoTracking().Select(d => new { d.Id, d.Code, d.Name, d.Description }).ToListAsync();
        }

        public new void Delete(Department entity)
        {
            entity.DeletedAt = DateTime.UtcNow;
            entity.UpdatedAt = entity.DeletedAt;
            _context.Departments.Update(entity);
            _context.SaveChanges();
        }
        public async Task<RepositoryResult> All()
        {
            return Ok(await WithEmployeeCount(_context.Departments.AsNoTracking()
                .OrderBy(d => d.Code).ThenBy(d => d.Id)).ToListAsync());
        }

        private IQueryable<DepartmentListDto> WithEmployeeCount(IQueryable<Department> query) =>
            query.Select(d => new DepartmentListDto
            {
                Id = d.Id, Code = d.Code, Name = d.Name, Description = d.Description,
                CreatedAt = d.CreatedAt, UpdatedAt = d.UpdatedAt, DeletedAt = d.DeletedAt,
                EmployeeCount = (from assignment in _context.EmployeeDepartments
                    join employee in _context.Employee on assignment.EmployeeId equals employee.Id
                    where assignment.DepartmentId == d.Id && employee.IsActive
                    select employee.Id).Distinct().Count()
            });

        public async Task<RepositoryResult> List(string search = null, int page = 1, int pageSize = 20)
        {
            if (page < 1 || page > 1000000 || pageSize < 1 || pageSize > 100 || search?.Length > 150)
                return BadRequest(new { message = "Tham số tìm kiếm hoặc phân trang không hợp lệ." });
            var query = _context.Departments.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(d => d.Code.Contains(term) || d.Name.Contains(term));
            }
            var totalItems = await query.CountAsync();
            var items = await WithEmployeeCount(query.OrderBy(d => d.Code).ThenBy(d => d.Id)
                .Skip((page - 1) * pageSize).Take(pageSize)).ToListAsync();
            return Ok(new { items, totalItems, page, pageSize, totalPages = (int)Math.Ceiling(totalItems / (double)pageSize) });
        }

        public async Task<RepositoryResult> GetById(int id)
        {
            var entity = await _context.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
            return entity == null ? NotFound(new { message = "Bộ phận không còn tồn tại." }) : Ok(entity);
        }

        public async Task<RepositoryResult> Create(DepartmentInput input)
        {
            if (input == null) return BadRequest(new { message = "Vui lòng kiểm tra dữ liệu bộ phận." });
            var code = input.Code.Trim().ToUpperInvariant();
            if (await _context.Departments.IgnoreQueryFilters().AnyAsync(d => d.Code == code)) return DuplicateCode();
            var entity = new Department { Code = code, Name = input.Name.Trim(),
                Description = NormalizeDescription(input.Description), CreatedAt = DateTime.UtcNow };
            _context.Departments.Add(entity);
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (IsDuplicate(ex)) { return DuplicateCode(); }
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        public async Task<RepositoryResult> Edit(int id, DepartmentInput input)
        {
            if (input == null) return BadRequest(new { message = "Vui lòng kiểm tra dữ liệu bộ phận." });
            var entity = await _context.Departments.FindAsync(id);
            if (entity == null) return NotFound(new { message = "Bộ phận không còn tồn tại." });
            var code = input.Code.Trim().ToUpperInvariant();
            if (await _context.Departments.IgnoreQueryFilters().AnyAsync(d => d.Code == code && d.Id != id)) return DuplicateCode();
            entity.Code = code; entity.Name = input.Name.Trim();
            entity.Description = NormalizeDescription(input.Description); entity.UpdatedAt = DateTime.UtcNow;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return NotFound(); }
            catch (DbUpdateException ex) when (IsDuplicate(ex)) { return DuplicateCode(); }
            return Ok(entity);
        }

        public async Task<RepositoryResult> Delete(int id)
        {
            var entity = await _context.Departments.FindAsync(id);
            if (entity == null) return NotFound(new { message = "Bộ phận không còn tồn tại." });
            entity.DeletedAt = DateTime.UtcNow;
            entity.UpdatedAt = entity.DeletedAt;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return NotFound(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && sql.Number == 547)
            { return Conflict(new { message = "Bộ phận đang được sử dụng, không thể xóa." }); }
            return NoContent();
        }

        private RepositoryResult DuplicateCode() => Conflict(new { message = "Mã bộ phận đã tồn tại." });
        private static bool IsDuplicate(DbUpdateException ex) => ex.InnerException is SqlException sql && (sql.Number == 2601 || sql.Number == 2627);
        private static string NormalizeDescription(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
