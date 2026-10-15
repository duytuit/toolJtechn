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
            modelBuider.Entity<Employee>().HasQueryFilter(e => e.DeletedAt == null);
        }           
    }
}
