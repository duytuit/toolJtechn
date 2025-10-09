
using JtechnApi.Requireds.Repositories;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Umesens.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Umesens.Repositories
{
    public class UmesenRepository : BaseRepository<Umesen>, IUmesenRepository
    {
        private readonly DBContext _context;
        public UmesenRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Umesen>> GetPaginatedAsync(int page, int pageSize)
        {
            var totalItems = await _context.Umesen.CountAsync();

            var items = await _context.Umesen
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Umesen>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = items
            };
        }

        Task<PaginatedResult<Umesen>> IUmesenRepository.GetPaginatedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
