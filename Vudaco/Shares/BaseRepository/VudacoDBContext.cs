

using Microsoft.EntityFrameworkCore;
using Vudaco.Auth.Models;

namespace Vudaco.Shares.BaseRepository
{
    public class VudacoDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public VudacoDBContext(DbContextOptions<VudacoDBContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuider)
        {
            modelBuider.Entity<User>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Permission>().HasQueryFilter(e => e.DeletedAt == null);
            modelBuider.Entity<Role>().HasQueryFilter(e => e.DeletedAt == null);
        }           
    }
}
