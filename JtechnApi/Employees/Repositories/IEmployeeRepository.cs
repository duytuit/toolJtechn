using System.Threading.Tasks;
using JtechnApi.Employees.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.Employees.Repositories
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        Task<PaginatedResult<Employee>> GetPaginatedAsync(int page, int pageSize);
    }
}
