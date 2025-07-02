
using System.Threading.Tasks;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using JtechnApi.Users.Models;

namespace JtechnApi.Users.Repositories
{
    public interface IAdminRepository : IBaseRepository<Admin>
    {
        Task<PaginatedResult<Admin>> GetPaginatedAsync(int page, int pageSize);
    }
}
