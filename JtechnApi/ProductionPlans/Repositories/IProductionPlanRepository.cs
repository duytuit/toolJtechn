
using System.Threading.Tasks;
using JtechnApi.ProductionPlans.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;

namespace JtechnApi.ProductionPlans.Repositories
{
    public interface IProductionPlanRepository : IBaseRepository<ProductionPlan>
    {
        Task<PaginatedResult<ProductionPlan>> GetPaginatedAsync(int page, int pageSize);
    }
}
