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
    public class FundCategoryRepository : BaseRepository<FundCategory>, IFundCategoryRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public FundCategoryRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<FundCategory> CreateAsync(FundCategory FundCategory)
        {
            _context.FundCategorys.Add(FundCategory);
            _context.SaveChanges();
            return Task.FromResult(FundCategory);
        }

        public Task<FundCategory> DeleteSoftAsync(FundCategory FundCategory)
        {
            _context.FundCategorys.Update(FundCategory);
            _context.SaveChanges();
            return Task.FromResult(FundCategory);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(FundCategoryDto FundCategoryDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "updated_at desc", "id" };
            // if (FundCategoryDto.StorageId > 0)
            //     whereEquals["storage_id"] = FundCategoryDto.StorageId;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "fund_categorys",
                        new[] { "id", "fund_code", "fund_name", "storage_id", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
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

        public Task<FundCategory> ShowAsync(int id)
        {
            return _context.FundCategorys.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<FundCategory> UpdateAsync(FundCategory FundCategory)
        {
            _context.FundCategorys.Update(FundCategory);
            _context.SaveChanges();
            return Task.FromResult(FundCategory);
        }
    }
}
