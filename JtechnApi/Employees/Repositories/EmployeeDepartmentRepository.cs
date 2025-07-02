using JtechnApi.Employees.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Employees.Repositories
{
    public class EmployeeDepartmentRepository : BaseRepository<EmployeeDepartment>, IEmployeeDepartmentRepository
    {
        private readonly DBContext _context;
        public EmployeeDepartmentRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<EmployeeDepartment>> GetPaginatedAsync(int page, int pageSize)
        {
            var totalItems = await _context.EmployeeDepartment.CountAsync();

            var items = await _context.EmployeeDepartment
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<EmployeeDepartment>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = items
            };
        }

    }
}
