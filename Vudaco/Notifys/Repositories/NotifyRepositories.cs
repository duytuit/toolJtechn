using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Notifys.Dtos;
using Vudaco.Notifys.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Notifys.Repositories
{
    public class NotifyRepositories : BaseRepository<Notify>, INotifyRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public NotifyRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Notify> CreateAsync(Notify notify)
        {
            _context.Notifys.Add(notify);
            _context.SaveChanges();
            return Task.FromResult(notify);
        }

        public Task<Notify> DeleteSoftAsync(Notify notify)
        {
            _context.Notifys.Update(notify);
            _context.SaveChanges();
            return Task.FromResult(notify);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(NotifyDto notifyDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
             if (notifyDto.StorageId > 0)
            {
                whereEquals.Add("storage_id", notifyDto.StorageId);
            }
            if (notifyDto.EmployeeId > 0)
            {
                whereEquals.Add("employee_id", notifyDto.EmployeeId);
            }
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "notifys",
                        new[] { "id","storage_id","employee_id","title","description","status","type","image","created_by","created_at","updated_by","updated_at","deleted_by","deleted_at"},
                        offset: (page - 1) * pageSize,
                        limit: pageSize,
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

        public Task<Notify> ShowAsync(int id)
        {
            return _context.Notifys.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Notify> UpdateAsync(Notify notify)
        {
            _context.Notifys.Update(notify);
            _context.SaveChanges();
            return Task.FromResult(notify);
        }
    }
}
