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
    public class BankRepository : BaseRepository<Bank>, IBankRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public BankRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Bank> CreateAsync(Bank Bank)
        {
            throw new NotImplementedException();
        }

        public Task<Bank> DeleteSoftAsync(Bank Bank)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(BankDto BankDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Bank> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Bank> UpdateAsync(Bank Bank)
        {
            throw new NotImplementedException();
        }
    }
}
