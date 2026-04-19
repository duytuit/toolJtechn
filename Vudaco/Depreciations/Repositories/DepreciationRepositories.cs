using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Depreciations.Dtos;
using Vudaco.Depreciations.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Depreciations.Repositories
{
    public class DepreciationRepositories : BaseRepository<Depreciation>, IDepreciationRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public DepreciationRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Depreciation> CreateAsync(Depreciation Depreciation)
        {
            throw new NotImplementedException();
        }

        public Task<Depreciation> DeleteSoftAsync(Depreciation Depreciation)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(DepreciationDto DepreciationDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Depreciation> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Depreciation> UpdateAsync(Depreciation Depreciation)
        {
            throw new NotImplementedException();
        }
    }
}
