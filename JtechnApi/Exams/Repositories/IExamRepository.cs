
using System.Threading.Tasks;
using JtechnApi.Exams.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.Exams.Repositories
{
    public interface IExamRepository : IBaseRepository<Exam>
    {
        Task<PaginatedResult<Exam>> GetPaginatedAsync(int page, int pageSize);
    }
}
