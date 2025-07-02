using JtechnApi.Employees.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Employees.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        private readonly DBContext _context;
        public EmployeeRepository(DBContext context) : base(context)
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

        Task<PaginatedResult<Employee>> IEmployeeRepository.GetPaginatedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
