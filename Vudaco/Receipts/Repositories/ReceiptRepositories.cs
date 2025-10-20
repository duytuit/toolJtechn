using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Receipts.Dtos;
using Vudaco.Receipts.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Receipts.Repositories
{
    public class ReceiptRepositories : BaseRepository<Receipt>, IReceiptRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public ReceiptRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Receipt> CreateAsync(Receipt Receipt)
        {
            throw new NotImplementedException();
        }

        public Task<Receipt> DeleteSoftAsync(Receipt Receipt)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Receipt> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Receipt> UpdateAsync(Receipt Receipt)
        {
            throw new NotImplementedException();
        }
    }
}
