
using System.Collections.Generic;
using System.Threading.Tasks;
using JtechnApi.Employees.Dtos;
using JtechnApi.Employees.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.Employees.Repositories
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        Task<PaginatedResult<Employee>> GetPaginatedAsync(int page, int pageSize);
        Task<Employee> GetByCode(string code);

        Task<RepositoryResult> Get();
        Task<RepositoryResult> GetById(int id);
        Task<RepositoryResult> Create(EmployeeDto input);
        Task<RepositoryResult> Update(int id, EmployeeDto input);
        Task<RepositoryResult> Delete(int id);
    }
}
