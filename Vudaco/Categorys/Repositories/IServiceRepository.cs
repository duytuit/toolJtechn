using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Categorys.Dtos;
using Vudaco.Categorys.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Categorys.Repositories
{
    public interface IServiceRepository : IBaseRepository<ServiceCategory>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(ServiceCategoryDto ServiceCategoryDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<ServiceCategory> ShowAsync(int id);
        Task<ServiceCategory> CreateAsync(ServiceCategory ServiceCategory);
        Task<ServiceCategory> UpdateAsync(ServiceCategory ServiceCategory);
        Task<ServiceCategory> DeleteSoftAsync(ServiceCategory ServiceCategory);
    }
}
