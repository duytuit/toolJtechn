using JtechnApi.Shares;
using JtechnApi.Employees.Dtos;

namespace JtechnApi.Employees.Repositories
{
    public interface IEmployeeDepartmentRepository
    {
        Task<object> GetOptions();
        Task<object> SearchEmployees(string q);
        Task<RepositoryResult> Remove(int departmentId, int employeeId);
        Task<RepositoryResult> Get(int employeeId);
        Task<RepositoryResult> Save(int employeeId, EmployeeDepartmentInput input);
        Task<RepositoryResult> Synchronize();
        Task<RepositoryResult> GetDepartmentEmployees(int departmentId);
    }
}
