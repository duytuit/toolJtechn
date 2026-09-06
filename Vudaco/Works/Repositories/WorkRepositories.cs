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
    public class WorkRepositories : BaseRepository<Work>, IWorkRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public WorkRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Work> CreateAsync(Work Work)
        {
            _context.Works.Add(Work);
            _context.SaveChanges();
            return Task.FromResult(Work);
        }

        public Task<Work> DeleteSoftAsync(Work Work)
        {
            _context.Works.Update(Work);
            _context.SaveChanges();
            return Task.FromResult(Work);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(WorkListDto WorkListDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            List<(string Sql, object[] Params)> whereCustoms = new();
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "updated_at desc", "id" };

            if (WorkListDto.Id > 0)
                whereEquals["id"] = WorkListDto.Id;
            if (WorkListDto.ParentId > 0)
                whereEquals["parent_id"] = WorkListDto.ParentId;
            if (WorkListDto.Status > 0)
                whereEquals["status"] = WorkListDto.Status;
            if (WorkListDto.StorageId > 0)
                whereEquals["storage_id"] = WorkListDto.StorageId;
            if (WorkListDto.CreatedBy > 0)
                whereEquals["created_by"] = WorkListDto.CreatedBy;
            if (WorkListDto.UpdatedBy > 0)
                whereEquals["updated_by"] = WorkListDto.UpdatedBy;
            if (!string.IsNullOrWhiteSpace(WorkListDto.Name))
                whereLikes["name"] = WorkListDto.Name;
            if (WorkListDto.FromDate.HasValue || WorkListDto.ToDate.HasValue)
                whereDateRange.Add(("created_at", WorkListDto.FromDate ?? DateTime.MinValue, WorkListDto.ToDate?.AddDays(1) ?? DateTime.MaxValue));

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                _configuration.GetConnectionString("DefaultConnection"),
                "works",
                new[] { "id", "name", "description", "type", "group", "parent_id", "storage_id", "status", "assignee_ids", "attachments", "due_date", "completed_date", "priority", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
                offset: null,
                limit: null,
                whereEquals: whereEquals,
                whereLikes: whereLikes,
                dateRangeList: whereDateRange,
                orderByList: orderByList,
                whereCustom: whereCustoms,
                relations: new List<AdoRelation>
                {
                    new AdoRelation
                    {
                        Name = "work_details",
                        Table = "work_details",
                        Columns = new[] { "id", "work_id", "name", "description", "storage_id", "checked", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
                        ParentKey = "id",
                        ForeignKey = "work_id",
                        KeyName = "work_id",
                        IsCollection = true
                    }
                },
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

        public async Task<Work> ShowAsync(int id)
        {
            var work = await _context.Works
                .FirstOrDefaultAsync(x => x.Id == id);

            if (work == null)
            {
                return null;
            }

            work.ChildWorks = await _context.Works
                .Where(x => x.ParentId == work.Id)
                .ToListAsync();

            var workIds = work.ChildWorks
                .Select(x => x.Id)
                .Append(work.Id)
                .ToList();

            var workDetails = await _context.WorkDetails
                .Where(x => workIds.Contains(x.WorkId))
                .ToListAsync();

            work.WorkDetails = workDetails
                .Where(x => x.WorkId == work.Id)
                .ToList();

            foreach (var childWork in work.ChildWorks)
            {
                childWork.WorkDetails = workDetails
                    .Where(x => x.WorkId == childWork.Id)
                    .ToList();
            }

            return work;
        }

        public Task<Work> UpdateAsync(Work Work)
        {
            _context.Works.Update(Work);
            _context.SaveChanges();
            return Task.FromResult(Work);
        }
    }
}
