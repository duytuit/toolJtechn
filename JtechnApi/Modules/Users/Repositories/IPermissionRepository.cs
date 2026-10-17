
namespace JtechnApi.Users.Repositories
{
    public interface IPermissionRepository
    {
        Task<HashSet<string>> GetGrantedCodes(int employeeId);
        Task<bool> HasPermission(int employeeId, string permissionCode);
    }
}
