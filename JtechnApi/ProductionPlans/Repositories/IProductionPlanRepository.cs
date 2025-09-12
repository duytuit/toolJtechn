
using System.Threading;
using System.Threading.Tasks;
using JtechnApi.ProductionPlans.Dtos;
using JtechnApi.ProductionPlans.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.ProductionPlans.Repositories
{
    public interface IProductionPlanRepository : IBaseRepository<ProductionPlan>
    {
        Task<PaginatedResultVue<object>> GetPaginatedAsync(RequestPlanDto dto, int page, int pageSize, CancellationToken cancellationToken);
    }
}
