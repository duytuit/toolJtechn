
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Vudaco.Auth.Models;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares;
using Vudaco.Auth.Dtos;
using Vudaco.Shares.SqlServerHelper;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Vudaco.Auth.Repositories
{
    public class PermissionRepository : BaseRepository<Permission>, IPermissionRepository
    {
        private readonly VudacoDBContext _context;
        private readonly ILogger<PermissionRepository> _logger;
        private readonly RedisService _redis;
        private readonly IConfiguration _configuration;
        public PermissionRepository(VudacoDBContext context, ILogger<PermissionRepository> logger, RedisService redis, IConfiguration configuration) : base(context)
        {
            _context = context;
            _logger = logger;
            _redis = redis;
            _configuration = configuration;
        }

        public async Task<object> GetPermissionByUserAsync(RolePermissionDto RolePermissionDto, int page, int pageSize, CancellationToken cancellationToken)
        {
           var sql = $@"
                   SELECT 
                        rp.permission_id,
                            p.name,
                            p.permission,
                        CAST(MAX(CAST(rp.[all] AS INT)) AS BIT)    AS [all],
                        CAST(MAX(CAST(rp.[view] AS INT)) AS BIT)   AS [view],
                        CAST(MAX(CAST(rp.[add] AS INT)) AS BIT)    AS [add],
                        CAST(MAX(CAST(rp.[edit] AS INT)) AS BIT)   AS [edit],
                        CAST(MAX(CAST(rp.[delete] AS INT)) AS BIT) AS [delete]
                    FROM role_permissions rp
                    JOIN user_roles ur 
                        ON ur.role_id = rp.role_id
                    JOIN roles r 
                        ON r.id = rp.role_id
                    JOIN permissions p 
                        ON p.id = rp.permission_id
                    WHERE ur.deleted_at IS NULL AND r.deleted_at IS NULL AND p.deleted_at IS NULL";
            if (RolePermissionDto.StorageId > 0)
            {
                sql += $@" AND r.storage_id = {RolePermissionDto.StorageId}";
            }
            if (RolePermissionDto.UserId > 0)
            {
                sql += $@" AND ur.user_id = {RolePermissionDto.UserId}";
            }
            sql += $@" GROUP BY rp.permission_id,p.name,p.permission";
            // _ = Task.Run(() => Helper.SendTelegramMessageAsync(sql));
            return await SqlServerHelpers.ExecuteQuerySqlAsync(_configuration.GetConnectionString("DefaultConnection"), sql, cancellationToken);
        }

       public async Task<Role?> ShowRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id);
            if (role == null) return null;

            role.RolePermissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == id)
                .Join(
                    _context.Permissions,
                    rp => rp.PermissionId,
                    p => p.Id,
                    (rp, p) => new RolePermission
                    {
                        Id = rp.Id,
                        RoleId = rp.RoleId,
                        PermissionId = rp.PermissionId,
                        // NotMapped field
                        PermissionName = p.PermissionName,
                        All = rp.All,
                        View = rp.View,
                        Add = rp.Add,
                        Edit = rp.Edit,
                        Delete = rp.Delete
                    }
                )
                .ToListAsync();

            return role;
        }
        public Task<List<Role>> GetRole(RolePermissionDto RolePermissionDto)
        {
            var query = _context.Roles.AsQueryable();

            if (RolePermissionDto.StorageId > 0)
            {
                query = query.Where(r => r.StorageId == RolePermissionDto.StorageId);
            }

            if (RolePermissionDto.RoleId > 0)
            {
                query = query.Where(r => r.Id == RolePermissionDto.RoleId);
            }

            if (RolePermissionDto.Status.HasValue)
            {
                query = query.Where(r => r.Status == RolePermissionDto.Status.Value);
            }

            return query.ToListAsync();
        }
    }
}
