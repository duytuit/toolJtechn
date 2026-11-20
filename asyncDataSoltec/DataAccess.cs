using System;
using System.Data;
using System.Data.SqlClient;

namespace asyncDataSoltec
{
    public class DataAccess
    {
        public static String ConnectionString = "SERVER=192.168.207.6\\SQLEXPRESS; Uid=sa;Pwd=Jtechn@1234;Database=STOCKMANAGEMENT";
        public SqlConnection con = new SqlConnection(ConnectionString);

        public DataTable RunQuery(string str)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(str, connection))
            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
            {
                command.CommandTimeout = 1800;
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        public void ExecuteTransaction(Action<SqlConnection, SqlTransaction> operation)
        {
            if (operation == null)
                throw new ArgumentNullException("operation");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        operation(connection, transaction);
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
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
            }
        }

        public void ExcuteQuery(string str)
        {
            ExecuteNonQuery(str);
        }

        public int ExecuteNonQuery(String sql, params Object[] parameters)
        {
            if (parameters == null || parameters.Length % 2 != 0)
                throw new ArgumentException("Parameters must be supplied as name/value pairs.", "parameters");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = 1800;
                for (int i = 0; i < parameters.Length; i += 2)
                {
                    command.Parameters.AddWithValue(
                        Convert.ToString(parameters[i]),
                        parameters[i + 1] ?? DBNull.Value);
                }

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public object ExecuteScalar(string sql)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.CommandTimeout = 1800;
                connection.Open();
                return command.ExecuteScalar();
            }
        }

        public DataTable GetDataTabale(String sql)
        {
            return RunQuery(sql);
        }

        public void sqlBulkCopy(DataTable dataTable, string destination)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(connection))
            {
                sqlBulkCopy.DestinationTableName = destination;
                connection.Open();
                sqlBulkCopy.WriteToServer(dataTable);
            }
        }
    }
}
