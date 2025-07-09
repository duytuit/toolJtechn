using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JtechnApi.Shares.AdoHelper
{
    public static class SqlHelpers
    {
        public static async Task<MySqlCommand> BuildBaseCommandAsync(
     MySqlConnection connection,
     string tableName,
     string[] fields,
     int? skip = null,
     int? take = null,
     Dictionary<string, object> whereEquals = null,
     Dictionary<string, string> whereLikes = null,
     Dictionary<string, IEnumerable<object>> whereInList = null,
     List<(string Field, DateTime From, DateTime To)> dateRangeList = null,
     List<string> orderByList = null,
     CancellationToken cancellationToken = default)
        {
            var cmd = new MySqlCommand();
            cmd.Connection = connection;

            // Kiểm tra có deleted_at
            cmd.CommandText = $@"
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @table AND COLUMN_NAME = 'deleted_at'";
            cmd.Parameters.AddWithValue("@table", tableName);
            var count = (long)(await cmd.ExecuteScalarAsync(cancellationToken));
            bool hasDeletedAt = count > 0;

            var whereClauses = new List<string>();

            if (hasDeletedAt)
                whereClauses.Add("deleted_at IS NULL");

            // WHERE EQUALS
            if (whereEquals != null)
            {
                foreach (var kvp in whereEquals)
                {
                    string paramName = $"@{kvp.Key}";
                    whereClauses.Add($"{kvp.Key} = {paramName}");
                    cmd.Parameters.AddWithValue(paramName, kvp.Value ?? DBNull.Value);
                }
            }

            // WHERE LIKE
            if (whereLikes != null)
            {
                foreach (var kvp in whereLikes)
                {
                    string paramName = $"@like_{kvp.Key}";
                    whereClauses.Add($"{kvp.Key} LIKE {paramName}");
                    cmd.Parameters.AddWithValue(paramName, $"%{kvp.Value}%");
                }
            }

            // WHERE IN
            if (whereInList != null)
            {
                foreach (var kvp in whereInList)
                {
                    var paramNames = new List<string>();
                    int index = 0;
                    foreach (var value in kvp.Value)
                    {
                        string paramName = $"@in_{kvp.Key}_{index++}";
                        paramNames.Add(paramName);
                        cmd.Parameters.AddWithValue(paramName, value);
                    }
                    whereClauses.Add($"{kvp.Key} IN ({string.Join(", ", paramNames)})");
                }
            }

            // WHERE Date Range
            if (dateRangeList != null)
            {
                for (int i = 0; i < dateRangeList.Count; i++)
                {
                    var range = dateRangeList[i];
                    string fromParam = $"@dateFrom{i}";
                    string toParam = $"@dateTo{i}";
                    whereClauses.Add($"{range.Field} BETWEEN {fromParam} AND {toParam}");
                    cmd.Parameters.AddWithValue(fromParam, range.From);
                    cmd.Parameters.AddWithValue(toParam, range.To);
                }
            }

            // Compose final parts
            string whereClause = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : "";
            string orderClause = (orderByList != null && orderByList.Count > 0) ? "ORDER BY " + string.Join(", ", orderByList) : "";
            string pagingClause = (take.HasValue ? $"LIMIT {take.Value}" : "") +
                                  (skip.HasValue ? $" OFFSET {skip.Value}" : "");

            string fieldList = string.Join(", ", fields);

            cmd.CommandText = $@"
            SELECT {fieldList}
            FROM {tableName}
            {whereClause}
            {orderClause}
            {pagingClause}".Trim();

            return cmd;
        }

        public static async Task<MySqlCommand> BuildSelectInCommandAsync(MySqlConnection conn, string tableName, string[] fields, string keyField, List<object> ids, CancellationToken cancellationToken = default)
        {
            string fieldList = string.Join(", ", fields);
            var cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText =$@"
            SELECT COUNT(*) 
            FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = @table AND COLUMN_NAME = 'deleted_at'";
            cmd.Parameters.AddWithValue("@table", tableName);
            var count = (long)await cmd.ExecuteScalarAsync(cancellationToken);
            bool hasDeletedAt = count > 0;
            var parameters = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                var paramName = "@id" + i;
                parameters.Add(paramName);
                cmd.Parameters.AddWithValue(paramName, ids[i]);
            }

            string whereIn = $"{keyField} IN ({string.Join(", ", parameters)})";
            string whereClause = hasDeletedAt
                ? $"WHERE {whereIn} AND deleted_at IS NULL"
                : $"WHERE {whereIn}";
            cmd.CommandText = $"SELECT {fieldList} FROM {tableName} {whereClause}";
            return cmd;
        }

        public static async Task<List<ExpandoObject>> ExecuteQueryAsync(MySqlCommand cmd, CancellationToken cancellationToken = default)
        {
            var result = new List<ExpandoObject>();
            using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var row = new ExpandoObject() as IDictionary<string, object>;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = await reader.IsDBNullAsync(i, cancellationToken)
                        ? null
                        : reader.GetValue(i);
                }
                result.Add((ExpandoObject)row);
            }
            return result;
        }
    }

}
