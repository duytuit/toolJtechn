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
    public class UserStorageRepository : BaseRepository<UserStorage>, IUserStorageRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public UserStorageRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<UserStorage> CreateAsync(UserStorage UserStorage)
        {
            _context.UserStorages.Add(UserStorage);
            _context.SaveChanges();
            return Task.FromResult(UserStorage);
        }

        public Task<UserStorage> DeleteSoftAsync(UserStorage UserStorage)
        {
            _context.UserStorages.Update(UserStorage);
            _context.SaveChanges();
            return Task.FromResult(UserStorage);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(UserStorageDto UserStorageDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id" };

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "user_storages",
                        new[] { "id", "user_id", "storage_id", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
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
            objectList = null;
            results = null;
            whereEquals?.Clear(); whereLikes?.Clear(); whereDateRange?.Clear(); orderByList?.Clear();
            return _results;
        }

        public Task<UserStorage> ShowAsync(int id)
        {
            return _context.UserStorages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<UserStorage> UpdateAsync(UserStorage UserStorage)
        {
            _context.UserStorages.Update(UserStorage);
            _context.SaveChanges();
            return Task.FromResult(UserStorage);
        }
    }
}
