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
    public class ServiceRepository : BaseRepository<ServiceCategory>, IServiceRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public ServiceRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<ServiceCategory> CreateAsync(ServiceCategory ServiceCategory)
        {
              _context.ServiceCategorys.Add(ServiceCategory);
            _context.SaveChanges();
            return Task.FromResult(ServiceCategory);
        }

        public Task<ServiceCategory> DeleteSoftAsync(ServiceCategory ServiceCategory)
        {
              _context.ServiceCategorys.Update(ServiceCategory);
            _context.SaveChanges();
            return Task.FromResult(ServiceCategory);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(ServiceCategoryDto ServiceCategoryDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (ServiceCategoryDto.StorageId > 0)
                whereEquals["storage_id"] = ServiceCategoryDto.StorageId;
            if (ServiceCategoryDto.Type > 0)
                whereEquals["type"] = ServiceCategoryDto.Type;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "services",
                        new[] { "id","code","name","type","storage_id","amount","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
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

        public Task<ServiceCategory> ShowAsync(int id)
        {
            return _context.ServiceCategorys.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<ServiceCategory> UpdateAsync(ServiceCategory ServiceCategory)
        {
              _context.ServiceCategorys.Update(ServiceCategory);
            _context.SaveChanges();
            return Task.FromResult(ServiceCategory);
        }
    }
}
