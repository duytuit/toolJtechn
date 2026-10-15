using JtechnApi.Accessorys.Models;
using JtechnApi.BorrowProducts.Models;
using JtechnApi.Departments.Models;
using JtechnApi.Employees.Models;
using JtechnApi.Exams.Models;
using JtechnApi.ProductionPlans.Models;
using JtechnApi.Requireds.Models;
using JtechnApi.Umesens.Models;
using JtechnApi.UploadDatas.Models;
using JtechnApi.UploadKTNQ.Models;
using JtechnApi.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace JtechnApi.Shares.BaseRepository
{
    public class DBJtecContext : DbContext
    {
        public DbSet<Employee> Employee { get; set; }
        public DBJtecContext(DbContextOptions<DBJtecContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuider)
        {
            modelBuider.Entity<Employee>().HasQueryFilter(e => e.DeletedAt == null);
        }           
    }
}
