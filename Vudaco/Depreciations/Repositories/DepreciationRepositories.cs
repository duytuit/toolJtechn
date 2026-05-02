using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Depreciations.Dtos;
using Vudaco.Depreciations.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Depreciations.Repositories
{
    public class DepreciationRepositories : BaseRepository<Depreciation>, IDepreciationRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public DepreciationRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Depreciation> CreateAsync(Depreciation Depreciation)
        {
            throw new NotImplementedException();
        }

        public Task<Depreciation> DeleteSoftAsync(Depreciation Depreciation)
        {
              _context.Depreciations.Update(Depreciation);
              _context.SaveChanges();
              return Task.FromResult(Depreciation);
        }

        public async Task<PaginatedResultReact<object>> GetDepreciationAllocationAsync(DepreciationAllocationDto DepreciationAllocationDto, int page, int pageSize, CancellationToken cancellationToken)
        {
              page = page <= 0 ? 1 : page;
             pageSize = pageSize <= 0 ? 500 : pageSize;
             int offset = (page - 1) * pageSize;
             var sql = $@"
                   SELECT 
                    d.*,
                    COALESCE(dt.total_depreciation, 0) AS total_depreciation
                FROM depreciation_allocations d
                LEFT JOIN (
                    SELECT 
                        depreciation_allocation_id,
                        SUM(monthly_depreciation) AS total_depreciation
                    FROM depreciation_allocation_details
                    WHERE deleted_at IS NULL
                    GROUP BY depreciation_allocation_id
                ) dt ON d.id = dt.depreciation_allocation_id
                WHERE d.deleted_at IS NULL";
            if (DepreciationAllocationDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DepreciationAllocationDto.StorageId}";
            }
            if (DepreciationAllocationDto.Type > 0)
            {
                sql += $@" AND d.type = {DepreciationAllocationDto.Type}";
            }
            // 👉 ORDER + PAGINATION
            // sql += $@"
            //     ORDER BY d.updated_at DESC
            //     OFFSET {offset} ROWS
            //     FETCH NEXT {pageSize} ROWS ONLY";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
                PageNum = page,
                PageSize = pageSize
            };
            return _results;
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(DepreciationDto DepreciationDto, int page, int pageSize, CancellationToken cancellationToken)
        {
             page = page <= 0 ? 1 : page;
             pageSize = pageSize <= 0 ? 500 : pageSize;
             int offset = (page - 1) * pageSize;
             var sql = $@"
                   SELECT 
                    d.*,
                    COALESCE(dt.total_depreciation, 0) AS total_depreciation
                FROM depreciations d
                LEFT JOIN (
                    SELECT 
                        depreciation_id,
                        SUM(monthly_depreciation) AS total_depreciation
                    FROM depreciation_allocation_details
                    WHERE deleted_at IS NULL
                    GROUP BY depreciation_id
                ) dt ON d.id = dt.depreciation_id
                WHERE d.deleted_at IS NULL";
            if (DepreciationDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {DepreciationDto.StorageId}";
            }
            if (DepreciationDto.Type > 0)
            {
                sql += $@" AND d.type = {DepreciationDto.Type}";
            }
            // 👉 ORDER + PAGINATION
            // sql += $@"
            //     ORDER BY d.updated_at DESC
            //     OFFSET {offset} ROWS
            //     FETCH NEXT {pageSize} ROWS ONLY";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
                PageNum = page,
                PageSize = pageSize
            };
            return _results;
        }

        public Task<Depreciation> ShowAsync(int id)
        {
             return _context.Depreciations.FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Depreciation> UpdateAsync(Depreciation Depreciation)
        {
              _context.Depreciations.Update(Depreciation);
              _context.SaveChanges();
              return Task.FromResult(Depreciation);
        }
    }
}
