
using JtechnApi.Requireds.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Requireds.Repositories
{
    public class SignatureSubmissionRepository : BaseRepository<SignatureSubmission>, ISignatureSubmissionRepository
    {
        private readonly DBContext _context;
        public SignatureSubmissionRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<SignatureSubmission>> GetPaginatedAsync(int page, int pageSize)
        {
            var totalItems = await _context.SignatureSubmission.CountAsync();

            var items = await _context.SignatureSubmission
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<SignatureSubmission>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = items
            };
        }

        Task<PaginatedResult<SignatureSubmission>> ISignatureSubmissionRepository.GetPaginatedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
