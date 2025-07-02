

using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Users.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Users.Repositories
{
    public class AdminRepository : BaseRepository<Admin>, IAdminRepository
    {
        private readonly DBContext _context;
        public AdminRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Admin>> GetPaginatedAsync(int page, int pageSize)
        {
            var totalItems = await _context.Admin.CountAsync();

            var items = await _context.Admin
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Admin>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = items
            };
        }

        Task<PaginatedResult<Admin>> IAdminRepository.GetPaginatedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
