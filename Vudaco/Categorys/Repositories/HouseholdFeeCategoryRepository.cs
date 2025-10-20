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
    public class HouseholdFeeCategoryRepository : BaseRepository<HouseholdFeeCategory>, IHouseholdFeeCategoryRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public HouseholdFeeCategoryRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<HouseholdFeeCategory> CreateAsync(HouseholdFeeCategory HouseholdFeeCategory)
        {
            throw new NotImplementedException();
        }

        public Task<HouseholdFeeCategory> DeleteSoftAsync(HouseholdFeeCategory HouseholdFeeCategory)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(HouseholdFeeCategoryDto HouseholdFeeCategoryDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<HouseholdFeeCategory> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<HouseholdFeeCategory> UpdateAsync(HouseholdFeeCategory HouseholdFeeCategory)
        {
            throw new NotImplementedException();
        }
    }
}
