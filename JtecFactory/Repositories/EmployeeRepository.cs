using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using JtecFactory.Models;

namespace JtecFactory.Repositories
{
    public class EmployeeRepository : BaseRepository<EmployeeModel>
    {
        public EmployeeRepository()
            : base("employee", true, true, true)
        {
        }

        public List<EmployeeModel> GetAll(bool includeInactive = true, bool includeDeleted = false)
        {
            var result = new List<EmployeeModel>();
            const string sql = "SELECT Id, EmployeeCode, Username, FullName, PasswordHash, Email, Phone, Department, DateOfBirth, Gender, Position, JoinDate, ProbationEndDate, LeaveDate, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM employee " +
                              "WHERE (@includeDeleted = 1 OR DeletedAt IS NULL) AND (@includeInactive = 1 OR IsActive = 1) ORDER BY FullName ASC";

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

        public EmployeeModel GetById(int id, bool includeDeleted = false)
        {
            const string sql = "SELECT Id, EmployeeCode, Username, FullName, PasswordHash, Email, Phone, Department, DateOfBirth, Gender, Position, JoinDate, ProbationEndDate, LeaveDate, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM employee WHERE Id = @Id AND (@includeDeleted = 1 OR DeletedAt IS NULL)";
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

        public EmployeeModel GetByUsername(string username, bool includeDeleted = false)
        {
            const string sql = "SELECT Id, EmployeeCode, Username, FullName, PasswordHash, Email, Phone, Department, DateOfBirth, Gender, Position, JoinDate, ProbationEndDate, LeaveDate, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM employee WHERE Username = @Username AND (@includeDeleted = 1 OR DeletedAt IS NULL)";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
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

        public EmployeeModel GetByEmployeeCode(string employeeCode, bool includeDeleted = false)
        {
            const string sql = "SELECT Id, EmployeeCode, Username, FullName, PasswordHash, Email, Phone, Department, DateOfBirth, Gender, Position, JoinDate, ProbationEndDate, LeaveDate, IsActive, CreatedAt, UpdatedAt, DeletedAt FROM employee WHERE EmployeeCode = @EmployeeCode AND (@includeDeleted = 1 OR DeletedAt IS NULL)";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
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

        public int Insert(EmployeeModel employee)
        {
            const string sql = "INSERT INTO employee (EmployeeCode, Username, FullName, PasswordHash, Email, Phone, Department, DateOfBirth, Gender, Position, JoinDate, ProbationEndDate, LeaveDate, IsActive, CreatedAt, UpdatedAt) " +
                              "VALUES (@EmployeeCode, @Username, @FullName, @PasswordHash, @Email, @Phone, @Department, @DateOfBirth, @Gender, @Position, @JoinDate, @ProbationEndDate, @LeaveDate, @IsActive, @CreatedAt, @UpdatedAt); SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeCode", (object)employee.EmployeeCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Username", (object)employee.Username ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FullName", (object)employee.FullName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PasswordHash", (object)employee.PasswordHash ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)employee.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", (object)employee.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Department", (object)employee.Department ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfBirth", (object)employee.DateOfBirth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", (object)employee.Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Position", (object)employee.Position ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@JoinDate", (object)employee.JoinDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProbationEndDate", (object)employee.ProbationEndDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LeaveDate", (object)employee.LeaveDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", employee.IsActive);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@UpdatedAt", (object)employee.UpdatedAt ?? DBNull.Value);

                conn.Open();
                var result = cmd.ExecuteScalar();
                return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
            }
        }

        public bool Update(EmployeeModel employee)
        {
            const string sql = "UPDATE employee SET EmployeeCode = @EmployeeCode, Username = @Username, FullName = @FullName, PasswordHash = @PasswordHash, Email = @Email, Phone = @Phone, Department = @Department, DateOfBirth = @DateOfBirth, Gender = @Gender, Position = @Position, JoinDate = @JoinDate, ProbationEndDate = @ProbationEndDate, LeaveDate = @LeaveDate, IsActive = @IsActive, UpdatedAt = @UpdatedAt WHERE Id = @Id AND DeletedAt IS NULL";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Id", employee.Id);
                cmd.Parameters.AddWithValue("@EmployeeCode", (object)employee.EmployeeCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Username", (object)employee.Username ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FullName", (object)employee.FullName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PasswordHash", (object)employee.PasswordHash ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Email", (object)employee.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Phone", (object)employee.Phone ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Department", (object)employee.Department ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DateOfBirth", (object)employee.DateOfBirth ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", (object)employee.Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Position", (object)employee.Position ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@JoinDate", (object)employee.JoinDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ProbationEndDate", (object)employee.ProbationEndDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@LeaveDate", (object)employee.LeaveDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", employee.IsActive);
                cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        public bool SoftDelete(int id)
        {
            return DeleteById(id) > 0;
        }

        public bool ExistsUsername(string username, int? excludeId = null)
        {
            const string sql = "SELECT COUNT(1) FROM employee WHERE Username = @Username AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@ExcludeId", excludeId ?? (object)DBNull.Value);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }

        public bool ExistsEmployeeCode(string employeeCode, int? excludeId = null)
        {
            const string sql = "SELECT COUNT(1) FROM employee WHERE EmployeeCode = @EmployeeCode AND (@ExcludeId IS NULL OR Id <> @ExcludeId)";
            using (var conn = CreateConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                cmd.Parameters.AddWithValue("@ExcludeId", excludeId ?? (object)DBNull.Value);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result != null && Convert.ToInt32(result) > 0;
            }
        }

        private static EmployeeModel Map(SqlDataReader reader)
        {
            return new EmployeeModel
            {
                Id = Convert.ToInt32(reader["Id"]),
                EmployeeCode = reader["EmployeeCode"] as string,
                Username = reader["Username"] as string,
                FullName = reader["FullName"] as string,
                PasswordHash = reader["PasswordHash"] as string,
                Email = reader["Email"] == DBNull.Value ? null : reader["Email"].ToString(),
                Phone = reader["Phone"] == DBNull.Value ? null : reader["Phone"].ToString(),
                Department = reader["Department"] == DBNull.Value ? null : reader["Department"].ToString(),
                DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DateOfBirth"]),
                Gender = reader["Gender"] == DBNull.Value ? null : reader["Gender"].ToString(),
                Position = reader["Position"] == DBNull.Value ? null : reader["Position"].ToString(),
                JoinDate = reader["JoinDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["JoinDate"]),
                ProbationEndDate = reader["ProbationEndDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ProbationEndDate"]),
                LeaveDate = reader["LeaveDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["LeaveDate"]),
                IsActive = Convert.ToBoolean(reader["IsActive"]),
                CreatedAt = reader["CreatedAt"] == DBNull.Value ? DateTime.UtcNow : Convert.ToDateTime(reader["CreatedAt"]),
                UpdatedAt = reader["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["UpdatedAt"]),
                DeletedAt = reader["DeletedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DeletedAt"])
            };
        }
    }
}
