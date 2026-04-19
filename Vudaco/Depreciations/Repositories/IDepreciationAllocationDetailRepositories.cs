using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Bills.Dtos;
using Vudaco.Bills.Models;
using Vudaco.Depreciations.Dtos;
using Vudaco.Depreciations.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Depreciations.Repositories
{
    public interface IDepreciationAllocationDetailRepositories : IBaseRepository<DepreciationAllocationDetail>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(DepreciationAllocationDetailDto DepreciationAllocationDetailDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<DepreciationAllocationDetail> ShowAsync(int id);
        Task<DepreciationAllocationDetail> CreateAsync(DepreciationAllocationDetail DepreciationAllocationDetail);
        Task<DepreciationAllocationDetail> UpdateAsync(DepreciationAllocationDetail DepreciationAllocationDetail);
        Task<DepreciationAllocationDetail> DeleteSoftAsync(DepreciationAllocationDetail DepreciationAllocationDetail);
    }
}
