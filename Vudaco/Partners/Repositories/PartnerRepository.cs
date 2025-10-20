using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Partners.Dtos;
using Vudaco.Partners.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Partners.Repositories
{
    public class PartnerRepository : BaseRepository<Partner>, IPartnerRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public PartnerRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<Partner> CreateAsync(Partner Partner)
        {
            _context.Partners.Add(Partner);
            _context.SaveChanges();
            return Task.FromResult(Partner);
        }

        public Task<Partner> DeleteSoftAsync(Partner Partner)
        {
            _context.Partners.Update(Partner);
            _context.SaveChanges();
            return Task.FromResult(Partner);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(PartnerDto PartnerDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id" };

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "partners",
                        new[] { "id", "code", "name", "address", "tax_code", "phone", "email", "bank_account", "allowed_debt_days", "max_debt", "is_supplier", "note", "storage_id", "abbreviation", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
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

        public Task<Partner> ShowAsync(int id)
        {
            return _context.Partners.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Partner> UpdateAsync(Partner Partner)
        {
            _context.Partners.Update(Partner);
            _context.SaveChanges();
            return Task.FromResult(Partner);
        }
    }
}
