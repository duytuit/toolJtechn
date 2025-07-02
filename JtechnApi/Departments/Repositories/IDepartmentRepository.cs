using JtechnApi.Departments.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using System.Threading.Tasks;

namespace JtechnApi.Departments.Repositories
{
    public interface IDepartmentRepository : IBaseRepository<Department>
    {
        Task<PaginatedResult<Department>> GetPaginatedAsync(int page, int pageSize);
    }
}
