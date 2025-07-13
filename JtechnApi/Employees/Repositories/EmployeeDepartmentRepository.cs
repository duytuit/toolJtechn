using JtechnApi.Employees.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;

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
        public async Task<object> GetListByIdEmp(List<int> id_emps)
        {
            return  await _context.EmployeeDepartment.AsNoTracking().Where(u=> id_emps.Contains(u.Employee_id)).Select("new (id, employee_id, department_id, positions, permissions)").ToDynamicListAsync();
        }

    }
}
