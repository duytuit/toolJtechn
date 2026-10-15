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
    }
}
