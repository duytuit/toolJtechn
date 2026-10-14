using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JtecFactory.Helpers
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
            using (var connection =
                new OracleConnection(_connectionString))
            using (var command =
                new OracleCommand(sql, connection))
            using (var adapter =
                new OracleDataAdapter(command))
            {
                var table = new DataTable();

                connection.Open();

                adapter.Fill(table);

                return table;
            }
        }
    }
}
