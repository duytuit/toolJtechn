using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Partners.Dtos;
using Vudaco.Partners.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.MysqlHelper;
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

            if (PartnerDto.Status > 0)
                whereEquals["status"] = PartnerDto.Status;
            if (PartnerDto.StorageId > 0)
                whereEquals["storage_id"] = PartnerDto.StorageId;

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "partners",
                        new[] { "id", "code", "name", "address", "tax_code", "phone", "email", "bank_account", "allowed_debt_days", "max_debt", "note", "storage_id", "abbreviation", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
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
                                        Name = "partner_details",
                                        Table = "partner_details",
                                        Columns = new[] { "id","code","partner_id","note","storage_id","status","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
                                        ParentKey = "id",
                                        ForeignKey = "partner_id",
                                        KeyName = "partner_id",
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

        public async Task<PaginatedResultReact<object>> GetPartnerDetail(PartnerDetailDto PartnerDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id" };

            if (PartnerDetailDto.Status > 0)
                whereEquals["status"] = PartnerDetailDto.Status;
            if (PartnerDetailDto.StorageId > 0)
                whereEquals["storage_id"] = PartnerDetailDto.StorageId;

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "partner_details",
                        new[] { "id","code","partner_id","note","storage_id","status","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
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
                                        Name = "partners",
                                        Table = "partners",
                                        Columns = new[] { "id", "code", "name", "address", "tax_code", "phone", "email", "bank_account", "allowed_debt_days", "max_debt", "note", "storage_id", "abbreviation", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at"},
                                        ParentKey = "partner_id",
                                        ForeignKey = "id",
                                        KeyName = "id",
                                        IsCollection = false,
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

        public async Task<PaginatedResultReact<object>> GetPartnerKHAndNCCDetail(PartnerDetailDto PartnerDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                   SELECT *
                    FROM partners p
                    WHERE EXISTS (
                        SELECT 1 FROM partner_details d1
                        WHERE d1.partner_id = p.id AND d1.status = 1
                    )
                    AND EXISTS (
                        SELECT 1 FROM partner_details d2
                        WHERE d2.partner_id = p.id AND d2.status = 2
                    )
                    AND p.deleted_at is null";
            if (PartnerDetailDto.StorageId > 0)
            {
                sql += $@" AND p.storage_id = {PartnerDetailDto.StorageId}";
            }
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetPartnerWithDebitNoBill(PartnerDetailDto PartnerDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             var sql = $@"
                    SELECT 
                        p.id AS customer_detail_id,
                        dt.abbreviation,
                        dt.name,
                        p.customer_credit_limit_month,
                        MIN(d.accounting_date) AS oldest_accounting_date
                    FROM partner_details p
                    LEFT JOIN debits d
                        ON d.customer_detail_id = p.id
                    AND d.deleted_at IS NULL
                    AND d.bill_id IS NULL
                    AND d.type IN (0,1,2,3,4,5,6,8)
                    AND (
                            d.status = 2 
                            OR (d.status = 0 AND d.file_info_id IS NULL)
                        )
                    AND (
                            d.service_id NOT IN (19,33)
                            OR (d.service_id = 33 AND d.service_status > 2)
                            OR d.service_id IS NULL
                        )";
                    if (PartnerDetailDto.StorageId > 0)
                    {
                        sql += $@" AND d.storage_id = {PartnerDetailDto.StorageId}";
                    }
                    sql += $@" LEFT JOIN file_infos f
                        ON f.id = d.file_info_id
                    AND f.deleted_at IS NULL
                    LEFT JOIN partners dt
                        ON dt.id = p.partner_id
                    WHERE p.status = 1
                    AND p.deleted_at IS NULL";
              sql += $@" GROUP BY p.id, dt.abbreviation, p.customer_credit_limit_month, dt.name ORDER BY CASE WHEN MIN(d.accounting_date) IS NULL THEN 1 ELSE 0 END, MIN(d.accounting_date) DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }

        public async Task<Partner> ShowAsync(int id)
        {
            var entity = await _context.Partners.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return null;
            entity.PartnerDetails = await _context.PartnerDetails.Where(d => d.PartnerId == entity.Id).ToListAsync();
            return entity;
        }

        public Task<Partner> UpdateAsync(Partner Partner)
        {
            _context.Partners.Update(Partner);
            _context.SaveChanges();
            return Task.FromResult(Partner);
        }
       
    }
}
