using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Bills.Dtos;
using Vudaco.Bills.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Bills.Repositories
{
    public class BillRepositories : BaseRepository<Bill>, IBillRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public BillRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Bill> CreateAsync(Bill Bill)
        {
            throw new NotImplementedException();
        }

        public Task<Bill> DeleteSoftAsync(Bill Bill)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(BillDto DepartmentDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Bill> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Bill> UpdateAsync(Bill Bill)
        {
            throw new NotImplementedException();
        }
    }
}
