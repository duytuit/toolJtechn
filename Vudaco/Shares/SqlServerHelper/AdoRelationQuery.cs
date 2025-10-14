using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Shares.MysqlHelper;

namespace Vudaco.Shares.SqlServerHelper
{
    public static class AdoRelationQuerySqlServer
    {
        /// <summary>
        /// Truy vấn dữ liệu từ SQL Server với khả năng chọn cột, phân trang, lọc, sắp xếp và load các quan hệ liên quan.
        /// Hỗ trợ đếm tổng số bản ghi và cache kết quả vào Redis.
        /// </summary>
        public static async Task<object> WithRelationsAdoAsync(
            string connectionString,
            string tableName,
            string[] columns,
            int? offset = null,
            int? limit = null,
            Dictionary<string, object> whereEquals = null,
            Dictionary<string, string> whereLikes = null,
            Dictionary<string, IEnumerable<object>> whereInList = null,
            List<(string Sql, object[] Params)> whereCustom = null,
            List<(string Field, DateTime From, DateTime To)> dateRangeList = null,
            List<string> orderByList = null,
            IEnumerable<AdoRelation> relations = null,
            RedisService redisCache = null,
            string redisKey = null,
            TimeSpan? redisKeyDuration = null,
            bool includeCount = false,
            CancellationToken cancellationToken = default)
        {
            int count = 0;

            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);

            // Nếu cần đếm tổng số bản ghi
            if (includeCount)
            {
                count = await SqlServerHelpers.ExecuteCountCommandAsync(
                    conn, tableName, whereEquals, whereLikes, whereInList, whereCustom, dateRangeList, cancellationToken);
            }

            // Kiểm tra Redis cache
            if (!string.IsNullOrEmpty(redisKey) && redisKeyDuration.HasValue && redisCache != null)
            {
                var cachedJson = await redisCache.GetAsync(redisKey, cancellationToken);
                if (!string.IsNullOrEmpty(cachedJson))
                {
                    var cachedData = JsonSerializer.Deserialize<List<ExpandoObject>>(cachedJson);
                    if (cachedData != null)
                        return new { Count = count, Data = cachedData };
                }
            }

            // Build câu lệnh SELECT chính
            using var baseCmd = await SqlServerHelpers.BuildBaseCommandAsync(
                conn, tableName, columns, offset, limit,
                whereEquals, whereLikes, whereInList, whereCustom, dateRangeList, orderByList, cancellationToken);

            var baseList = (await SqlServerHelpers.ExecuteQueryAsync(baseCmd, cancellationToken))
                .Cast<IDictionary<string, object>>()
                .ToList();

            // Load các quan hệ liên quan (1-n, 1-1)
            await LoadRelationsRecursiveAsync(conn, baseList, relations, cancellationToken);

            var result = baseList.Select(r => (ExpandoObject)r).ToList();

            // Lưu vào Redis nếu cần
            if (!string.IsNullOrEmpty(redisKey) && redisKeyDuration.HasValue && redisCache != null)
            {
                var json = JsonSerializer.Serialize(result);
                await redisCache.SetAsync(redisKey, json, redisKeyDuration, cancellationToken);
            }

            return new { Count = count, Data = result };
        }

        private static async Task LoadRelationsRecursiveAsync(
            SqlConnection conn,
            List<IDictionary<string, object>> parentList,
            IEnumerable<AdoRelation> relations,
            CancellationToken cancellationToken)
        {
            foreach (var relation in relations ?? Enumerable.Empty<AdoRelation>())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var parentKeys = parentList
                    .Select(p => p[relation.ParentKey])
                    .Where(v => v != null)
                    .Distinct()
                    .ToList();

                if (parentKeys.Count == 0)
                    continue;

                List<IDictionary<string, object>> childList;

                // Dùng SqlServerHelpers thay vì SqlHelpers
                var cmd = await SqlServerHelpers.BuildSelectInCommandAsync(
                    conn, relation.Table, relation.Columns, relation.KeyName, parentKeys, cancellationToken);

                childList = (await SqlServerHelpers.ExecuteQueryAsync(cmd, cancellationToken))
                    .Cast<IDictionary<string, object>>()
                    .ToList();

                // Mapping dữ liệu con theo khóa ngoại
                var childLookup = childList
                    .Where(c => c.ContainsKey(relation.ForeignKey))
                    .GroupBy(c => Convert.ToInt64(c[relation.ForeignKey]))
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var parent in parentList)
                {
                    if (!parent.ContainsKey(relation.ParentKey) || parent[relation.ParentKey] == null)
                        continue;

                    var key = Convert.ToInt64(parent[relation.ParentKey]);
                    childLookup.TryGetValue(key, out var relatedItems);

                    if (relation.IsCollection)
                    {
                        parent[relation.Name] = relatedItems != null
                            ? relatedItems.Select(x => (ExpandoObject)x).ToList()
                            : new List<ExpandoObject>();
                    }
                    else
                    {
                        parent[relation.Name] = relatedItems != null
                            ? (ExpandoObject)relatedItems.FirstOrDefault()
                            : new ExpandoObject();
                    }
                }

                // Đệ quy nạp sub-relations (nếu có)
                if (relation.SubRelations?.Any() == true)
                {
                    var allChildren = childList.Select(c => (IDictionary<string, object>)c).ToList();
                    await LoadRelationsRecursiveAsync(conn, allChildren, relation.SubRelations, cancellationToken);
                }
            }
        }
    }
}
