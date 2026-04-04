using Microsoft.EntityFrameworkCore;
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
using Vudaco.Shares.SqlServerHelper;

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
            _context.Banks.Add(Bank);
            _context.SaveChanges();
            return Task.FromResult(Bank);
        }

        public Task<Bank> DeleteSoftAsync(Bank Bank)
        {
            _context.Banks.Update(Bank);
            _context.SaveChanges();
            return Task.FromResult(Bank);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(BankDto BankDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "updated_at desc", "id" };
            if (BankDto.StorageId > 0)
                whereEquals["storage_id"] = BankDto.StorageId;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "banks",
                        new[] { "id", "account_number", "bank_name", "branch_name", "storage_id", "account_holder", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
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

        public Task<Bank> ShowAsync(int id)
        {
            return _context.Banks.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Bank> UpdateAsync(Bank Bank)
        {
            _context.Banks.Update(Bank);
            _context.SaveChanges();
            return Task.FromResult(Bank);
        }
    }
}
