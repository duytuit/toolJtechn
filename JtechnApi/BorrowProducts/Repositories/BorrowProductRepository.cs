
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
    public class BorrowProductRepository : BaseRepository<BorrowProduct>, IBorrowProductRepository
    {
        private readonly DBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        private readonly ILogger<BorrowProductRepository> _logger;
        public BorrowProductRepository(DBContext context, IConfiguration configuration, RedisService redis, ILogger<BorrowProductRepository> logger) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
            _logger = logger;
        }

        public Task<BorrowProduct> CreateAsync(BorrowProduct BorrowProduct)
        {
            throw new System.NotImplementedException();
        }

        public Task<BorrowProduct> DeleteSoftAsync(BorrowProduct BorrowProduct)
        {
            throw new System.NotImplementedException();
        }

        public async Task<PaginatedResultVue<object>> GetPaginatedAsync(BorrowProductDto dto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id Desc" };
            

            if (!string.IsNullOrWhiteSpace(dto.Code))
                whereLikes["code"] = dto.Code;
            if (dto.UpdatedBy > 0)
                whereEquals["updated_by"] = dto.UpdatedBy;
            if (dto.From_date.HasValue)
                whereDateRange.Add(("created_at", dto.From_date.Value, dto.From_date.Value.AddDays(1)));
            // var departments = await _context.Department.AsNoTracking().Select($"new ({"id,code,name,status,permissions"})").ToDynamicListAsync();
            dynamic results = await AdoRelationQuery.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "borrow_products",
                        new[] { "id", "code", "quantity", "note","status", "updated_by","created_by", "deleted_at", "created_at", "updated_at" },
                        offset: (page - 1) * pageSize,
                        limit: pageSize,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                         relations: new List<AdoRelation>
                        {
                            new AdoRelation
                            {
                                Name = "employee",
                                Table = "employees",
                                Columns = new[] { "id","code","first_name","last_name","updated_by","deleted_at","created_at","updated_at"},
                                ParentKey = "created_by",
                                ForeignKey = "id",
                                KeyName = "id",
                                IsCollection = false,
                            },
                             new AdoRelation
                            {
                                Name = "employee_update",
                                Table = "employees",
                                Columns = new[] { "id","code","first_name","last_name","updated_by","deleted_at","created_at","updated_at"},
                                ParentKey = "updated_by",
                                ForeignKey = "id",
                                KeyName = "id",
                                IsCollection = false,
                            }
                        },
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

        public Task<BorrowProduct> ShowAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<BorrowProduct> UpdateAsync(BorrowProduct BorrowProduct)
        {
            throw new System.NotImplementedException();
        } 
        public Task<BorrowProduct> ChangeStatusAsync(BorrowProduct BorrowProduct)
        {
            throw new System.NotImplementedException();
        }
    }
}
