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
    public interface IEmployeeDepartmentRepository : IBaseRepository<EmployeeDepartment>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(EmployeeDepartmentDto EmployeeDepartmentDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<EmployeeDepartment> ShowAsync(int id);
        Task<EmployeeDepartment> CreateAsync(EmployeeDepartment EmployeeDepartment);
        Task<EmployeeDepartment> UpdateAsync(EmployeeDepartment EmployeeDepartment);
        Task<EmployeeDepartment> DeleteSoftAsync(EmployeeDepartment EmployeeDepartment);
    }
}
