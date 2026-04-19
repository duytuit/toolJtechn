
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Depreciations.Dtos;
using Vudaco.Depreciations.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Depreciations.Repositories
{
    public interface IDepreciationAllocationRepositories : IBaseRepository<DepreciationAllocation>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(DepreciationAllocationDto DepreciationAllocationDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<DepreciationAllocation> ShowAsync(int id);
        Task<DepreciationAllocation> CreateAsync(DepreciationAllocation DepreciationAllocation);
        Task<DepreciationAllocation> UpdateAsync(DepreciationAllocation DepreciationAllocation);
        Task<DepreciationAllocation> DeleteSoftAsync(DepreciationAllocation DepreciationAllocation);
    }
}
