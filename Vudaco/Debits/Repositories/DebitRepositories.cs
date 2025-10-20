using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Debits.Dtos;
using Vudaco.Debits.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Debits.Repositories
{
    public class DebitRepositories : BaseRepository<Debit>, IDebitRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public DebitRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Debit> CreateAsync(Debit Debit)
        {
            throw new NotImplementedException();
        }

        public Task<Debit> DeleteSoftAsync(Debit Debit)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(DebitDto DepartmentDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Debit> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Debit> UpdateAsync(Debit Debit)
        {
            throw new NotImplementedException();
        }
    }
}
