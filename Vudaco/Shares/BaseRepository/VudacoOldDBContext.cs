using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Shares.BaseRepository
{
    public class VudacoOldDBContext : DbContext
    {
        public VudacoOldDBContext(DbContextOptions<VudacoOldDBContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuider)
        {
            
        }
    }
}
