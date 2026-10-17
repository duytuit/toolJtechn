using JtechnApi.Departments.Dtos;
using JtechnApi.Departments.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using System.Threading.Tasks;

namespace JtechnApi.Departments.Repositories
{
    public interface IDepartmentRepository : IBaseRepository<Department>
    {
        Task<PaginatedResult<Department>> GetPaginatedAsync(int page, int pageSize);
        Task<object> GetAllAsync();

        Task<RepositoryResult> All();
        Task<RepositoryResult> List(string search = null, int page = 1, int pageSize = 20);
        Task<RepositoryResult> GetById(int id);
        Task<RepositoryResult> Create(DepartmentInput input);
        Task<RepositoryResult> Edit(int id, DepartmentInput input);
        Task<RepositoryResult> Delete(int id);
    }
}
