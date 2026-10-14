using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using JtecFactory.Models;

namespace JtecFactory.Repositories
{
    public class PermissionRepository : BaseRepository<PermissionModel>
    {
        public PermissionRepository()
            : base("permission", true, true, true)
        {
        }

        public List<PermissionModel> GetAll(bool includeInactive = true, bool includeDeleted = false)
        {
            var result = new List<PermissionModel>();
            const string sql = "SELECT Id, Code, Name, Description, MenuCode, FormName, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM permission " +
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

        public PermissionModel GetById(int id, bool includeDeleted = false)
        {
            const string sql = "SELECT Id, Code, Name, Description, MenuCode, FormName, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM permission WHERE Id = @Id AND (@includeDeleted = 1 OR DeletedAt IS NULL)";
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

        public PermissionModel GetByCode(string code, bool includeDeleted = false)
        {
            const string sql = "SELECT Id, Code, Name, Description, MenuCode, FormName, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM permission WHERE Code = @Code AND (@includeDeleted = 1 OR DeletedAt IS NULL)";
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

        public List<PermissionModel> GetByMenuCode(string menuCode, bool includeDeleted = false)
        {
            var result = new List<PermissionModel>();
            const string sql = "SELECT Id, Code, Name, Description, MenuCode, FormName, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM permission WHERE MenuCode = @MenuCode AND (@includeDeleted = 1 OR (IsActive = 1 AND DeletedAt IS NULL)) ORDER BY Code ASC";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@MenuCode", menuCode);
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

        public int Insert(PermissionModel permission)
        {
            const string sql = "INSERT INTO permission (Code, Name, Description, MenuCode, FormName, IsActive, CreatedAt, UpdatedAt) " +
                              "VALUES (@Code, @Name, @Description, @MenuCode, @FormName, @IsActive, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Code", (object)permission.Code ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Name", (object)permission.Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", (object)permission.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MenuCode", (object)permission.MenuCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FormName", (object)permission.FormName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", permission.IsActive);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@UpdatedAt", (object)permission.UpdatedAt ?? DBNull.Value);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        public bool Update(PermissionModel permission)
        {
            const string sql = "UPDATE permission SET Code = @Code, Name = @Name, Description = @Description, MenuCode = @MenuCode, FormName = @FormName, IsActive = @IsActive, UpdatedAt = @UpdatedAt WHERE Id = @Id AND DeletedAt IS NULL";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", permission.Id);
                cmd.Parameters.AddWithValue("@Code", (object)permission.Code ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Name", (object)permission.Name ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", (object)permission.Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MenuCode", (object)permission.MenuCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FormName", (object)permission.FormName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", permission.IsActive);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool ExistsCode(string code, int? excludeId = null)
        {
            const string sql = "SELECT COUNT(1) FROM permission WHERE Code = @Code AND (@ExcludeId IS NULL OR Id <> @ExcludeId) AND DeletedAt IS NULL";
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

        public int EnsureActive(PermissionModel permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));
            if (string.IsNullOrWhiteSpace(permission.Code))
                throw new ArgumentException("Permission Code is required.", nameof(permission));

            const string selectSql = "SELECT Id FROM permission WITH (UPDLOCK, HOLDLOCK) WHERE Code = @Code";
            const string restoreSql = "UPDATE permission SET IsActive = 1, DeletedAt = NULL, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            const string insertSql = "INSERT INTO permission (Code, Name, Description, MenuCode, FormName, IsActive, CreatedAt, UpdatedAt) " +
                                     "VALUES (@Code, @Name, @Description, @MenuCode, @FormName, 1, @CreatedAt, @UpdatedAt); " +
                                     "SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = CreateConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction(IsolationLevel.Serializable))
                {
                    int? existingId = null;
                    using (var selectCommand = new SqlCommand(selectSql, conn, transaction))
                    {
                        selectCommand.Parameters.Add("@Code", SqlDbType.VarChar, 100).Value = permission.Code;
                        var result = selectCommand.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            existingId = Convert.ToInt32(result);
                    }

                    int permissionId;
                    if (existingId.HasValue)
                    {
                        using (var restoreCommand = new SqlCommand(restoreSql, conn, transaction))
                        {
                            restoreCommand.Parameters.Add("@Id", SqlDbType.Int).Value = existingId.Value;
                            restoreCommand.Parameters.Add("@UpdatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                            restoreCommand.ExecuteNonQuery();
                        }

                        permissionId = existingId.Value;
                    }
                    else
                    {
                        using (var insertCommand = new SqlCommand(insertSql, conn, transaction))
                        {
                            insertCommand.Parameters.Add("@Code", SqlDbType.VarChar, 100).Value = permission.Code;
                            insertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 200).Value = (object)permission.Name ?? DBNull.Value;
                            insertCommand.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object)permission.Description ?? DBNull.Value;
                            insertCommand.Parameters.Add("@MenuCode", SqlDbType.VarChar, 100).Value = (object)permission.MenuCode ?? DBNull.Value;
                            insertCommand.Parameters.Add("@FormName", SqlDbType.VarChar, 200).Value = (object)permission.FormName ?? DBNull.Value;
                            insertCommand.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                            insertCommand.Parameters.Add("@UpdatedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                            permissionId = Convert.ToInt32(insertCommand.ExecuteScalar());
                        }
                    }

                    transaction.Commit();
                    return permissionId;
                }
            }
        }

        public bool SoftDelete(int id)
        {
            return DeleteById(id) > 0;
        }

        private static PermissionModel Map(SqlDataReader reader)
        {
            return new PermissionModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                Code = reader["Code"] as string,
                Name = reader["Name"] as string,
                Description = reader["Description"] == DBNull.Value ? null : reader["Description"].ToString(),
                MenuCode = reader["MenuCode"] == DBNull.Value ? null : reader["MenuCode"].ToString(),
                FormName = reader["FormName"] == DBNull.Value ? null : reader["FormName"].ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CreatedAt = reader["CreatedAt"] == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UpdatedAt"]),
                DeletedAt = reader["DeletedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedAt"])
            };
        }
    }
}
