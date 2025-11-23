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
using Vudaco.Shares.MysqlHelper;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Receipts.Repositories
{
    public class ReceiptRepositories : BaseRepository<Receipt>, IReceiptRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public const int ThuKH = 0;
        public const int ChiGiaoNhan = 1;
        public const int ChiHoanUngGiaoNhan = 2;
        public const int ThuHoanUngGiaoNhan = 3;
        public const int ChiMuaHangNCC = 4;
        public const int ThuBanHangKH = 5;
        public const int ThuBanHangNV = 6;
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
            if (ReceiptDto.EmployeeId > 0)
                whereEquals["employee_id"] = ReceiptDto.EmployeeId;
            whereEquals["type_receipt"] = ReceiptDto.TypeReceipt;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "receipts",
                        new[] { "id","code_receipt","storage_id","partner_detail_id","accounting_date","employee_id","file_info_id","fund_id","income_expense_category_id","bill","note","description","form_of_payment","type_receipt","type","bank_id","status","data","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at" },
                        offset: null,
                        limit: null,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        relations: new List<AdoRelation>
                                {
                                    new AdoRelation
                                    {
                                        Name = "receipt_details",
                                        Table = "receipt_details",
                                        Columns = new[] { "id","receipt_id","storage_id","debit_id","accounting_date","amount","vat","data","note","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                                        ParentKey = "id",
                                        ForeignKey = "receipt_id",
                                        KeyName = "receipt_id",
                                        IsCollection = true,
                                    }
                                },
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
         public async Task<PaginatedResultReact<object>> GetXacNhanChiPhiGiaoNhanAsync(ReceiptDto ReceiptDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             List<(string Sql, object[] Params)> whereCustoms = new();
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (ReceiptDto.StorageId > 0)
                whereEquals["storage_id"] = ReceiptDto.StorageId;
            if (ReceiptDto.EmployeeId > 0)
                whereEquals["employee_id"] = ReceiptDto.EmployeeId;

            whereCustoms.Add(("type_receipt IN (2,3)", Array.Empty<object>()));

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "receipts",
                        new[] { "id","code_receipt","storage_id","partner_detail_id","accounting_date","employee_id","file_info_id","fund_id","income_expense_category_id","bill","note","description","form_of_payment","type_receipt","type","bank_id","status","data","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at" },
                        offset: null,
                        limit: null,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        whereCustom:whereCustoms,
                        relations: new List<AdoRelation>
                                {
                                    new AdoRelation
                                    {
                                        Name = "receipt_details",
                                        Table = "receipt_details",
                                        Columns = new[] { "id","receipt_id","storage_id","debit_id","accounting_date","amount","vat","data","note","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                                        ParentKey = "id",
                                        ForeignKey = "receipt_id",
                                        KeyName = "receipt_id",
                                        IsCollection = true,
                                    }
                                },
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

        public async Task<Receipt> ShowAsync(int id)
        {
            var entity = await _context.Receipts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null) return null;

            entity.ReceiptDetails = await _context.ReceiptDetails
                .AsNoTracking()
                .Where(d => d.ReceiptId == id)
                .ToListAsync();

            return entity;
        }

        public Task<Receipt> UpdateAsync(Receipt Receipt)
        {
             _context.Receipts.Update(Receipt);
            _context.SaveChanges();
            return Task.FromResult(Receipt);
        }

        public async Task<ReceiptDetail> ShowWithDebitAsync(int id)
        {
            var entity = await _context.ReceiptDetails
                .FirstOrDefaultAsync(x => x.ReceiptId == id);

            if (entity == null) return null;

            entity.Debit = await _context.Debits
                .Where(d => d.Id == entity.DebitId)
                .FirstOrDefaultAsync();

            return entity;
        }
    }
}
