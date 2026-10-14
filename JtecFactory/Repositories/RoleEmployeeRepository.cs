using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using JtecFactory.Models;

namespace JtecFactory.Repositories
{
    public class RoleEmployeeRepository : BaseRepository<RoleEmployeeModel>
    {
        public RoleEmployeeRepository()
            : base("role_employee")
        {
        }

        public List<RoleEmployeeModel> GetByEmployeeId(int employeeId)
        {
            var result = new List<RoleEmployeeModel>();
            const string sql = "SELECT Id, RoleId, EmployeeId, CreatedAt FROM role_employee WHERE EmployeeId = @EmployeeId ORDER BY RoleId ASC";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                conn.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                        result.Add(Map(reader));
                }
            }

            return result;
        }

        public List<RoleEmployeeModel> GetByRoleId(int roleId)
        {
            var result = new List<RoleEmployeeModel>();
            const string sql = "SELECT Id, RoleId, EmployeeId, CreatedAt FROM role_employee WHERE RoleId = @RoleId ORDER BY EmployeeId ASC";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@RoleId", roleId);
                conn.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                        result.Add(Map(reader));
                }
            }

            return result;
        }

        public int Insert(int roleId, int employeeId)
        {
            const string sql = "INSERT INTO role_employee (RoleId, EmployeeId, CreatedAt) VALUES (@RoleId, @EmployeeId, @CreatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@RoleId", roleId);
                cmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        public int DeleteById(int id)
        {
            return base.DeleteById(id, false);
        }

        public int DeleteByEmployeeId(int employeeId)
        {
            return DeleteWhere("EmployeeId", employeeId, false);
        }

        public int DeleteByRoleId(int roleId)
        {
            return DeleteWhere("RoleId", roleId, false);
        }

        private static RoleEmployeeModel Map(SqlDataReader reader)
        {
            return new RoleEmployeeModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                RoleId = Convert.ToInt32(reader["RoleId"]),
                EmployeeId = Convert.ToInt32(reader["EmployeeId"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}
