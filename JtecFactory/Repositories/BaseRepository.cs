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
        private readonly bool hasUpdatedAt;
        private readonly bool hasDeletedAt;

        protected BaseRepository(
            string tableName,
            bool hasIsActive = false,
            bool hasUpdatedAt = false,
            bool hasDeletedAt = false)
        {
            if (string.IsNullOrWhiteSpace(tableName) ||
                !Regex.IsMatch(tableName, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            {
                throw new ArgumentException("A valid SQL table name is required.", nameof(tableName));
            }

            this.tableName = "[" + tableName + "]";
            this.hasIsActive = hasIsActive;
            this.hasUpdatedAt = hasUpdatedAt;
            this.hasDeletedAt = hasDeletedAt;
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
                ? "UPDATE " + tableName + " SET DeletedAt = @DeletedAt" +
                  (hasIsActive ? ", IsActive = 0" : string.Empty) +
                  (hasUpdatedAt ? ", UpdatedAt = @DeletedAt" : string.Empty) +
                  " WHERE " + quotedColumn + " = @Value AND DeletedAt IS NULL"
                : "DELETE FROM " + tableName + " WHERE " + quotedColumn + " = @Value";

            using (var connection = CreateConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Value", SqlDbType.Int).Value = value;
                if (useSoftDelete)
                    command.Parameters.Add("@DeletedAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public bool RestoreById(int id)
        {
            EnsureSoftDeleteSupported(true);
            var sql = "UPDATE " + tableName + " SET DeletedAt = NULL" +
                      (hasIsActive ? ", IsActive = 1" : string.Empty) +
                      (hasUpdatedAt ? ", UpdatedAt = SYSUTCDATETIME()" : string.Empty) +
                      " WHERE Id = @Id AND DeletedAt IS NOT NULL";

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
            if (useSoftDelete && !hasDeletedAt)
            {
                throw new InvalidOperationException(
                    "Soft delete is not enabled for this repository. Use a physical delete instead.");
            }
        }
    }
}
