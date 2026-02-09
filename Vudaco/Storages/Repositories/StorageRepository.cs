using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;
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
            _context.Storages.Add(Storage);
            _context.SaveChanges();
            return Task.FromResult(Storage);
        }

        public Task<Storage> DeleteSoftAsync(Storage Storage)
        {
            _context.Storages.Update(Storage);
            _context.SaveChanges();
            return Task.FromResult(Storage);
        }

        public async Task<PaginatedResultReact<object>> GetByUserIdAsync(StorageDto StorageDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"SELECT s.* FROM employees e LEFT JOIN data_storages s on s.id = e.storage_id WHERE e.deleted_at IS NULL AND s.deleted_at IS NULL";
            if (StorageDto.UserId > 0)
            {
                sql += $@" AND e.user_id = {StorageDto.UserId}";
            }
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(StorageDto StorageDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id" };

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "data_storages",
                        new[] { "id", "code", "name", "note", "address", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
                        offset: null,
                        limit: null,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        redisCache: _redis,
                        includeCount: false,
                        cancellationToken: cancellationToken
                    );
            int totalItems = results.Count;
            var objectList = new List<object>();
            objectList.AddRange(results.Data);
            var _results = new PaginatedResultReact<object>
            {
                PageNum = page,
                PageSize = pageSize,
                First = (int)Math.Ceiling((double)totalItems / pageSize),
                Total = totalItems,
                Data = objectList,
            };
            whereEquals?.Clear(); whereLikes?.Clear(); whereDateRange?.Clear(); orderByList?.Clear();
            return _results;
        }

        public Task<Storage> ShowAsync(int id)
        {
            return _context.Storages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Storage> UpdateAsync(Storage Storage)
        {
            _context.Storages.Update(Storage);
            _context.SaveChanges();
            return Task.FromResult(Storage);
        }
    }
}
