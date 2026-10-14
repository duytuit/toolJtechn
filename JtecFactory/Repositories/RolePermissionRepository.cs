using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using JtecFactory.Models;

namespace JtecFactory.Repositories
{
    public class RolePermissionRepository : BaseRepository<RolePermissionModel>
    {
        public RolePermissionRepository()
            : base("role_permission")
        {
        }

        public List<RolePermissionModel> GetByRoleId(int roleId)
        {
            var result = new List<RolePermissionModel>();
            const string sql = "SELECT Id, RoleId, PermissionId, CreatedAt FROM role_permission WHERE RoleId = @RoleId ORDER BY PermissionId ASC";
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

        public List<RolePermissionModel> GetByPermissionId(int permissionId)
        {
            var result = new List<RolePermissionModel>();
            const string sql = "SELECT Id, RoleId, PermissionId, CreatedAt FROM role_permission WHERE PermissionId = @PermissionId ORDER BY RoleId ASC";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@PermissionId", permissionId);
                conn.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                        result.Add(Map(reader));
                }
            }

            return result;
        }

        public int Insert(int roleId, int permissionId)
        {
            const string sql = "INSERT INTO role_permission (RoleId, PermissionId, CreatedAt) VALUES (@RoleId, @PermissionId, @CreatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@RoleId", roleId);
                cmd.Parameters.AddWithValue("@PermissionId", permissionId);
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

        public int DeleteByRoleId(int roleId)
        {
            return DeleteWhere("RoleId", roleId, false);
        }

        public int DeleteByPermissionId(int permissionId)
        {
            return DeleteWhere("PermissionId", permissionId, false);
        }

        private static RolePermissionModel Map(SqlDataReader reader)
        {
            return new RolePermissionModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                RoleId = Convert.ToInt32(reader["RoleId"]),
                PermissionId = Convert.ToInt32(reader["PermissionId"]),
                CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
            };
        }
    }
}
