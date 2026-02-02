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
    public class PayrollPeriodRepositories : BaseRepository<PayrollPeriod>, IPayrollPeriodRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public PayrollPeriodRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<PayrollPeriod> CreateAsync(PayrollPeriod payrollPeriod)
        {
            _context.PayrollPeriods.Add(payrollPeriod);
            _context.SaveChanges();
            return Task.FromResult(payrollPeriod);
        }

        public Task<PayrollPeriod> DeleteSoftAsync(PayrollPeriod payrollPeriod)
        {
            _context.PayrollPeriods.Update(payrollPeriod);
            _context.SaveChanges();
            return Task.FromResult(payrollPeriod);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(PayrollPeriodDto payrollPeriodDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "payroll_periods",
                        new[] { "id","name","start_date","end_date","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
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
            whereEquals?.Clear(); whereLikes?.Clear(); whereDateRange?.Clear(); orderByList?.Clear();
            return _results;
        }

        public Task<PayrollPeriod> ShowAsync(int id)
        {
            return _context.PayrollPeriods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<PayrollPeriod> UpdateAsync(PayrollPeriod payrollPeriod)
        {
            _context.PayrollPeriods.Update(payrollPeriod);
            _context.SaveChanges();
            return Task.FromResult(payrollPeriod);
        }
    }
}
