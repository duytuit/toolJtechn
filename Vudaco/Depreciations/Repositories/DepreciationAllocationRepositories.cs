using Microsoft.Extensions.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Depreciations.Dtos;
using Vudaco.Depreciations.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Depreciations.Repositories
{
    public class DepreciationAllocationRepositories : BaseRepository<DepreciationAllocation>, IDepreciationAllocationRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public DepreciationAllocationRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<DepreciationAllocation> CreateAsync(DepreciationAllocation DepreciationAllocation)
        {
            throw new System.NotImplementedException();
        }

        public Task<DepreciationAllocation> DeleteSoftAsync(DepreciationAllocation DepreciationAllocation)
        {
            _context.DepreciationAllocations.Update(DepreciationAllocation);
            _context.SaveChanges();
            return Task.FromResult(DepreciationAllocation);
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(DepreciationAllocationDto DepreciationAllocationDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<DepreciationAllocation> ShowAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<DepreciationAllocation> UpdateAsync(DepreciationAllocation DepreciationAllocation)
        {
            throw new System.NotImplementedException();
        }
    }
}
