using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Depreciations.Dtos;
using Vudaco.Depreciations.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;

namespace Vudaco.Depreciations.Repositories
{
    public class DepreciationAllocationDetailRepositories : BaseRepository<DepreciationAllocationDetail>, IDepreciationAllocationDetailRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public DepreciationAllocationDetailRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<DepreciationAllocationDetail> CreateAsync(DepreciationAllocationDetail DepreciationAllocationDetail)
        {
            throw new System.NotImplementedException();
        }

        public Task<DepreciationAllocationDetail> DeleteSoftAsync(DepreciationAllocationDetail DepreciationAllocationDetail)
        {
            throw new System.NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(DepreciationAllocationDetailDto DepreciationAllocationDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public  Task<DepreciationAllocationDetail> ShowAsync(int id)
        {
            return  _context.DepreciationAllocationDetails.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<DepreciationAllocationDetail> UpdateAsync(DepreciationAllocationDetail DepreciationAllocationDetail)
        {
            throw new System.NotImplementedException();
        }
    }
}
