using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Employees.Models;
using Vudaco.Partners.Dtos;
using Vudaco.Partners.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Partners.Repositories
{
    public class PartnerDetailRepository : BaseRepository<PartnerDetail>, IPartnerDetailRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public PartnerDetailRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<PartnerDetail> CreateAsync(PartnerDetail PartnerDetail)
        {
            _context.PartnerDetails.Add(PartnerDetail);
            _context.SaveChanges();
            return Task.FromResult(PartnerDetail);
        }

        public Task<PartnerDetail> DeleteSoftAsync(PartnerDetail PartnerDetail)
        {
            _context.PartnerDetails.Update(PartnerDetail);
            _context.SaveChanges();
            return Task.FromResult(PartnerDetail);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(PartnerDetailDto PartnerDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id" };

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "partner_details",
                        new[] { "id", "code", "partner_id", "status", "note", "storage_id", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at"},
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

        public Task<PartnerDetail> ShowAsync(int id)
        {
            return _context.PartnerDetails.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<PartnerDetail> UpdateAsync(PartnerDetail PartnerDetail)
        {
            _context.PartnerDetails.Update(PartnerDetail);
            _context.SaveChanges();
            return Task.FromResult(PartnerDetail);
        }
        public async Task<PartnerDetail> GetPartnerInfoByIdWithCacheAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            string cacheKey = $"GetPartnerInfoById_{id}";

            // 1️⃣ GET CACHE
            var cacheValue = await _redis.GetAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrEmpty(cacheValue))
            {
                return JsonSerializer.Deserialize<PartnerDetail>(cacheValue);
            }
            // 2️⃣ GET DB
            var entity = await _context.PartnerDetails.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            if (entity == null)
                return null;
            entity.Partner = await _context.Partners.FirstOrDefaultAsync(d => d.Id == entity.PartnerId);
            // 3️⃣ SET CACHE
            var json = JsonSerializer.Serialize(entity);
            await _redis.SetAsync(
                cacheKey,
                json,
                TimeSpan.FromDays(1),
                cancellationToken
            );
            return entity;
        }
    }
}
