using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Employees.Dtos;
using Vudaco.Employees.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Employees.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public EmployeeRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<Employee> CreateAsync(Employee Employee)
        {
            _context.Employees.Add(Employee);
            _context.SaveChanges();
            return Task.FromResult(Employee);
        }

        public Task<Employee> DeleteSoftAsync(Employee Employee)
        {
            _context.Employees.Update(Employee);
            _context.SaveChanges();
            return Task.FromResult(Employee);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(EmployeeDto EmployeeDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id" };

            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "employees",
                        new[] { "id", "code", "first_name", "last_name", "identity_card", "native_land", "addresss", "birthday", "status", "marital", "worker", "positions", "begin_date_company", "end_date_company", "storage_id", "created_by", "updated_by", "deleted_by", "created_at", "updated_at", "deleted_at", "avatar", "phone", "email", "bank_number", "bank_name", "user_id" },
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

        public Task<Employee> ShowAsync(int id)
        {
            return _context.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<Employee> UpdateAsync(Employee Employee)
        {
            _context.Employees.Update(Employee);
            _context.SaveChanges();
            return Task.FromResult(Employee);
        }
    }
}
