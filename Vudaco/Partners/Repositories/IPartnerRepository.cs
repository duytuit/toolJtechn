using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vudaco.Employees.Models;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Partners.Repositories
{
    public interface IPartnerRepository : IBaseRepository<Employee>
    {
    }
}
