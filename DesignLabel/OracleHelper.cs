using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp5
{
    public class OracleHelper
    {
        private readonly string _connectionString;

        public OracleHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable ExecuteDataTable(string sql)
        {
            return ExecuteDataTable(sql, new OracleParameter[0]);
        }

        public DataTable ExecuteDataTable(string sql, params OracleParameter[] parameters)
        {
            using (var connection =
                new OracleConnection(_connectionString))
            using (var command =
                new OracleCommand(sql, connection))
            using (var adapter =
                new OracleDataAdapter(command))
            {
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);

                var table = new DataTable();

                connection.Open();

                adapter.Fill(table);

                return table;
            }
        }
    }
}
