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
    public class CommentRepositories : BaseRepository<WorkComment>, ICommentRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public CommentRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<WorkComment> CreateAsync(WorkComment WorkComment)
        {
            _context.WorkComments.Add(WorkComment);
            _context.SaveChanges();
            return Task.FromResult(WorkComment);
        }

        public Task<WorkComment> DeleteSoftAsync(WorkComment WorkComment)
        {
              _context.WorkComments.Update(WorkComment);
            _context.SaveChanges();
            return Task.FromResult(WorkComment);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(WorkCommentDto WorkCommentDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            List<(string Sql, object[] Params)> whereCustoms = new();
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "updated_at desc", "id" };

            if (WorkCommentDto.Id > 0)
                whereEquals["id"] = WorkCommentDto.Id;
            if (WorkCommentDto.ParentId > 0)
                whereEquals["parent_id"] = WorkCommentDto.ParentId;
            if (WorkCommentDto.StorageId > 0)
                whereEquals["storage_id"] = WorkCommentDto.StorageId;
            if (WorkCommentDto.CreatedBy > 0)
                whereEquals["created_by"] = WorkCommentDto.CreatedBy;
            if (WorkCommentDto.UpdatedBy > 0)
                whereEquals["updated_by"] = WorkCommentDto.UpdatedBy;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                _configuration.GetConnectionString("DefaultConnection"),
                "work_comments",
                new[] { "id", "parent_id", "status", "storage_id", "created_by", "updated_by", "deleted_by", "deleted_at", "created_at", "updated_at" },
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

        public async Task<WorkComment> ShowAsync(int id)
        {
            var workComment = await _context.WorkComments
                .FirstOrDefaultAsync(x => x.Id == id);

            if (workComment == null)
            {
                return null;
            }

            return workComment;
        }

        public Task<WorkComment> UpdateAsync(WorkComment WorkComment)
        {
            _context.WorkComments.Update(WorkComment);
            _context.SaveChanges();
            return Task.FromResult(WorkComment);
        }
        public async Task<List<WorkComment>> GetByModelId(int id, string model, CancellationToken cancellationToken)
        {
            var workComments = await _context.WorkComments
                .Where(x => x.ModelId == id && x.Model == model)
                .ToListAsync(cancellationToken);
            return workComments;
        }
    }
}