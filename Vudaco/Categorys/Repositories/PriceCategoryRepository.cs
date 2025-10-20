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
    public class PriceCategoryRepository : BaseRepository<PriceCategory>, IPriceCategoryRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public PriceCategoryRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<PriceCategory> CreateAsync(PriceCategory PriceCategory)
        {
            throw new NotImplementedException();
        }

        public Task<PriceCategory> DeleteSoftAsync(PriceCategory PriceCategory)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(PriceCategoryDto PriceCategoryDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<PriceCategory> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<PriceCategory> UpdateAsync(PriceCategory PriceCategory)
        {
            throw new NotImplementedException();
        }
    }
}
