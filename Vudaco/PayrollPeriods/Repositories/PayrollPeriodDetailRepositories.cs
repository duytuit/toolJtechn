using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.PayrollPeriods.Dtos;
using Vudaco.PayrollPeriods.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.PayrollPeriods.Repositories
{
    public class PayrollPeriodDetailRepositories : BaseRepository<PayrollPeriodDetail>, IPayrollPeriodDetailRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public PayrollPeriodDetailRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<PayrollPeriodDetail> CreateAsync(PayrollPeriodDetail payrollPeriodDetail)
        {
            _context.PayrollPeriodDetails.Add(payrollPeriodDetail);
            _context.SaveChanges();
            return Task.FromResult(payrollPeriodDetail);
        }

        public Task<PayrollPeriodDetail> DeleteSoftAsync(PayrollPeriodDetail payrollPeriodDetail)
        {
            _context.PayrollPeriodDetails.Update(payrollPeriodDetail);
            _context.SaveChanges();
            return Task.FromResult(payrollPeriodDetail);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(PayrollPeriodDetailDto payrollPeriodDetailDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            page = page <= 0 ? 1 : page;
             pageSize = pageSize <= 0 ? 500 : pageSize;
             int offset = (page - 1) * pageSize;
             var sql = $@"
                 SELECT d.* FROM payroll_period_details d
                 WHERE d.deleted_at IS NULL";
            if (payrollPeriodDetailDto.StorageId > 0)
            {
                sql += $@" AND d.storage_id = {payrollPeriodDetailDto.StorageId}";
            }
            // 👉 ORDER + PAGINATION
            sql += $@"
                ORDER BY d.updated_at DESC
                OFFSET {offset} ROWS
                FETCH NEXT {pageSize} ROWS ONLY";
            var results = await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
            var _results = new PaginatedResultReact<object>
            {
                Data = results,
                PageNum = page,
                PageSize = pageSize
            };
            return _results;
        }

        public Task<PayrollPeriodDetail> ShowAsync(int id)
        {
            return _context.PayrollPeriodDetails.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<PayrollPeriodDetail> UpdateAsync(PayrollPeriodDetail payrollPeriodDetail)
        {
            _context.PayrollPeriodDetails.Update(payrollPeriodDetail);
            _context.SaveChanges();
            return Task.FromResult(payrollPeriodDetail);
        }
    }
}
