

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.ContractFiles.Dtos;
using Vudaco.ContractFiles.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.ContractFiles.Repositories
{
    public class ContractFileDetailRepository :BaseRepository<FileInfoDetail>, IContractFileDetailRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public ContractFileDetailRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<FileInfoDetail> CreateAsync(FileInfoDetail FileInfoDetail)
        {
            throw new NotImplementedException();
        }

        public Task<FileInfoDetail> DeleteSoftAsync(FileInfoDetail FileInfoDetail)
        {
            _context.FileInfoDetails.Update(FileInfoDetail);
            _context.SaveChanges();
            return Task.FromResult(FileInfoDetail);
        }

        public async Task<PaginatedResultReact<object>> GetObjectFileNotDispatchAsync(FileInfoDetailDto FileInfoDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
              SELECT 
                    f.*
                FROM file_infos f
                LEFT JOIN partner_details p ON p.id = f.customer_detail_id
                WHERE 
                    f.deleted_at IS NULL
                    AND p.status = 1
                    AND p.deleted_at IS NULL
                    AND NOT EXISTS (
                        SELECT 1
                        FROM debits d
                        WHERE 
                            d.file_info_id = f.id
                            AND d.customer_detail_id = f.customer_detail_id
                            AND d.type = 0 
                            AND d.deleted_at IS NULL
                            AND d.employee_staff_id IS NULL
                    )";
            if (FileInfoDetailDto.StorageId > 0)
            {
                sql += $@" AND f.storage_id = {FileInfoDetailDto.StorageId}";
            }
            if (FileInfoDetailDto.FromDate.HasValue && FileInfoDetailDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = FileInfoDetailDto.ToDate.Value.Date.AddDays(1);
                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND f.accounting_date >= '{FileInfoDetailDto.FromDate.Value:yyyy-MM-dd}' 
                AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY f.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectNotServiceAsync(FileInfoDetailDto FileInfoDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var sql = $@"
                SELECT 
                    f.*,
                    fdt.price,
                    fdt.status,
                    fdt.employee_id,
                    ISNULL(rdt_total.total, 0) AS total
                FROM file_info_details fdt
                INNER JOIN file_infos f 
                    ON f.id = fdt.file_id
                LEFT JOIN partner_details p 
                    ON p.id = f.customer_detail_id
                OUTER APPLY (
                    SELECT SUM(rdt.amount * rdt.vat / 100) + SUM(rdt.amount) AS total
                    FROM receipts r
                    INNER JOIN receipt_details rdt 
                        ON rdt.receipt_id = r.id
                    WHERE r.file_info_id = f.id 
                    AND r.employee_id = fdt.employee_id
                ) rdt_total
                WHERE 
                    fdt.deleted_at IS NULL
                    AND f.deleted_at IS NULL
                    AND p.status = 1
                    AND p.deleted_at IS NULL
                    AND NOT EXISTS (
                        SELECT 1
                        FROM debits d
                        WHERE 
                            d.file_info_id = f.id
                            AND d.customer_detail_id = f.customer_detail_id
                            AND d.employee_staff_id = fdt.employee_id
                            AND d.type BETWEEN 1 AND 2
                            AND d.deleted_at IS NULL
                    )";
            if (FileInfoDetailDto.StorageId > 0) {

                sql += $@" AND fdt.storage_id = {FileInfoDetailDto.StorageId}";
            }
            if (FileInfoDetailDto.EmployeeId > 0)
            {
                sql += $@" AND fdt.employee_id = {FileInfoDetailDto.EmployeeId}";
            }
            if (FileInfoDetailDto.FromDate.HasValue && FileInfoDetailDto.ToDate.HasValue)
            {
                // Cộng thêm 1 ngày cho ToDate
                var toDateNext = FileInfoDetailDto.ToDate.Value.Date.AddDays(1);

                // Format chuẩn yyyy-MM-dd HH:mm:ss để SQL hiểu đúng
                sql += $@" AND f.accounting_date >= '{FileInfoDetailDto.FromDate.Value:yyyy-MM-dd}' 
                AND f.accounting_date < '{toDateNext:yyyy-MM-dd}'";
            }

            sql += " ORDER BY f.updated_at DESC";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
            };
            return _results;
        }
        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(FileInfoDetailDto FileInfoDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id" };

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "file_info_details",
                        new[] { "id","file_id","employee_id","price","storage_id","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
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

        public Task<FileInfoDetail> ShowAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<FileInfoDetail> UpdateAsync(FileInfoDetail FileInfoDetail)
        {
            throw new NotImplementedException();
        }
    }
}
