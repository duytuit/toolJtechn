
using JtechnApi.ProductionPlans.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace JtechnApi.ProductionPlans.Repositories
{
    public class ProductionPlanRepository : BaseRepository<ProductionPlan>, IProductionPlanRepository
    {
        private readonly DBContext _context;
        public ProductionPlanRepository(DBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<ProductionPlan>> GetPaginatedAsync(int page, int pageSize)
        {
          //  var totalItems = await _context.ProductionPlan.AsNoTracking().CountAsync();

            var items = await _context.ProductionPlan
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<ProductionPlan>
            {
                CurrentPage = page,
           /*     TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                TotalItems = totalItems,*/
                Items = items
            };
        }

        Task<PaginatedResult<ProductionPlan>> IProductionPlanRepository.GetPaginatedAsync(int page, int pageSize)
        {
            throw new System.NotImplementedException();
        }
    }
}
