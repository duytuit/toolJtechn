using JtechnApi.Accessorys.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Accessorys.Repositories
{
    public class AccessoryRepository : BaseRepository<Accessory>, IAccessoryRepository
    {
        private readonly DBContext _context;
        public AccessoryRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Accessory>> GetPaginatedAsync(int page, int pageSize)
        {
            var totalItems = await _context.Accessory.CountAsync();

            var items = await _context.Accessory
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Accessory>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = items
            };
        }

        Task<PaginatedResult<Accessory>> IAccessoryRepository.GetPaginatedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
