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
    public class BranchCategoryRepository : BaseRepository<BranchCategory>, IBranchCategoryRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public BranchCategoryRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<BranchCategory> CreateAsync(BranchCategory BranchCategory)
        {
            throw new NotImplementedException();
        }

        public Task<BranchCategory> DeleteSoftAsync(BranchCategory BranchCategory)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(BranchCategoryDto BranchCategoryDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<BranchCategory> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<BranchCategory> UpdateAsync(BranchCategory BranchCategory)
        {
            throw new NotImplementedException();
        }
    }
}
