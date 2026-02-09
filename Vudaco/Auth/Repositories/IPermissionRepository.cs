
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vudaco.Auth.Dtos;
using Vudaco.Auth.Models;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;

namespace Vudaco.Auth.Repositories
{
    public interface IPermissionRepository : IBaseRepository<Permission>
    {
        Task<object> GetPermissionByUserAsync(RolePermissionDto RolePermissionDto, int page, int pageSize, CancellationToken cancellationToken);
        Task<List<Role>> GetRole(RolePermissionDto RolePermissionDto);
        Task<Role> ShowRole(int id);
    }
}
