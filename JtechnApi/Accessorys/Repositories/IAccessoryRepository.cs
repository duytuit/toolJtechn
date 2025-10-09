
using System.Threading.Tasks;
using JtechnApi.Accessorys.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.Accessorys.Repositories
{
    public interface IAccessoryRepository : IBaseRepository<Accessory>
    {
        Task<PaginatedResult<Accessory>> GetPaginatedAsync(int page, int pageSize);
    }
}
