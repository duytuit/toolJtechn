
using System.Threading.Tasks;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Umesens.Models;

namespace JtechnApi.Umesens.Repositories
{
    public interface IUmesenRepository : IBaseRepository<Umesen>
    {
        Task<PaginatedResult<Umesen>> GetPaginatedAsync(int page, int pageSize);
    }
}
