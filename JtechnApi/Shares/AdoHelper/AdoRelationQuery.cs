using Microsoft.Extensions.Caching.Distributed;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace JtechnApi.Shares.AdoHelper
{
    public static class AdoRelationQuery
    {
         public static async Task<List<ExpandoObject>> WithRelationsAdoAsync(
         string connectionString,
         string tableName,
         string[] columns,
         int? offset = null,
         int? limit = null,
         IEnumerable<AdoRelation> relations = null,
         IDistributedCache redisCache = null,
         string redisKey = null,
         TimeSpan? redisKeyDuration = null,
         CancellationToken cancellationToken = default)
        {
            // Nếu có Redis key, thử lấy dữ liệu từ Redis trước
            if (!string.IsNullOrEmpty(redisKey) && redisKeyDuration.HasValue && redisCache != null)
            {
                var cachedJson = await redisCache.GetStringAsync(redisKey, cancellationToken);
                if (!string.IsNullOrEmpty(cachedJson))
                {
                    var cachedData = JsonSerializer.Deserialize<List<ExpandoObject>>(cachedJson);
                    if (cachedData != null)
                        return cachedData;
                }
            }

            await using var conn = new MySqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);

            var baseCmd = await SqlHelpers.BuildBaseCommandAsync(conn, tableName, columns, offset, limit, cancellationToken: cancellationToken);

            var baseList = (await SqlHelpers.ExecuteQueryAsync(baseCmd, cancellationToken))
                .Cast<IDictionary<string, object>>().ToList();

            await LoadRelationsRecursiveAsync(conn, baseList, relations, redisCache, cancellationToken);

            var result = baseList.Select(r => (ExpandoObject)r).ToList();

            // Lưu vào Redis nếu có key và thời gian sống
            if (!string.IsNullOrEmpty(redisKey) && redisKeyDuration.HasValue && redisCache != null)
            {
                var json = JsonSerializer.Serialize(result);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = redisKeyDuration
                };
                await redisCache.SetStringAsync(redisKey, json, options, cancellationToken);
            }

            return result;
        }

        private static async Task LoadRelationsRecursiveAsync(
            MySqlConnection conn,
            List<IDictionary<string, object>> parentList,
            IEnumerable<AdoRelation> relations,
            IDistributedCache redisCache,
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

               
                var cmd = await SqlHelpers.BuildSelectInCommandAsync(conn, relation.Table, relation.Columns, relation.KeyName, parentKeys, cancellationToken);
                childList = (await SqlHelpers.ExecuteQueryAsync(cmd, cancellationToken))
                    .Cast<IDictionary<string, object>>().ToList();

                var childLookup = childList
                    .Where(c => c.ContainsKey(relation.ForeignKey))
                    .GroupBy(c => Convert.ToInt64(c[relation.ForeignKey]))
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var parent in parentList)
                {
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
                        parent[relation.Name] = relatedItems != null ? ((ExpandoObject)relatedItems.FirstOrDefault()) : new ExpandoObject();
                    }
                }

                if (relation.SubRelations?.Any() == true)
                {
                    var allChildren = childList.Select(c => (IDictionary<string, object>)c).ToList();
                    await LoadRelationsRecursiveAsync(conn, allChildren, relation.SubRelations, redisCache, cancellationToken);
                }
            }
        }
    }
}
