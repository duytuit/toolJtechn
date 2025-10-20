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
    public interface IHouseholdFeeCategoryRepository : IBaseRepository<HouseholdFeeCategory>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(HouseholdFeeCategoryDto HouseholdFeeCategoryDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<HouseholdFeeCategory> ShowAsync(int id);
        Task<HouseholdFeeCategory> CreateAsync(HouseholdFeeCategory HouseholdFeeCategory);
        Task<HouseholdFeeCategory> UpdateAsync(HouseholdFeeCategory HouseholdFeeCategory);
        Task<HouseholdFeeCategory> DeleteSoftAsync(HouseholdFeeCategory HouseholdFeeCategory);
    }
}
