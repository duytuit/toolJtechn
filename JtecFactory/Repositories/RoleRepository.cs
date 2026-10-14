using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using JtecFactory.Models;

namespace JtecFactory.Repositories
{
    public class RoleRepository : BaseRepository<RoleModel>
    {
        public RoleRepository()
            : base("role", true, true, true)
        {
        }

        public List<RoleModel> GetAll(bool includeInactive = true, bool includeDeleted = false)
        {
            var result = new List<RoleModel>();
            const string sql = "SELECT Id, Name, Code, Description, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM [role] " +
                              "WHERE (@includeDeleted = 1 OR DeletedAt IS NULL) AND (@includeInactive = 1 OR IsActive = 1) ORDER BY Name ASC";

            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@includeInactive", includeInactive ? 1 : 0);
                cmd.Parameters.AddWithValue("@includeDeleted", includeDeleted ? 1 : 0);
                conn.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    result.Add(Map(reader));
                }
            }

            return result;
        }

        public RoleModel GetById(int id, bool includeDeleted = false)
        {
            const string sql = "SELECT Id, Name, Code, Description, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM [role] WHERE Id = @Id AND (@includeDeleted = 1 OR DeletedAt IS NULL)";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@includeDeleted", includeDeleted ? 1 : 0);
                conn.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (reader.Read())
                        return Map(reader);
                }
            }

            return null;
        }

        public RoleModel GetByCode(string code, bool includeDeleted = false)
        {
            const string sql = "SELECT Id, Name, Code, Description, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM [role] WHERE Code = @Code AND (@includeDeleted = 1 OR DeletedAt IS NULL)";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@includeDeleted", includeDeleted ? 1 : 0);
                conn.Open();
                using (var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (reader.Read())
                        return Map(reader);
                }
            }

            return null;
        }

        public int Insert(RoleModel role)
        {
            const string sql = "INSERT INTO [role] (Name, Code, Description, IsActive, CreatedAt, UpdatedAt) " +
                              "VALUES (@Name, @Code, @Description, @IsActive, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Name", (object)role.Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Code", (object)role.Code ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", (object)role.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", role.IsActive);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@UpdatedAt", (object)role.UpdatedAt ?? DBNull.Value);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        public bool Update(RoleModel role)
        {
            const string sql = "UPDATE [role] SET Name = @Name, Code = @Code, Description = @Description, IsActive = @IsActive, UpdatedAt = @UpdatedAt WHERE Id = @Id AND DeletedAt IS NULL";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", role.Id);
                cmd.Parameters.AddWithValue("@Name", (object)role.Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Code", (object)role.Code ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", (object)role.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", role.IsActive);
                cmd.Parameters.AddWithValue("@UpdatedAt", (object)DateTime.UtcNow ?? DBNull.Value);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool SoftDelete(int id)
        {
            return DeleteById(id) > 0;
        }

        public bool ExistsCode(string code, int? excludeId = null)
        {
            const string sql = "SELECT COUNT(1) FROM [role] WHERE Code = @Code AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Code", code);
                cmd.Parameters.AddWithValue("@ExcludeId", excludeId ?? (object)DBNull.Value);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }

        public bool IsAssignedToEmployees(int roleId)
        {
            const string sql = "SELECT COUNT(1) FROM role_employee WHERE RoleId = @RoleId";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@RoleId", roleId);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }

        private static RoleModel Map(SqlDataReader reader)
        {
            return new RoleModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"] as string,
                Code = reader["Code"] as string,
                Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CreatedAt = reader["CreatedAt"] == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UpdatedAt"]),
                DeletedAt = reader["DeletedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedAt"])
            };
        }
    }
}
