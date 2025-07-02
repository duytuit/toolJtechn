
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.UploadDatas.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.UploadDatas.Repositories
{
    public class UploadDataRepository : BaseRepository<UploadData>, IUploadDataRepository
    {
        private readonly DBContext _context;
        public UploadDataRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<UploadData>> GetPaginatedAsync(int page, int pageSize)
        {
            var totalItems = await _context.UploadData.CountAsync();

            var items = await _context.UploadData
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<UploadData>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = items
            };
        }

        Task<PaginatedResult<UploadData>> IUploadDataRepository.GetPaginatedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
