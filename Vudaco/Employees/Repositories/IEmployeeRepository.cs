using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Employees.Dtos;
using Vudaco.Employees.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Employees.Repositories
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(EmployeeDto EmployeeDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Employee> ShowAsync(int id);
        Task<Employee> InfoEmployeeAsync(int userId);
        Task<Employee> CreateAsync(Employee Employee);
        Task<Employee> UpdateAsync(Employee Employee);
        Task<Employee> DeleteSoftAsync(Employee Employee);
    }
}
