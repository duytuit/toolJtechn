using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Depreciations.Dtos;
using Vudaco.Depreciations.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Depreciations.Repositories
{
    public interface IDepreciationRepositories : IBaseRepository<Depreciation>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(DepreciationDto DepreciationDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<Depreciation> ShowAsync(int id);
        Task<Depreciation> CreateAsync(Depreciation Depreciation);
        Task<Depreciation> UpdateAsync(Depreciation Depreciation);
        Task<Depreciation> DeleteSoftAsync(Depreciation Depreciation);
    }
}
