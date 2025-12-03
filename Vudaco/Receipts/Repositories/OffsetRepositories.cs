using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Receipts.Dtos;
using Vudaco.Receipts.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Receipts.Repositories
{
    public class OffsetRepositories : BaseRepository<Offset>, IOffsetRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public OffsetRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Offset> CreateAsync(Offset Offset)
        {
            throw new NotImplementedException();
        }

        public Task<Offset> DeleteSoftAsync(Offset Offset)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(OffsetDto OffsetDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Offset> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Offset> UpdateAsync(Offset Offset)
        {
            throw new NotImplementedException();
        }
    }
}
