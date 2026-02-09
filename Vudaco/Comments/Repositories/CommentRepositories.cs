using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Comments.Dtos;
using Vudaco.Comments.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.MysqlHelper;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Comments.Repositories
{
    public class CommentRepositories : BaseRepository<Comment>, ICommentRepositories
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

        public Task<Comment> CreateAsync(Comment Comment)
        {
            _context.Comments.Add(Comment);
            _context.SaveChanges();
            return Task.FromResult(Comment);
        }

        public Task<Comment> DeleteSoftAsync(Comment Comment)
        {
            _context.Comments.Update(Comment);
            _context.SaveChanges();
            return Task.FromResult(Comment);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(CommentDto CommentDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (CommentDto.StorageId > 0)
            {
                whereEquals.Add("storage_id", CommentDto.StorageId);
            }
            if (CommentDto.EmployeeId > 0)
            {
                whereEquals.Add("employee_id", CommentDto.EmployeeId);
            }
            if (CommentDto.PostId > 0)
            {
                whereEquals.Add("post_id", CommentDto.PostId);
            }
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "comments",
                        new[] { "id","storage_id","post_id","type","message","attach","parent_id","employee_id","created_by","created_at","updated_by","updated_at","deleted_by","deleted_at"},
                        offset: (page - 1) * pageSize,
                        limit: pageSize,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        relations: new List<AdoRelation>
                                {
                                    new AdoRelation
                                    {
                                        Name = "employee",
                                        Table = "employees",
                                        Columns = new[] { "id","first_name","last_name","storage_id","deleted_at","avatar","phone","email","user_id"},
                                        ParentKey = "employee_id",
                                        ForeignKey = "id",
                                        KeyName = "id",
                                        IsCollection = false
                                    },
                                },
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

        public Task<Comment> ShowAsync(int id)
        {
            return _context.Comments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Comment> UpdateAsync(Comment Comment)
        {
            _context.Comments.Update(Comment);
            _context.SaveChanges();
            return Task.FromResult(Comment);
        }
    }
}
