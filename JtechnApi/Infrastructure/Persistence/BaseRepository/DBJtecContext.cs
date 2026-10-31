using JtechnApi.Accessorys.Models;
using JtechnApi.Departments.Models;
using JtechnApi.Employees.Models;
using JtechnApi.Exams.Models;
using JtechnApi.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace JtechnApi.Shares.BaseRepository
{
    public class DBJtecContext : DbContext
    {
        public DbSet<Employee> Employee { get; set; }
        public DbSet<DepartmentExam> DepartmentExams { get; set; }
        public DbSet<OnlineExamAttemptRecord> OnlineExamAttempts { get; set; }
        public DbSet<EmployeeDepartment> EmployeeDepartments { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RoleEmployee> RoleEmployees { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        public DBJtecContext(DbContextOptions<DBJtecContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuider)
        {
            base.OnModelCreating(modelBuider);
            modelBuider.Entity<DepartmentExam>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<OnlineExamAttemptRecord>().HasIndex(e => e.AttemptKey)
                .IsUnique().HasFilter("[attempt_key] IS NOT NULL");
            modelBuider.Entity<OnlineExamAttemptRecord>().HasIndex(e => new { e.EmployeeId, e.CatalogExamId, e.SubmittedAt });
            modelBuider.Entity<DepartmentExam>().HasIndex(e => e.Code).IsUnique();
            modelBuider.Entity<EmployeeDepartment>().HasIndex(a => new { a.EmployeeId, a.DepartmentId }).IsUnique();
            modelBuider.Entity<EmployeeDepartment>().HasQueryFilter(a => a.DeletedAt == null);
            modelBuider.Entity<Department>().HasIndex(d => d.Code).IsUnique();
            modelBuider.Entity<Department>().HasQueryFilter(d => d.DeletedAt == null);
            modelBuider.Entity<Employee>().HasQueryFilter(e => e.DeletedAt == null);
        }           
    }
}
