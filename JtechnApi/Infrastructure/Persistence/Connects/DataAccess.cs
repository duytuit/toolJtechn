using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace JtechnApi.Shares.Connects
{
    public class DataAccess : IDisposable
    {
        public static string ConnectionString =  "SERVER=192.168.207.6\\SQLEXPRESS; Uid=sa;Pwd=Jtechn@1234;Database=STOCKMANAGEMENT";
        private SqlConnection con;

        public DataAccess()
        {
            con = new SqlConnection(ConnectionString);
        }

        public void OpenConnect()
        {
            if (con.State == ConnectionState.Closed)
            {
                con.Open();
            }
        }

        public void CloseConnect()
        {
            if (con.State != ConnectionState.Closed)
            {
                con.Close();
                SqlConnection.ClearPool(con);
            }
        }

        public DataTable RunQuery(string query)
        {
            DataTable dt = new DataTable();
            OpenConnect();

            using (SqlCommand cmd = new SqlCommand(query, con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandTimeout = 1800;
                da.Fill(dt);
            }

            CloseConnect();
            return dt;
        }

        public void ExecuteQuery(string query)
        {
            OpenConnect();

            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.ExecuteNonQuery();
            }

            CloseConnect();
        }

        public int ExecuteNonQuery(string sql, params object[] parameters)
        {
            OpenConnect();

            using (SqlCommand command = new SqlCommand(sql, con))
            {
                for (int i = 0; i < parameters.Length; i += 2)
                {
                    command.Parameters.AddWithValue(parameters[i].ToString(), parameters[i + 1]);
                }

                int rows = command.ExecuteNonQuery();
                CloseConnect();
                return rows;
            }
        }

        public object ExecuteScalar(string sql)
        {
            OpenConnect();

            using (SqlCommand command = new SqlCommand(sql, con))
            {
                object result = command.ExecuteScalar();
                CloseConnect();
                return result;
            }
        }

        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            OpenConnect();

            using (SqlCommand command = new SqlCommand(sql, con))
            using (SqlDataAdapter da = new SqlDataAdapter(command))
            {
                da.Fill(dt);
            }

            CloseConnect();
            return dt;
        }

        public void SqlBulkCopy(DataTable dataTable, string destination)
        {
            OpenConnect();

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(con))
            {
                bulkCopy.DestinationTableName = destination;
                bulkCopy.WriteToServer(dataTable);
            }

            CloseConnect();
        }

        public int UpsertFromObject<T>(string tableName, T obj, string keyProperty = "Id", bool returnInsertedId = false)
        {
            var type = typeof(T);
            var props = type.GetProperties();
            var keyProp = props.FirstOrDefault(p => p.Name.Equals(keyProperty, StringComparison.OrdinalIgnoreCase));

            if (keyProp == null)
                throw new ArgumentException("Không tìm thấy khóa chính: " + keyProperty);

            var keyValue = keyProp.GetValue(obj);

            var columns = new List<string>();
            var values = new List<string>();
            var insertParameters = new List<SqlParameter>();
            var updateParameters = new List<SqlParameter>();
            var updateCols = new List<string>();

            foreach (var prop in props)
            {
                var name = prop.Name;
                var value = prop.GetValue(obj) ?? DBNull.Value;

                if (!name.Equals(keyProperty, StringComparison.OrdinalIgnoreCase))
                {
                    updateCols.Add($"{name} = @{name}");
                    updateParameters.Add(new SqlParameter("@" + name, value));

                    columns.Add(name);
                    values.Add("@" + name);
                    insertParameters.Add(new SqlParameter("@" + name, value));
                }
            }

            updateParameters.Add(new SqlParameter("@" + keyProperty, keyValue));

            OpenConnect();

            string checkSql = $"SELECT COUNT(1) FROM {tableName} WHERE {keyProperty} = @{keyProperty}";
            using (var checkCmd = new SqlCommand(checkSql, con))
            {
                checkCmd.Parameters.Add(new SqlParameter("@" + keyProperty, keyValue));
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    // UPDATE
                    string updateSql = $"UPDATE {tableName} SET {string.Join(", ", updateCols)} WHERE {keyProperty} = @{keyProperty}";
                    using (var updateCmd = new SqlCommand(updateSql, con))
                    {
                        updateCmd.Parameters.AddRange(updateParameters.ToArray());
                        return updateCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // INSERT
                    string insertSql = $"INSERT INTO {tableName} ({string.Join(",", columns)}) VALUES ({string.Join(",", values)})";
                    if (returnInsertedId)
                        insertSql += "; SELECT CAST(SCOPE_IDENTITY() AS INT)";

                    using (var insertCmd = new SqlCommand(insertSql, con))
                    {
                        insertCmd.Parameters.AddRange(insertParameters.ToArray());
                        if (returnInsertedId)
                        {
                            object result = insertCmd.ExecuteScalar();
                            return result != null ? Convert.ToInt32(result) : 0;
                        }
                        else
                        {
                            return insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (con != null)
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                con.Dispose();
            }
        }
    }
}
