using JtechnApi.Employees.Dtos;
using JtechnApi.Employees.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
        public async Task<List<SelectEmployeeDto>> GetByListCode(List<string> codes)
        {
            List<SelectEmployeeDto> _emps = await _context.Employee.Where(e => codes.Contains(e.Code))
                .Select(e => new SelectEmployeeDto
                {
                    Id = e.Id,
                    Code = e.Code,
                    Last_name = e.Last_name,
                    First_name = e.First_name,
                })
                .AsNoTracking()
                .ToListAsync();
            if (_emps == null || _emps.Count == 0)
            {
                return new List<SelectEmployeeDto>();
            }
            var EmployeeDepartmentIds = _emps.Select(c => c.Id).ToList();
             List<SelectEmployeeDepartmentDto> emp_depts = await _context.EmployeeDepartment.Where(p => EmployeeDepartmentIds.Contains(p.Employee_id)).Select(e => new SelectEmployeeDepartmentDto
            {
                Id = e.Id,
                Employee_id = e.Employee_id,
                Department_id = e.Department_id,
                Permissions = e.Permissions,
            }).AsNoTracking().ToListAsync();
            var __result = new List<SelectEmployeeDto>();
            foreach (SelectEmployeeDto item in _emps)
            {
                item.SelectEmployeeDepartmentDto = emp_depts.FirstOrDefault(p => p.Employee_id == item.Id);
                __result.Add(item);
            }
            return __result;
        }

        public Task<Employee> GetByCode(string code)
        {
            return _context.Employee
                .Where(e => e.Code == code)
                .Select(e => new Employee
                {
                    Id = e.Id,
                    Code = e.Code,
                    Last_name = e.Last_name,
                    First_name = e.First_name,
                })
                .FirstOrDefaultAsync();
        }
    }
}
