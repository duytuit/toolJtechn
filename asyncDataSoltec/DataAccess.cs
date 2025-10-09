using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace asyncDataSoltec
{
    public class DataAccess
    {
        public static String ConnectionString = "SERVER=192.168.207.6\\SQLEXPRESS; Uid=sa;Pwd=Jtechn@1234;Database=STOCKMANAGEMENT";
        public SqlConnection con = new SqlConnection(ConnectionString);
        public DataTable RunQuery(string str)
        {
            OpenConnect();
            SqlCommand cmd = new SqlCommand(str, con);
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();
            cmd.CommandTimeout = 1800;
            da.Dispose();
            da = new SqlDataAdapter(cmd);
            //dt.Dispose();
            da.Fill(dt);
            CloseConnect();
            return dt;
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

        public void ExcuteQuery(string str)
        {
            OpenConnect();
            SqlCommand cmd = new SqlCommand(str, con);
            CloseConnect();
        }

        public int ExecuteNonQuery(String sql, params Object[] parameters)
        {
            OpenConnect();
            SqlCommand command = new SqlCommand(sql);
            command.Connection.Open();
            for (int i = 0; i < parameters.Length; i = i + 2)
            {
                command.Parameters.AddWithValue(parameters[i].ToString(), parameters[i + 1]);
            }
            int rows = command.ExecuteNonQuery();
            CloseConnect();
            return rows;
        }

        public object ExecuteScalar(string sql)
        {
            SqlCommand command = new SqlCommand(sql);
            command.Connection.Open();
            object value = command.ExecuteScalar();
            return value;
        }

        public DataTable GetDataTabale(String sql)
        {
            OpenConnect();
            SqlCommand command = new SqlCommand();
            command.CommandText = sql;
            command.Connection = con;
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = command;
            DataTable dt = new DataTable();
            da.Fill(dt);
            CloseConnect();
            return dt;
        }

        public void sqlBulkCopy(DataTable dataTable, string destination)
        {
            SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con);
            sqlBulkCopy.DestinationTableName = "[DatabaseRepairPart].[dbo].[Status]";
            OpenConnect();
            sqlBulkCopy.WriteToServer(dataTable);
        }
    }
}
