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
    public class IncomeCategoryRepository : BaseRepository<IncomeCategory>, IIncomeCategoryRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public IncomeCategoryRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<IncomeCategory> CreateAsync(IncomeCategory IncomeCategory)
        {
            throw new NotImplementedException();
        }

        public Task<IncomeCategory> DeleteSoftAsync(IncomeCategory IncomeCategory)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(IncomeCategoryDto IncomeCategoryDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IncomeCategory> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IncomeCategory> UpdateAsync(IncomeCategory IncomeCategory)
        {
            throw new NotImplementedException();
        }
    }
}
