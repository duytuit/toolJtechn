using JtechnApi.Accessorys.Models;
using JtechnApi.Departments.Models;
using JtechnApi.Exams.Models;
using Microsoft.EntityFrameworkCore;

namespace JtechnApi.Shares.BaseRepository
{
    public class DBContext : DbContext
    {
        public DbSet<Exam> Exam { get; set; }
        public DbSet<Department> Department { get; set; }
        public DbSet<Accessory> Accessory { get; set; }
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuider)
        {
            modelBuider.Entity<Exam>().HasQueryFilter(e => e.Deleted_at == null);
            modelBuider.Entity<Accessory>().HasQueryFilter(e => e.Deleted_at == null);
        }           
    }
}
