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
    public interface IPriceCategoryRepository : IBaseRepository<PriceCategory>
    {
        Task<PaginatedResultReact<object>> GetObjectTaskAsync(PriceCategoryDto PriceCategoryDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<PriceCategory> ShowAsync(int id);
        Task<PriceCategory> CreateAsync(PriceCategory PriceCategory);
        Task<PriceCategory> UpdateAsync(PriceCategory PriceCategory);
        Task<PriceCategory> DeleteSoftAsync(PriceCategory PriceCategory);
    }
}
