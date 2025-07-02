
using System.Threading.Tasks;
using JtechnApi.Requireds.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.Requireds.Repositories
{
    public interface IRequiredRepository : IBaseRepository<Required>
    {
        Task<PaginatedResult<Required>> GetPaginatedAsync(int page, int pageSize);
    }
}
