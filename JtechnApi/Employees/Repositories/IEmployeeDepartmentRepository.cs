
using System.Threading.Tasks;
using JtechnApi.Employees.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.Employees.Repositories
{
    public interface IEmployeeDepartmentRepository : IBaseRepository<EmployeeDepartment>
    {
        Task<PaginatedResult<EmployeeDepartment>> GetPaginatedAsync(int page, int pageSize);
    }
}
