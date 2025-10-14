
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Vudaco.Auth.Models;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares;
using Vudaco.Auth.Dtos;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Auth.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly VudacoDBContext _context;
        private readonly ILogger<UserRepository> _logger;
        private readonly RedisService _redis;
        private readonly IConfiguration _configuration;
        public UserRepository(VudacoDBContext context, ILogger<UserRepository> logger, RedisService redis, IConfiguration configuration) : base(context)
        {
            _context = context;
            _logger = logger;
            _redis = redis;
            _configuration = configuration;
        }

        public Task<int> CheckDuplicateTitle(string title, int from_type, DateTime? created_client)
        {
            throw new NotImplementedException();
        }

        public Task<User> CreateRequiredAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteRequiredAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<object> detail(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(UserDto UserDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string>{"id"};
          
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "users",
                        new[] { "id", "first_name", "last_name", "username", "phone_no", "email", "email_verified_at", "avatar", "status", "deleted_at", "updated_by", "created_at", "updated_at" },
                        offset: (page - 1) * pageSize,
                        limit: pageSize,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        redisCache: _redis,
                        includeCount: true,
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

        public Task<PaginatedResult<User>> GetPaginatedAsync(UserDto UserDto, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedResult<User>> GetTaskAsync(UserDto UserDto, int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public Task<User> show(int id)
        {
            throw new NotImplementedException();
        }

        public Task<User> UpdateRequiredAsync(User user)
        {
            throw new NotImplementedException();
        }
    }
}
