using JtechnApi.Shares;
using JtechnApi.Users.Dtos;

namespace JtechnApi.Users.Repositories
{
    public interface IRoleRepository
    {
        Task<RepositoryResult> SynchronizePermissions();
        Task<RepositoryResult> GetRoles();
        Task<RepositoryResult> GetRole(int id);
        Task<RepositoryResult> GetPermissions();
        Task<RepositoryResult> GetAssignableEmployees(string q = null);
        Task<RepositoryResult> CreateRole(CreateRoleDto input);
        Task<RepositoryResult> UpdateRole(int id, UpdateRoleDto input);
        Task<RepositoryResult> DeleteRole(int id);
        Task<RepositoryResult> SetRolePermissions(
            int id,
            AssignRolePermissionsDto input);
        Task<RepositoryResult> GetEmployeeRoles(int employeeId);
        Task<RepositoryResult> AddDepartmentRoles(int departmentId, AssignEmployeeRolesDto input);
        Task<RepositoryResult> SetEmployeeRoles(
            int employeeId,
            AssignEmployeeRolesDto input);
    }
}
