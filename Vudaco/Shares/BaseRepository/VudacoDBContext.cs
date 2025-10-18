

using Microsoft.EntityFrameworkCore;
using Vudaco.Activitys.Models;
using Vudaco.Auth.Models;
using Vudaco.Category.Models;
using Vudaco.ContractFile.Models;
using Vudaco.Departments.Models;
using Vudaco.Employees.Models;
using Vudaco.Partners.Models;
using Vudaco.Storage.Models;
using Vudaco.Vehicle.Models;

namespace Vudaco.Shares.BaseRepository
{
    public class VudacoDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Activity> Activitys { get; set; }
        public DbSet<AdministrativeFeeCategory> AdministrativeFeeCategorys { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<Branch> Branchs { get; set; }
        public DbSet<FundCategory> FundCategorys { get; set; }
        public DbSet<HouseholdFeeCategory> HouseholdFeeCategorys { get; set; }
        public DbSet<IncomeCategory> IncomeCategorys { get; set; }
        public DbSet<FileInfo> FileInfos { get; set; }
        public DbSet<FileInfoDetail> FileInfoDetails { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeDepartment> EmployeeDepartments { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<PartnerDetail> PartnerDetails { get; set; }
        public DbSet<Storages> Storages { get; set; }
        public DbSet<UserStorage> UserStorages { get; set; }
        public DbSet<Vehicles> Vehicles { get; set; }
        public DbSet<VehicleDispatch> VehicleDispatchs { get; set; }
        public VudacoDBContext(DbContextOptions<VudacoDBContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuider)
        {
            modelBuider.Entity<User>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Permission>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Role>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<AdministrativeFeeCategory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Bank>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Branch>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<FundCategory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<HouseholdFeeCategory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<IncomeCategory>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<FileInfo>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<FileInfoDetail>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Department>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Employee>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<EmployeeDepartment>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Partner>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<PartnerDetail>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Storages>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<UserStorage>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Vehicles>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<VehicleDispatch>().HasQueryFilter(e => e.DeletedAt == null);
        }           
    }
}
