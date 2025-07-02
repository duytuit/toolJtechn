using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

namespace JtechnApi.Shares.Connects
{
    public class OracleConnectionFactory
    {
        private readonly string _connectionString;

        public OracleConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Oracle");
        }

        public OracleConnection CreateConnection()
        {
            var conn = new OracleConnection(_connectionString);
            conn.Open(); // Mở tại đây, dùng trong Scoped
            return conn;
        }
    }
}
