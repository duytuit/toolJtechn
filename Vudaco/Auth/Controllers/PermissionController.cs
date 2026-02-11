
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vudaco.Auth.Dtos;
using Vudaco.Auth.Models;
using Vudaco.Auth.Repositories;
using Vudaco.Controllers;
using Vudaco.Shares;
using Vudaco.Shares.BaseRepository;
using Vudaco.Shares.Connects;

namespace Vudaco.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : BaseApiController
    {
        private readonly IPermissionRepository repoPermission;
        private readonly ILogger<PermissionController> _logger;
        private readonly VudacoDBContext _context;
        private readonly RedisService _redis;
        public int userId => (int)HttpContext.Items["UserId"];
        public PermissionController(ILogger<PermissionController> logger,RedisService redis, IPermissionRepository repoPermission, VudacoDBContext context)
        {
            _logger = logger;
            _redis = redis;
            this.repoPermission = repoPermission;
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> GetTask(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] RolePermissionDto RolePermissionDto = null )
        {
            // test
            var result = await repoPermission.GetPermissionByUserAsync(RolePermissionDto, page, pageSize, cancellationToken);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("role")]
        public async Task<IActionResult> GetRole(CancellationToken cancellationToken,[FromQuery] int page = 1, int pageSize = 50, [FromQuery] RolePermissionDto RolePermissionDto = null )
        {
            var result = await repoPermission.GetRole(RolePermissionDto);
            if (result == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", result);
        }
        [HttpGet("show")]
        public async Task<IActionResult> Show([FromQuery] int id)
        {
            if (id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity =  await repoPermission.ShowRole(id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            return ApiResponseResult(true, "Lấy dữ liệu thành công", entity);
        }
        [HttpPost]
        [Route("addOrUpdate")]
        public async Task<IActionResult> Create([FromBody] RolePermissionDto dto)
        {
            using var tran = await _context.Database.BeginTransactionAsync();

            try
            {
                var now = DateTime.Now;

                // 1) Lấy role nếu có
                var entity = await _context.Roles.FirstOrDefaultAsync(p => p.Id == dto.Id);

                // 2) Nếu chưa có role => tạo mới
                if (entity == null)
                {
                    entity = new Role
                    {
                        Name = dto.Name,
                        Note = dto.Note,
                        StorageId = dto.StorageId,
                        CreatedAt = now,
                        CreatedBy = userId,
                        UpdatedAt = now,
                        UpdatedBy = userId
                    };

                    _context.Roles.Add(entity);
                    await _context.SaveChangesAsync(); // để lấy entity.Id
                }
                else
                {
                    // 3) Update role
                    entity.Name = dto.Name;
                    entity.Note = dto.Note;
                    entity.UpdatedAt = now;
                    entity.UpdatedBy = userId;

                    _context.Roles.Update(entity);

                    // 4) Xóa quyền cũ
                    var oldRolePermissions = await _context.RolePermissions
                        .Where(t => t.RoleId == entity.Id)
                        .ToListAsync();

                    if (oldRolePermissions.Any())
                    {
                        _context.RolePermissions.RemoveRange(oldRolePermissions);
                        await _context.SaveChangesAsync();
                    }
                }

                // 5) Thêm lại quyền mới
                foreach (var item in dto.PermissionDetail)
                {
                    // ✅ FIX: PHẢI await
                    var permission = await _context.Permissions
                        .FirstOrDefaultAsync(x => x.PermissionName == item.permission);

                    // ✅ FIX: nếu chưa có thì mới tạo
                    if (permission == null)
                    {
                        permission = new Permission
                        {
                            Name = item.Name,
                            PermissionName = item.permission,
                            CreatedAt = now,
                            UpdatedAt = now,
                            UpdatedBy = userId
                        };

                        _context.Permissions.Add(permission);
                        await _context.SaveChangesAsync(); // để lấy permission.Id
                    }

                    var rolePermission = new RolePermission
                    {
                        PermissionId = permission.Id,
                        RoleId = entity.Id,
                        All = item.All,
                        View = item.View,
                        Add = item.Add,
                        Edit = item.Edit,
                        Delete = item.Delete
                    };

                    _context.RolePermissions.Add(rolePermission);
                }
                var pattern = $"token:*";
                var keys = _redis.GetKeysByPattern(pattern);
                foreach (var key in keys)
                {
                    await _redis.RemoveAsync(key);
                }
                // 2. Xóa DB
                var tokens = await _context.UserTokens.Where(t => t.Type != "app" && t.UserId != userId).ToListAsync();
                if (tokens.Any())
                {
                    _context.UserTokens.RemoveRange(tokens);
                }
                // 6) Save cuối
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult(true, "Cập nhật nhóm quyền thành công", entity);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException?.Message, null);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi hệ thống: " + ex.Message, null);
            }
        }

         [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody]  RolePermissionDto RolePermissionDto)
        {
            if (RolePermissionDto.Id <= 0)
            {
                return ApiResponseResult<object>(false, "Id không tồn tại", null);
            }
            var entity = _context.Roles.Find(RolePermissionDto.Id);
            if (entity == null)
            {
                return ApiResponseResult<object>(false, "Không tìm thấy dữ liệu", null);
            }
            var now = DateTime.Now;
            using var tran = await _context.Database.BeginTransactionAsync();
            try
            {
                entity.DeletedAt = now;
                entity.DeletedBy = userId;
                var getUserRole = await _context.UserRoles.Where(x=>x.RoleId == entity.Id).ToListAsync();
                if (getUserRole.Any())
                {
                    foreach (var item in getUserRole)
                    {
                       item.DeletedAt = now; 
                       item.DeletedBy = userId; 
                       _context.UserRoles.Update(item);
                    }
                }
                _context.Roles.Update(entity);
                await _context.SaveChangesAsync();
                await tran.CommitAsync();
                return ApiResponseResult(true, "Xóa nhóm quyền thành công", entity);
            }
            catch (DbUpdateException ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi khi thêm: " + ex.InnerException?.Message, null);
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();
                return ApiResponseResult<object>(false, "Lỗi hệ thống: " + ex.Message, null);
            }
        }
    }
}
