using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Departments.Dtos;
using Vudaco.Departments.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Departments.Repositories
{
    public interface IDepartmentRepositories : IBaseRepository<Department>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(DepartmentDto DepartmentDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Department> ShowAsync(int id);
        Task<Department> CreateAsync(Department Department);
        Task<Department> UpdateAsync(Department Department);
        Task<Department> DeleteSoftAsync(Department Department);
    }
}
