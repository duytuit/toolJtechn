using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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
    public class OffsetRepositories : BaseRepository<Offset>, IOffsetRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        //================Offset=============================
        public const int ChuyenTienNoiBo = 0;
        public const int BuTruCongNo = 1;
        public OffsetRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Offset> CreateAsync(Offset Offset)
        {
            throw new NotImplementedException();
        }

        public Task<Offset> DeleteSoftAsync(Offset Offset)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(OffsetDto OffsetDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "updated_at desc", "id" };
            if (OffsetDto.StorageId > 0)
                whereEquals["storage_id"] = OffsetDto.StorageId;
            if (OffsetDto.Type > 0)
                whereEquals["type"] = OffsetDto.Type;
            if (OffsetDto.FromDate.HasValue && OffsetDto.ToDate.HasValue)
                whereDateRange.Add(("accounting_date", OffsetDto.FromDate.Value, OffsetDto.ToDate.Value.AddDays(1)));
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "off_sets",
                        new[] { "id","a_receipt_id","b_receipt_id","accounting_date","note","price","type","storage_id","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at" },
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
                                    Name = "receipts",
                                    Table = "receipts",
                                    Columns = new[] { "id","code_receipt","offset_id","storage_id","income_expense_category_id","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                                    ParentKey = "id",
                                    ForeignKey = "offset_id",
                                    KeyName = "offset_id",
                                    IsCollection = true
                                }
                            },
                        redisCache: _redis,
                        includeCount: false,
                        cancellationToken: cancellationToken
                    );
            var objectList = new List<object>();
            objectList.AddRange(results.Data);
            var _results = new PaginatedResultReact<object>
            {
                Data = objectList,
            };
            objectList = null;
            results = null;
            whereEquals?.Clear(); whereLikes?.Clear(); whereDateRange?.Clear(); orderByList?.Clear();
            return _results;
        }

        public async Task<Offset> ShowAsync(int id)
        {
            return await _context.Offsets.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Offset> UpdateAsync(Offset Offset)
        {
            throw new NotImplementedException();
        }
    }
}
