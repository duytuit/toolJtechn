using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.FormRequests.Dtos;
using Vudaco.FormRequests.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.FormRequests.Repositories
{
    public class FormRequestRepositories : BaseRepository<FormRequest>, IFormRequestRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public FormRequestRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<FormRequest> CreateAsync(FormRequest formRequest)
        {
            _context.FormRequests.Add(formRequest);
            _context.SaveChanges();
            return Task.FromResult(formRequest);
        }

        public Task<FormRequest> DeleteSoftAsync(FormRequest formRequest)
        {
            _context.FormRequests.Update(formRequest);
            _context.SaveChanges();
            return Task.FromResult(formRequest);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(FormRequestDto formRequestDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (formRequestDto.StorageId > 0)
            {
                whereEquals.Add("storage_id", formRequestDto.StorageId);
            }
            if (formRequestDto.EmployeeId > 0)
            {
                whereEquals.Add("employee_id", formRequestDto.EmployeeId);
            }
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "form_requests",
                        new[] { "id","storage_id","employee_id","description","confirm_by","confirm_at","status","type","note","total_day_has_paid_leave","total_day_no_paid_leave","created_by","created_at","updated_by","updated_at","deleted_by","deleted_at"},
                        offset: (page - 1) * pageSize,
                        limit: pageSize,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        redisCache: _redis,
                        includeCount: true,
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

        public Task<FormRequest> ShowAsync(int id)
        {
            return _context.FormRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<FormRequest> UpdateAsync(FormRequest formRequest)
        {
            _context.FormRequests.Update(formRequest);
            _context.SaveChanges();
            return Task.FromResult(formRequest);
        }
    }
}
