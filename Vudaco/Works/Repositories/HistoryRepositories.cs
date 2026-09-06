using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Works.Dtos;
using Vudaco.Works.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;
using Vudaco.Shares.MysqlHelper;

namespace Vudaco.Works.Repositories
{
    public class HistoryRepositories : BaseRepository<WorkHistory>, IHistoryRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public HistoryRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<WorkHistory> CreateAsync(WorkHistory WorkHistory)
        {
              _context.WorkHistories.Add(WorkHistory);
            _context.SaveChanges();
            return Task.FromResult(WorkHistory);
        }

        public Task<WorkHistory> DeleteSoftAsync(WorkHistory WorkHistory)
        {
              _context.WorkHistories.Update(WorkHistory);
            _context.SaveChanges();
            return Task.FromResult(WorkHistory);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(WorkHistoryDto WorkHistoryDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            List<(string Sql, object[] Params)> whereCustoms = new();
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "updated_at desc", "id" };

            if (WorkHistoryDto.Id > 0)
                whereEquals["id"] = WorkHistoryDto.Id;
            if (WorkHistoryDto.StorageId > 0)
                whereEquals["storage_id"] = WorkHistoryDto.StorageId;
            if (WorkHistoryDto.CreatedBy > 0)
                whereEquals["created_by"] = WorkHistoryDto.CreatedBy;
            if (WorkHistoryDto.UpdatedBy > 0)
                whereEquals["updated_by"] = WorkHistoryDto.UpdatedBy;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                _configuration.GetConnectionString("DefaultConnection"),
                "works_history",
                new[] { "id", "work_id", "name", "description", "type", "group", "parent_id", "storage_id", "status", "assignee_ids", "attachments", "due_date", "completed_date", "priority", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
                offset: null,
                limit: null,
                whereEquals: whereEquals,
                whereLikes: whereLikes,
                dateRangeList: whereDateRange,
                orderByList: orderByList,
                whereCustom: whereCustoms,
                redisCache: _redis,
                includeCount: false,
                cancellationToken: cancellationToken);

            var objectList = new List<object>();
            objectList.AddRange(results.Data);
            return new PaginatedResultReact<object>
            {
                PageNum = page,
                PageSize = pageSize,
                First = (int)Math.Ceiling((double)results.Count / pageSize),
                Total = results.Count,
                Data = objectList
            };
        }

        public async Task<WorkHistory> ShowAsync(int id)
        {
            var workHistory = await _context.WorkHistories  
                .FirstOrDefaultAsync(x => x.Id == id);

            if (workHistory == null)
            {
                return null;
            }

            return workHistory;
        }

        public Task<WorkHistory> UpdateAsync(WorkHistory WorkHistory)
        {
            _context.WorkHistories.Update(WorkHistory);
            _context.SaveChanges();
            return Task.FromResult(WorkHistory);
        }
        public async Task<List<WorkHistory>> GetByModelId(int id, string model, CancellationToken cancellationToken)
        {
            var workHistories = await _context.WorkHistories
                .Where(x => x.ModelId == id && x.Model == model)
                .ToListAsync(cancellationToken);
            return workHistories;
        }
    }
}


         