using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Categorys.Dtos;
using Vudaco.Categorys.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.SqlServerHelper;

namespace Vudaco.Categorys.Repositories
{
    public class IncomeExpenseCategoryRepository : BaseRepository<IncomeExpenseCategory>, IIncomeExpenseCategoryRepository
    {
        private readonly VudacoDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        public IncomeExpenseCategoryRepository(VudacoDBContext context, IConfiguration configuration, RedisService redis) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
        }
        public Task<IncomeExpenseCategory> CreateAsync(IncomeExpenseCategory IncomeExpenseCategory)
        {
              _context.IncomeExpenseCategorys.Add(IncomeExpenseCategory);
            _context.SaveChanges();
            return Task.FromResult(IncomeExpenseCategory);
        }

        public Task<IncomeExpenseCategory> DeleteSoftAsync(IncomeExpenseCategory IncomeExpenseCategory)
        {
              _context.IncomeExpenseCategorys.Update(IncomeExpenseCategory);
            _context.SaveChanges();
            return Task.FromResult(IncomeExpenseCategory);
        }

        public async Task<PaginatedResultReact<object>> GetObjectTaskAsync(IncomeExpenseCategoryDto IncomeExpenseCategoryDto, int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> {  "updated_at desc" , "id"};
            if (IncomeExpenseCategoryDto.StorageId > 0)
                whereEquals["storage_id"] = IncomeExpenseCategoryDto.StorageId;
            if (IncomeExpenseCategoryDto.Type >= 0)
                whereEquals["type"] = IncomeExpenseCategoryDto.Type;
            dynamic results = await AdoRelationQuerySqlServer.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "income_expense_categorys",
                        new[] { "id","code","name","type","parent_id","storage_id","created_by","updated_by","deleted_by","deleted_at","created_at","updated_at",},
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

        public Task<IncomeExpenseCategory> ShowAsync(int id)
        {
             return _context.IncomeExpenseCategorys.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public Task<IncomeExpenseCategory> UpdateAsync(IncomeExpenseCategory IncomeExpenseCategory)
        {
             _context.IncomeExpenseCategorys.Update(IncomeExpenseCategory);
            _context.SaveChanges();
            return Task.FromResult(IncomeExpenseCategory);
        }
    }
}
