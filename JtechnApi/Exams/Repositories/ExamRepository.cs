using JtechnApi.Exams.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.Exams.Repositories
{
    public class ExamRepository : BaseRepository<Exam>, IExamRepository
    {
        private readonly DBContext _context;
        public ExamRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<Exam>> GetPaginatedAsync(int page, int pageSize)
        {
            var totalItems = await _context.Exam.CountAsync();

            var items = await _context.Exam
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Exam>
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,
                Items = items
            };
        }

        Task<PaginatedResult<Exam>> IExamRepository.GetPaginatedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }
    }
}
