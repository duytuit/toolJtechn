
using JtechnApi.BorrowProducts.Dtos;
using JtechnApi.BorrowProducts.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.AdoHelper;
using JtechnApi.Shares.BaseRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace JtechnApi.BorrowProducts.Repositories
{
    public class DataSayRepository : BaseRepository<DataSay>, IDataSayRepository
    {
        private readonly DBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        private readonly ILogger<DataSayRepository> _logger;
        public DataSayRepository(DBContext context, IConfiguration configuration, RedisService redis, ILogger<DataSayRepository> logger) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
            _logger = logger;
        }

        public Task<DataSay> ChangeStatusAsync(DataSay DataSay)
        {
            throw new NotImplementedException();
        }

        public Task<DataSay> CreateAsync(DataSay DataSay)
        {
            throw new NotImplementedException();
        }

        public Task<DataSay> DeleteSoftAsync(DataSay DataSay)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedResultVue<object>> GetPaginatedAsync(DataSayDto dto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id Desc" };
            if (!string.IsNullOrWhiteSpace(dto.Code))
                whereLikes["code"] = dto.Code;
            if (!string.IsNullOrWhiteSpace(dto.Lot))
                whereLikes["lot"] = dto.Lot;
            if (dto.UpdatedBy > 0)
                whereEquals["updated_by"] = dto.UpdatedBy;
            whereEquals["type"] = 2;
            if (dto.From_date.HasValue)
                whereDateRange.Add(("created_at", dto.From_date.Value, dto.From_date.Value.AddDays(1)));
            // var departments = await _context.Department.AsNoTracking().Select($"new ({"id,code,name,status,permissions"})").ToDynamicListAsync();
            dynamic results = await AdoRelationQuery.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "upload_data_cams",
                        new[] { "id","lot","code","content","attach","type","note","date","user_by","deleted_at","created_at" },
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
            var _results = new PaginatedResultVue<object>
            {
                Current_page = page,
                Per_page = pageSize,
                Last_page = (int)Math.Ceiling((double)totalItems / pageSize),
                Total = totalItems,
                Data = objectList,
            };
            objectList = null;
            results = null;
            whereEquals?.Clear(); whereLikes?.Clear(); whereDateRange?.Clear(); orderByList?.Clear();
            return _results;
        }

        public Task<DataSay> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<DataSay> UpdateAsync(DataSay DataSay)
        {
            throw new NotImplementedException();
        }
    }
}
