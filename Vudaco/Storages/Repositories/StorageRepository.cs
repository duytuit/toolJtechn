using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Storages.Dtos;
using Vudaco.Storages.Models;

namespace Vudaco.Storages.Repositories
{
    public class StorageRepository : BaseRepository<Storage>, IStorageRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public StorageRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<Storage> CreateAsync(Storage Storage)
        {
            throw new NotImplementedException();
        }

        public Task<Storage> DeleteSoftAsync(Storage Storage)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResultReact<object>> GetObjectTaskAsync(StorageDto StorageDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Storage> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Storage> UpdateAsync(Storage Storage)
        {
            throw new NotImplementedException();
        }
    }
}
