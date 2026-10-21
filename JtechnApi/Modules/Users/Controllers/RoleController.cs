using JtechnApi.Controllers;
using JtechnApi.Users.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using JtechnApi.Users.Dtos;
using JtechnApi.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JtechnApi.Users.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository repository;

        public RoleController(IRoleRepository repository) => this.repository = repository;

        [HttpPost("permissions/sync")]
        [Authorize(Policy = "Permission:PERMISSIONS.ASSIGN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SynchronizePermissions() => (await repository.SynchronizePermissions()).ToActionResult(this);


        [HttpGet("roles")]
        [Authorize(Policy = "Permission:ROLES.VIEW")]
        public async Task<IActionResult> GetRoles()
        {
            return (await repository.GetRoles()).ToActionResult(this);
        }

        [HttpGet("roles/{id:int}")]
        [Authorize(Policy = "Permission:ROLES.VIEW")]
        public async Task<IActionResult> GetRole(int id)
        {
            return (await repository.GetRole(id)).ToActionResult(this);
        }

        [HttpGet("permissions")]
        [Authorize(Policy = "Permission:PERMISSIONS.VIEW")]
        public async Task<IActionResult> GetPermissions()
        {
            return (await repository.GetPermissions()).ToActionResult(this);
        }

        [HttpGet("assignable-employees")]
        [Authorize(Policy = "Permission:EMPLOYEES.VIEW")]
        public async Task<IActionResult> GetAssignableEmployees(string q = null)
        {
            return (await repository.GetAssignableEmployees(q)).ToActionResult(this);
        }

        [HttpPost("roles")]
        [Authorize(Policy = "Permission:ROLES.CREATE")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.CreateRole(input)).ToActionResult(this);
        }

        [HttpPut("roles/{id:int}")]
        [Authorize(Policy = "Permission:ROLES.UPDATE")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.UpdateRole(id, input)).ToActionResult(this);
        }

        [HttpDelete("roles/{id:int}")]
        [Authorize(Policy = "Permission:ROLES.DELETE")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            return (await repository.DeleteRole(id)).ToActionResult(this);
        }

        [HttpPut("roles/{id:int}/permissions")]
        [Authorize(Policy = "Permission:PERMISSIONS.ASSIGN")]
        public async Task<IActionResult> SetRolePermissions(
            int id,
            [FromBody] AssignRolePermissionsDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.SetRolePermissions(id, input)).ToActionResult(this);
        }

        [HttpGet("employees/{employeeId:int}/roles")]
        [Authorize(Policy = "Permission:ROLES.VIEW")]
        public async Task<IActionResult> GetEmployeeRoles(int employeeId)
        {
            return (await repository.GetEmployeeRoles(employeeId)).ToActionResult(this);
        }

        [HttpPut("departments/{departmentId:int}/roles")]
        [Authorize(Policy = "Permission:PERMISSIONS.ASSIGN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDepartmentRoles(int departmentId, [FromBody] AssignEmployeeRolesDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.AddDepartmentRoles(departmentId, input)).ToActionResult(this);
        }

        [HttpPut("employees/{employeeId:int}/roles")]
        [Authorize(Policy = "Permission:PERMISSIONS.ASSIGN")]
        public async Task<IActionResult> SetEmployeeRoles(
            int employeeId,
            [FromBody] AssignEmployeeRolesDto input)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return (await repository.SetEmployeeRoles(employeeId, input)).ToActionResult(this);
        }
    }
}
