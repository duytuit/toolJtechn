using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;

namespace productLapRap.common
{
    public class MySqlHelper : IDisposable
    {
        private MySqlConnection _con;
        private MySqlTransaction _tran;

        public MySqlHelper()
        {
            string connStr = ConfigurationManager.ConnectionStrings["project"].ConnectionString;
            _con = new MySqlConnection(connStr);
            if (_con.State == ConnectionState.Closed)
                _con.Open();
        }

        #region UPSERT METHODS
        public int UpsertFromObject<T>(
            string tableName,
            T obj,
            string keyProperty = "Id",
            bool returnInsertedId = false)
        {
            var props = typeof(T).GetProperties();
            var keyProp = props.FirstOrDefault(p => p.Name.Equals(keyProperty, StringComparison.OrdinalIgnoreCase));
            if (keyProp == null)
                throw new ArgumentException("Không tìm thấy khóa chính: " + keyProperty);

            var columns = new List<string>();
            var values = new List<string>();
            var updates = new List<string>();
            var parameters = new List<MySqlParameter>();

            foreach (var prop in props)
            {
                string name = prop.Name;
                object value = prop.GetValue(obj) ?? DBNull.Value;

                if (name.Equals(keyProperty, StringComparison.OrdinalIgnoreCase))
                    continue;

                columns.Add(name);
                values.Add("@" + name);
                updates.Add($"{name} = VALUES({name})");
                parameters.Add(new MySqlParameter("@" + name, value));
            }

            string sql = $@"
                INSERT INTO {tableName} ({string.Join(",", columns)})
                VALUES ({string.Join(",", values)})
                ON DUPLICATE KEY UPDATE {string.Join(", ", updates)};";

            if (returnInsertedId)
                sql += " SELECT LAST_INSERT_ID();";

            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                cmd.Parameters.AddRange(parameters.ToArray());

                if (returnInsertedId)
                {
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
                return cmd.ExecuteNonQuery();
            }
        }

        public int UpsertFromObjectByColumn<T>(
            string tableName,
            T obj,
            string[] keyColumns,
            bool returnInsertedId = false)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Tên bảng không được để trống.", nameof(tableName));
            if (keyColumns == null || keyColumns.Length == 0)
                throw new ArgumentException("Phải truyền ít nhất một cột khóa.", nameof(keyColumns));

            var props = typeof(T).GetProperties();
            var columns = new List<string>();
            var values = new List<string>();
            var updateCols = new List<string>();
            var parameters = new List<MySqlParameter>();

            foreach (var prop in props)
            {
                string name = prop.Name;
                object value = prop.GetValue(obj) ?? DBNull.Value;

                if (name.Equals("id", StringComparison.OrdinalIgnoreCase))
                    continue;

                columns.Add(name);
                values.Add("@" + name);
                parameters.Add(new MySqlParameter("@" + name, value));

                if (!keyColumns.Any(k => k.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    updateCols.Add($"{name} = VALUES({name})");
            }

            string sql = $@"
                INSERT INTO {tableName} ({string.Join(",", columns)})
                VALUES ({string.Join(",", values)})
                ON DUPLICATE KEY UPDATE {string.Join(", ", updateCols)};";

            if (returnInsertedId)
                sql += " SELECT LAST_INSERT_ID();";

            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                cmd.Parameters.AddRange(parameters.ToArray());
                if (returnInsertedId)
                {
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
                return cmd.ExecuteNonQuery();
            }
        }
        #endregion

        #region LOAD / EXECUTE
        public DataTable LoadTable(string sql, params MySqlParameter[] parameters)
        {
            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public DataTable LoadTable(string sql, string[] names, object[] values)
        {
            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                if (names != null && values != null)
                {
                    if (names.Length != values.Length)
                        throw new ArgumentException("Số lượng tên và giá trị tham số không khớp.");

                    for (int i = 0; i < names.Length; i++)
                        cmd.Parameters.AddWithValue(names[i], values[i] ?? DBNull.Value);
                }

                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }

        public int ExecuteNonQuery(string sql, params MySqlParameter[] parameters)
        {
            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(string sql, params MySqlParameter[] parameters)
        {
            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        public DataTable ExecuteQuery(string sql, params MySqlParameter[] parameters)
        {
            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
        }
        #endregion

        #region CRUD HELPERS
        public int DeleteById(string tableName, object idValue, string keyProperty = "Id")
        {
            string sql = $"DELETE FROM {tableName} WHERE {keyProperty} = @{keyProperty}";
            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                cmd.Parameters.AddWithValue("@" + keyProperty, idValue ?? DBNull.Value);
                return cmd.ExecuteNonQuery();
            }
        }

        public DataRow GetSingleRecord(string tableName, object value, string keyProperty = "Id", bool descending = false)
        {
            string orderDir = descending ? "DESC" : "ASC";
            string sql = $"SELECT * FROM {tableName}";

            if (value != null)
                sql += $" WHERE {keyProperty} = @val";

            sql += $" ORDER BY {keyProperty} {orderDir} LIMIT 1";

            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                if (value != null)
                    cmd.Parameters.AddWithValue("@val", value);

                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            }
        }
        #endregion

        #region UTILS
        public string GenerateSoChungTu(string tableName, string columnName, string prefix, int numberLength)
        {
            string sql = $@"
                SELECT MAX({columnName}) 
                FROM {tableName} 
                WHERE {columnName} LIKE @prefix";

            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                cmd.Parameters.AddWithValue("@prefix", prefix + "%");

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    string maxCode = result.ToString();
                    string numberPart = maxCode.Substring(prefix.Length);
                    int number;
                    if (int.TryParse(numberPart, out number))
                        return prefix + (number + 1).ToString().PadLeft(numberLength, '0');
                }
            }
            return prefix + 1.ToString().PadLeft(numberLength, '0');
        }

        public double GetSumValue(string tableName, string sumColumn, string whereColumn, object whereValue)
        {
            string sql = $@"
                SELECT SUM({sumColumn}) 
                FROM {tableName} 
                WHERE {whereColumn} = @WhereValue";

            using (var cmd = new MySqlCommand(sql, _con, _tran))
            {
                cmd.Parameters.AddWithValue("@WhereValue", whereValue ?? DBNull.Value);
                object result = cmd.ExecuteScalar();
                return (result != null && result != DBNull.Value) ? Convert.ToDouble(result) : 0;
            }
        }
        #endregion

        #region TRANSACTION
        public void BeginTransaction()
        {
            if (_tran == null)
            {
                if (_con.State != ConnectionState.Open)
                    _con.Open();
                _tran = _con.BeginTransaction();
            }
        }

        public void CommitTransaction()
        {
            if (_tran != null)
            {
                _tran.Commit();
                _tran.Dispose();
                _tran = null;
            }
        }

        public void RollbackTransaction()
        {
            if (_tran != null)
            {
                _tran.Rollback();
                _tran.Dispose();
                _tran = null;
            }
        }
        #endregion

        #region DISPOSE
        public void Dispose()
        {
            if (_tran != null)
            {
                _tran.Dispose();
                _tran = null;
            }
            if (_con != null)
            {
                if (_con.State != ConnectionState.Closed)
                    _con.Close();
                _con.Dispose();
                _con = null;
            }
        }
        #endregion
    }
}
