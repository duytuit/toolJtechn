using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Receipts.Dtos;
using Vudaco.Receipts.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Receipts.Repositories
{
    public class ReceiptDetailRepositories : BaseRepository<ReceiptDetail>, IReceiptDetailRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public ReceiptDetailRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<ReceiptDetail> CreateAsync(ReceiptDetail ReceiptDetail)
        {
            throw new NotImplementedException();
        }

        public Task<ReceiptDetail> DeleteSoftAsync(ReceiptDetail ReceiptDetail)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(ReceiptDetailDto ReceiptDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ReceiptDetail> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ReceiptDetail> UpdateAsync(ReceiptDetail ReceiptDetail)
        {
            throw new NotImplementedException();
        }
    }
}
