using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Departments.Dtos;
using Vudaco.Departments.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Departments.Repositories
{
    public class DepartmentRepositories : BaseRepository<Department>, IDepartmentRepositories
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public DepartmentRepositories(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }

        public Task<Department> CreateAsync(Department Department)
        {
              _context.Departments.Add(Department);
            _context.SaveChanges();
            return Task.FromResult(Department);
        }

        public Task<Department> DeleteSoftAsync(Department Department)
        {
              _context.Departments.Update(Department);
            _context.SaveChanges();
            return Task.FromResult(Department);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(DepartmentDto DepartmentDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            // if (DepartmentDto.StorageId > 0)
            //     whereEquals["storage_id"] = DepartmentDto.StorageId;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "departments",
                        new[] { "id","code","name","parent_id","status","storage_id","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at","permissions"},
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
            objectList = null;
            results = null;
            whereEquals?.Clear(); whereLikes?.Clear(); whereDateRange?.Clear(); orderByList?.Clear();
            return _results;
        }

        public Task<Department> ShowAsync(int id)
        {
            return _context.Departments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Department> UpdateAsync(Department Department)
        {
              _context.Departments.Update(Department);
            _context.SaveChanges();
            return Task.FromResult(Department);
        }
    }
}
