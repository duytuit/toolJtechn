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
    public interface IFundCategoryRepository : IBaseRepository<FundCategory>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(FundCategoryDto FundCategoryDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<FundCategory> ShowAsync(int id);
        Task<FundCategory> CreateAsync(FundCategory FundCategory);
        Task<FundCategory> UpdateAsync(FundCategory FundCategory);
        Task<FundCategory> DeleteSoftAsync(FundCategory FundCategory);
    }
}
