using JtechnApi.Accessorys.Models;
using JtechnApi.Departments.Models;
using JtechnApi.Employees.Models;
using JtechnApi.Exams.Models;
using JtechnApi.ProductionPlans.Models;
using JtechnApi.Requireds.Models;
using JtechnApi.Umesens.Models;
using JtechnApi.UploadDatas.Models;
using JtechnApi.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace JtechnApi.Shares.BaseRepository
{
    public class DBContext : DbContext
    {
        public DbSet<Exam> Exam { get; set; }
        public DbSet<Admin> Admin { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmployeeDepartment> EmployeeDepartment { get; set; }
        public DbSet<ProductionPlan> ProductionPlan { get; set; }
        public DbSet<SignatureSubmission> SignatureSubmission { get; set; }
        public DbSet<Umesen> Umesen { get; set; }
        public DbSet<UploadData> UploadData { get; set; }
        public DbSet<Accessory> Accessory { get; set; }
        public DbSet<Required> Required { get; set; }
        public DbSet<TempRequired> TempRequired { get; set; }
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuider)
        {
            modelBuider.Entity<Employee>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<TempRequired>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<Required>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<Exam>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<Accessory>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<SignatureSubmission>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<UploadData>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<ProductionPlan>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<Umesen>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<Department>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<EmployeeDepartment>().HasQueryFilter(e => e.Deleted_at == null);
        }           
    }
}
