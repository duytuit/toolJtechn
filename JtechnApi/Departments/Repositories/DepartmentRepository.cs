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
    public class Repository : BaseRepository<Department>, IDepartmentRepository
    {
        private readonly DBContext _context;
        public Repository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Department>> GetPaginatedAsync(int page, int pageSize)
        {
            var totalItems = await _context.Department.CountAsync();

            var items = await _context.Department
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
            return  await _context.Department.AsNoTracking().Select("new (id, code, name, status, permissions)").ToDynamicListAsync();
        }
    }
}
