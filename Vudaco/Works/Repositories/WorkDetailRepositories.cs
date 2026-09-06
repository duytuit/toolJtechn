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
    public class WorkDetailRepositories : BaseRepository<WorkDetail>, IWorkDetailRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public WorkDetailRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<WorkDetail> CreateAsync(WorkDetail WorkDetail)
        {
            _context.WorkDetails.Add(WorkDetail);
            _context.SaveChanges();
            return Task.FromResult(WorkDetail);
        }

        public Task<WorkDetail> DeleteSoftAsync(WorkDetail WorkDetail)
        {
              _context.WorkDetails.Update(WorkDetail);
            _context.SaveChanges();
            return Task.FromResult(WorkDetail);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(CheckListDto CheckListDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            List<(string Sql, object[] Params)> whereCustoms = new();
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "updated_at desc", "id" };

            if (CheckListDto.Id > 0)
                whereEquals["id"] = CheckListDto.Id;
            if (CheckListDto.StorageId > 0)
                whereEquals["storage_id"] = CheckListDto.StorageId;
            if (CheckListDto.CreatedBy > 0)
                whereEquals["created_by"] = CheckListDto.CreatedBy;
            if (CheckListDto.UpdatedBy > 0)
                whereEquals["updated_by"] = CheckListDto.UpdatedBy;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                _configuration.GetConnectionString("DefaultConnection"),
                "work_details",
                new[] { "id", "work_id", "name", "description", "storage_id", "checked", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
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

        public async Task<WorkDetail> ShowAsync(int id)
        {
            var workDetail = await _context.WorkDetails
                .FirstOrDefaultAsync(x => x.Id == id);

            if (workDetail == null)
            {
                return null;
            }

            return workDetail;
        }

        public Task<WorkDetail> UpdateAsync(WorkDetail WorkDetail)
        {
            _context.WorkDetails.Update(WorkDetail);
            _context.SaveChanges();
            return Task.FromResult(WorkDetail);
        }
        public async Task<List<WorkDetail>> GetByModelId(int id, string model, CancellationToken cancellationToken)
        {
            var workDetails = await _context.WorkDetails
                .ToListAsync(cancellationToken);
            return workDetails;
        }
    }
}