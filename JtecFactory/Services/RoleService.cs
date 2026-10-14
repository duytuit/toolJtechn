using System;
using System.Collections.Generic;
using JtecFactory.Models;
using JtecFactory.Repositories;

namespace JtecFactory.Services
{
    public class RoleService
    {
        private readonly RoleRepository repository = new RoleRepository();

        public List<RoleModel> GetAll(bool includeInactive = true)
        {
            return repository.GetAll(includeInactive);
        }

        public RoleModel GetById(int id)
        {
            return repository.GetById(id);
        }

        public int Add(RoleModel role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (string.IsNullOrWhiteSpace(role.Name))
                throw new ArgumentException("Role Name is required.", nameof(role));
            if (string.IsNullOrWhiteSpace(role.Code))
                throw new ArgumentException("Role Code is required.", nameof(role));
            if (repository.ExistsCode(role.Code.Trim(), null))
                throw new InvalidOperationException("This role code already exists.");

            role.IsActive = true;
            role.CreatedAt = DateTime.UtcNow;
            return repository.Insert(role);
        }

        public bool Update(RoleModel role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));
            if (string.IsNullOrWhiteSpace(role.Name))
                throw new ArgumentException("Role Name is required.", nameof(role));
            if (string.IsNullOrWhiteSpace(role.Code))
                throw new ArgumentException("Role Code is required.", nameof(role));
            if (repository.ExistsCode(role.Code.Trim(), role.Id))
                throw new InvalidOperationException("This role code already exists.");

            role.UpdatedAt = DateTime.UtcNow;
            return repository.Update(role);
        }

        public bool Delete(int id)
        {
            if (repository.IsAssignedToEmployees(id))
                throw new InvalidOperationException("This role is currently assigned to employees. Please remove the role assignments before deleting.");

            return repository.SoftDelete(id);
        }
    }
}
