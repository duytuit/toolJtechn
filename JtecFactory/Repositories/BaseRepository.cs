using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace JtecFactory.Repositories
{
    public abstract class BaseRepository<TEntity> where TEntity : class
    {
        private readonly string tableName;
        private readonly bool hasIsActive;
        private readonly bool hasupdated_at;
        private readonly bool hasdeleted_at;

        protected BaseRepository(
            string tableName,
            bool hasIsActive = false,
            bool hasupdated_at = false,
            bool hasdeleted_at = false)
        {
            if (string.IsNullOrWhiteSpace(tableName) ||
                !Regex.IsMatch(tableName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                throw new ArgumentException("A valid SQL table name is required.", nameof(tableName));
            }

            this.tableName = "[" + tableName + "]";
            this.hasIsActive = hasIsActive;
            this.hasupdated_at = hasupdated_at;
            this.hasdeleted_at = hasdeleted_at;
        }

        protected string ConnectionString
        {
            get
            {
                var settings = ConfigurationManager.ConnectionStrings["JtecFactory"];
                if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                {
                    throw new InvalidOperationException("The JtecFactory connection string is not configured.");
                }

                return settings.ConnectionString;
            }
        }

        protected SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public int DeleteById(int id, bool useSoftDelete = true)
        {
            EnsureSoftDeleteSupported(useSoftDelete);
            return DeleteWhere("Id", id, useSoftDelete);
        }

        protected int DeleteWhere(string columnName, int value, bool useSoftDelete = true)
        {
            EnsureSoftDeleteSupported(useSoftDelete);
            if (string.IsNullOrWhiteSpace(columnName) ||
                !Regex.IsMatch(columnName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                throw new ArgumentException("A valid SQL column name is required.", nameof(columnName));
            }

            var quotedColumn = "[" + columnName + "]";
            var sql = useSoftDelete
                ? "UPDATE " + tableName + " SET deleted_at = @deleted_at" +
                  (hasIsActive ? ", IsActive = 0" : string.Empty) +
                  (hasupdated_at ? ", updated_at = @deleted_at" : string.Empty) +
                  " WHERE " + quotedColumn + " = @Value AND deleted_at IS NULL"
                : "DELETE FROM " + tableName + " WHERE " + quotedColumn + " = @Value";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Value", SqlDbType.Int).Value = value;
                if (useSoftDelete)
                    command.Parameters.Add("@deleted_at", SqlDbType.DateTime2).Value = DateTime.UtcNow;

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public bool RestoreById(int id)
        {
            EnsureSoftDeleteSupported(true);
            var sql = "UPDATE " + tableName + " SET deleted_at = NULL" +
                      (hasIsActive ? ", IsActive = 1" : string.Empty) +
                      (hasupdated_at ? ", updated_at = SYSUTCDATETIME()" : string.Empty) +
                      " WHERE Id = @Id AND deleted_at IS NOT NULL";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                connection.Open();
                return command.ExecuteNonQuery() > 0;
            }
        }

        private void EnsureSoftDeleteSupported(bool useSoftDelete)
        {
            if (useSoftDelete && !hasdeleted_at)
            {
                throw new InvalidOperationException(
                    "Soft delete is not enabled for this repository. Use a physical delete instead.");
            }
        }
    }
}
