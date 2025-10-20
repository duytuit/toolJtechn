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
    public interface IAdministrativeFeeRepository : IBaseRepository<AdministrativeFeeCategory>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(AdministrativeFeeCategoryDto AdministrativeFeeCategoryDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<AdministrativeFeeCategory> ShowAsync(int id);
        Task<AdministrativeFeeCategory> CreateAsync(AdministrativeFeeCategory AdministrativeFeeCategory);
        Task<AdministrativeFeeCategory> UpdateAsync(AdministrativeFeeCategory AdministrativeFeeCategory);
        Task<AdministrativeFeeCategory> DeleteSoftAsync(AdministrativeFeeCategory AdministrativeFeeCategory);
    }
}
