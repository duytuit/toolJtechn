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
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "comments",
                        new[] { "id","content","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at"},
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
