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
    public interface IBranchCategoryRepository : IBaseRepository<BranchCategory>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(BranchCategoryDto BranchCategoryDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<BranchCategory> ShowAsync(int id);
        Task<BranchCategory> CreateAsync(BranchCategory BranchCategory);
        Task<BranchCategory> UpdateAsync(BranchCategory BranchCategory);
        Task<BranchCategory> DeleteSoftAsync(BranchCategory BranchCategory);
    }
}
