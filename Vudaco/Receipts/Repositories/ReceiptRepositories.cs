using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Receipts.Dtos;
using Vudaco.Receipts.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Receipts.Repositories
{
    public class ReceiptRepositories : BaseRepository<Receipt>, IReceiptRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public ReceiptRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Receipt> CreateAsync(Receipt Receipt)
        {
               _context.Receipts.Add(Receipt);
            _context.SaveChanges();
            return Task.FromResult(Receipt);
        }

        public Task<Receipt> DeleteSoftAsync(Receipt Receipt)
        {
                _context.Receipts.Update(Receipt);
            _context.SaveChanges();
            return Task.FromResult(Receipt);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (ReceiptDto.StorageId > 0)
                whereEquals["storage_id"] = ReceiptDto.StorageId;
                whereEquals["type"] = ReceiptDto.Type;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "receipts",
                        new[] { "id","code_receipt","storage_id","partner_detail_id","accounting_date","employee_id","file_info_id","code_fund","code","bill","note","description","form_of_payment","type_receipt","type","account_number","bank_name","branch_name","account_holder","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
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

        public Task<Receipt> ShowAsync(int id)
        {
             return _context.Receipts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Receipt> UpdateAsync(Receipt Receipt)
        {
             _context.Receipts.Update(Receipt);
            _context.SaveChanges();
            return Task.FromResult(Receipt);
        }
    }
}
