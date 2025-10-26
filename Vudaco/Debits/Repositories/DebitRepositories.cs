using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Debits.Dtos;
using Vudaco.Debits.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Debits.Repositories
{
    public class DebitRepositories : BaseRepository<Debit>, IDebitRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public DebitRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Debit> CreateAsync(Debit Debit)
        {
              _context.Debits.Add(Debit);
            _context.SaveChanges();
            return Task.FromResult(Debit);
        }

        public Task<Debit> DeleteSoftAsync(Debit Debit)
        {
            _context.Debits.Update(Debit);
            _context.SaveChanges();
            return Task.FromResult(Debit);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(DebitDto DebitDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (DebitDto.StorageId > 0)
                whereEquals["storage_id"] = DebitDto.StorageId;
            if (DebitDto.PartnerDetailId > 0)
                whereEquals["partner_detail_id"] = DebitDto.PartnerDetailId;
            if (DebitDto.FromDate.HasValue && DebitDto.ToDate.HasValue)
                whereDateRange.Add(("accounting_date", DebitDto.FromDate.Value, DebitDto.ToDate.Value));
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "debits",
                        new[] { "id","bill_id","vehicle_dispatch_id","partner_detail_id","file_info_id","storage_id","type","name","accounting_date","purchase_price","price","vat","status","data","note","approved_by_user","approval_time","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at" },
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

        public Task<Debit> ShowAsync(int id)
        {
             return _context.Debits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Debit> UpdateAsync(Debit Debit)
        {
            _context.Debits.Update(Debit);
            _context.SaveChanges();
            return Task.FromResult(Debit);
        }
    }
}
