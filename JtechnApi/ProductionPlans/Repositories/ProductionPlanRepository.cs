
using JtechnApi.ProductionPlans.Dtos;
using JtechnApi.ProductionPlans.Models;
using JtechnApi.Shares;
using JtechnApi.Shares.AdoHelper;
using JtechnApi.Shares.BaseRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JtechnApi.ProductionPlans.Repositories
{
    public class ProductionPlanRepository : BaseRepository<ProductionPlan>, IProductionPlanRepository
    {
        private readonly DBContext _context;
        private readonly IConfiguration _configuration;
        private readonly RedisService _redis;
        private readonly ILogger<ProductionPlanRepository> _logger;
        public ProductionPlanRepository(DBContext context, IConfiguration configuration, RedisService redis, ILogger<ProductionPlanRepository> logger) : base(context)
        {
            _context = context;
            _configuration = configuration;
            _redis = redis;
            _logger = logger;
        }

        public async Task<PaginatedResultVue<object>> GetPaginatedAsync(RequestPlanDto dto,int page, int pageSize, CancellationToken cancellationToken)
        {
            var whereEquals = new Dictionary<string, object>();
            var whereLikes = new Dictionary<string, string>();
            var whereDateRange = new List<(string Field, DateTime From, DateTime To)>();
            var orderByList = new List<string> { "id ASC" };
            var whereCustom = new List<(string Sql, object[] Params)>();


            if (!string.IsNullOrWhiteSpace(dto.ProductCode))
                whereLikes["code"] = dto.ProductCode;

            if (!string.IsNullOrWhiteSpace(dto.Code))
                whereLikes["code"] = dto.Code;

            if (!string.IsNullOrWhiteSpace(dto.Cam))
                whereLikes["cam"] = dto.Cam;

            if (!string.IsNullOrWhiteSpace(dto.Filter_15))
            {
                whereCustom.Add(("JSON_EXTRACT(description, '$.hangxuatchualam_dap') < ?", new object[] { 0 }));
                whereCustom.Add(("JSON_EXTRACT(description, '$.hangxuatchualam_dap') < ?", new object[] { "" }));
            }
            if (!string.IsNullOrWhiteSpace(dto.Filter_16))
            {
                whereCustom.Add(("JSON_EXTRACT(description, '$.hangxuatchualam_cam') < ?", new object[] { 0 }));
                whereCustom.Add(("JSON_EXTRACT(description, '$.hangxuatchualam_cam') < ?", new object[] { "" }));
            } 
            if (!string.IsNullOrWhiteSpace(dto.Filter_17))
            {
                whereCustom.Add(("JSON_EXTRACT(description, '$.hangxuatchualam_lrap') < ?", new object[] { 0 }));
                whereCustom.Add(("JSON_EXTRACT(description, '$.hangxuatchualam_lrap') < ?", new object[] { "" }));
            }

            if (!string.IsNullOrWhiteSpace(dto.GiaCong))
                 whereCustom.Add(("JSON_UNQUOTE(JSON_EXTRACT(description, '$.gia_cong')) LIKE ?",new object[] { "%"+ dto.GiaCong + "%" }));

            whereEquals["flag_a"] = 1;
            dynamic results = await AdoRelationQuery.WithRelationsAdoAsync(
                        _configuration.GetConnectionString("DefaultConnection"),
                        "production_plans",
                        new[] { "id", "code", "description", "cam", "flag_a", "flag_b", "version", "updated_at", "deleted_at" },
                        offset: (page - 1) * pageSize,
                        limit: pageSize,
                        whereEquals: whereEquals,
                        whereLikes: whereLikes,
                        whereCustom: whereCustom,
                        dateRangeList: whereDateRange,
                        orderByList: orderByList,
                        redisCache: _redis,
                        includeCount: true,
                        cancellationToken: cancellationToken
                    );
            int totalItems = results.Count;
            var objectList = new List<object>();
            objectList.AddRange(results.Data);
            var EmployeeProductionPlan = await _redis.GetAsync("jtec_hn_database_update_EmployeeProductionPlan");
            var productionPlanHeaderWeekday = await _redis.GetAsync("jtec_hn_database_productionPlanHeaderWeekday");
            var productionPlanCam = await _redis.GetAsync("jtec_hn_database_productionPlanCam");
            var update_AsyncCam = await _redis.GetAsync("jtec_hn_database_update_AsyncCam");
            var history_plan = _redis.GetSortedHistoryPlansAsync("ke_hoach_san_xuat_");
            //=====================================================================
               // decimal tongCam = await ExecuteSumJsonFieldAsync(
               //      _configuration.GetConnectionString("DefaultConnection"),
               //     "production_plans",
               //      "$.soluongdaycatchualam_cam",
               //      whereEquals: whereEquals,
               //      whereLikes: whereLikes,
               //      whereCustom: whereCustom,
               //      dateRangeList: whereDateRange
               // );
            //=====================================================================
            var _results = new PaginatedResultVue<object>
            {
                Current_page = page,
                Per_page = pageSize,
                Last_page = (int)Math.Ceiling((double)totalItems / pageSize),
                Total = totalItems,
                Data = objectList,
            };
            objectList = null;
            results = null;
            
            _results.Extra["EmployeeProductionPlan"] = EmployeeProductionPlan;
            _results.Extra["productionPlanHeaderWeekday"] = productionPlanHeaderWeekday;
            _results.Extra["history_plan"] = history_plan;
            _results.Extra["productionPlanCam"] = productionPlanCam;
            _results.Extra["update_AsyncCam"] = update_AsyncCam;
           // _results.Extra["tongCam"] = tongCam;
            productionPlanCam = null;
            update_AsyncCam = null;
            // _logger.LogInformation(EmployeeProductionPlan);
            whereEquals?.Clear(); whereLikes?.Clear(); whereDateRange?.Clear(); orderByList?.Clear();
            return _results;
        }
        public static async Task<decimal> ExecuteSumJsonFieldAsync(
             string connectionString,
             string tableName,
             string jsonFieldPath, // ví dụ "$.so_day"
             Dictionary<string, object> whereEquals = null,
             Dictionary<string, string> whereLikes = null,
             Dictionary<string, IEnumerable<object>> whereInList = null,
             List<(string Sql, object[] Params)> whereCustom = null,
             List<(string Field, DateTime From, DateTime To)> dateRangeList = null,
             CancellationToken cancellationToken = default)
        {
            await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);

            var whereClauses = new List<string>();
            using var cmd = conn.CreateCommand();

            if (whereEquals != null)
            {
                foreach (var kv in whereEquals)
                {
                    var paramName = $"@eq_{kv.Key}";
                    whereClauses.Add($"`{kv.Key}` = {paramName}");
                    cmd.Parameters.AddWithValue(paramName, kv.Value);
                }
            }

            if (whereLikes != null)
            {
                foreach (var kv in whereLikes)
                {
                    var paramName = $"@like_{kv.Key}";
                    whereClauses.Add($"`{kv.Key}` LIKE {paramName}");
                    cmd.Parameters.AddWithValue(paramName, $"%{kv.Value}%");
                }
            }

            if (whereInList != null)
            {
                foreach (var kv in whereInList)
                {
                    var paramNames = kv.Value.Select((v, i) => $"@in_{kv.Key}_{i}").ToList();
                    whereClauses.Add($"`{kv.Key}` IN ({string.Join(", ", paramNames)})");

                    int index = 0;
                    foreach (var val in kv.Value)
                        cmd.Parameters.AddWithValue(paramNames[index++], val);
                }
            }

            if (whereCustom != null)
            {
                int customIndex = 0;
                foreach (var (sql, paramValues) in whereCustom)
                {
                    var parts = sql.Split('?');
                    var sqlWithParams = "";

                    for (int i = 0; i < paramValues.Length; i++)
                    {
                        string paramName = $"@customParam_{customIndex}";
                        cmd.Parameters.AddWithValue(paramName, paramValues[i]);
                        sqlWithParams += parts[i] + paramName;
                        customIndex++;
                    }

                    if (parts.Length > paramValues.Length)
                    {
                        sqlWithParams += parts.Last();
                    }

                    whereClauses.Add(sqlWithParams);
                }
            }

            if (dateRangeList != null)
            {
                foreach (var range in dateRangeList)
                {
                    var fromParam = $"@from_{range.Field}";
                    var toParam = $"@to_{range.Field}";
                    whereClauses.Add($"`{range.Field}` BETWEEN {fromParam} AND {toParam}");
                    cmd.Parameters.AddWithValue(fromParam, range.From);
                    cmd.Parameters.AddWithValue(toParam, range.To);
                }
            }

            var whereSql = whereClauses.Count > 0 ? $"WHERE {string.Join(" AND ", whereClauses)}" : "";

            // SUM trên field JSON
            cmd.CommandText = $@"
            SELECT SUM(
                CAST(JSON_UNQUOTE(JSON_EXTRACT(description, '{jsonFieldPath}')) AS DECIMAL(18,2))
            ) 
            FROM `{tableName}` {whereSql}";

            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return result != DBNull.Value ? Convert.ToDecimal(result) : 0m;
        }
    }
}
