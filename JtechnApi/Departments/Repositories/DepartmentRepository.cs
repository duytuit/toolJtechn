using JtechnApi.Departments.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace JtechnApi.Departments.Repositories
{
    public class DepartmentRepository : BaseRepository<Department>, IDepartmentRepository
    {
        private readonly DBContext _context;
        public DepartmentRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public bool Any(Expression<Func<Department, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public void Delete(Department entity)
        {
            throw new NotImplementedException();
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

        public IEnumerable<Department> List(Expression<Func<Department, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public Department Save(Department entity)
        {
            throw new NotImplementedException();
        }

        public Department Update(Department entity)
        {
            throw new NotImplementedException();
        }

        Task<PaginatedResult<Department>> IDepartmentRepository.GetPaginatedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
