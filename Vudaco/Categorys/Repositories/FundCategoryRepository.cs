using Microsoft.Extensions.Configuration;
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
    public class FundCategoryRepository : BaseRepository<FundCategory>, IFundCategoryRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public FundCategoryRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<FundCategory> CreateAsync(FundCategory FundCategory)
        {
            throw new NotImplementedException();
        }

        public Task<FundCategory> DeleteSoftAsync(FundCategory FundCategory)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(FundCategoryDto FundCategoryDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<FundCategory> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<FundCategory> UpdateAsync(FundCategory FundCategory)
        {
            throw new NotImplementedException();
        }
    }
}
