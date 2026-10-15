using System;
using System.Collections.Generic;
using System.Linq;
using JtecFactory.Models;
using JtecFactory.Repositories;

namespace JtecFactory.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepository employeeRepository = new EmployeeRepository();
        private readonly RoleEmployeeRepository roleEmployeeRepository = new RoleEmployeeRepository();
        private readonly RoleRepository roleRepository = new RoleRepository();

        public List<EmployeeModel> GetAll(bool includeInactive = true)
        {
            return employeeRepository.GetAll(includeInactive);
        }

        public EmployeeModel GetById(int id)
        {
            return employeeRepository.GetById(id);
        }

        public EmployeeModel GetByEmployeeCode(string employeeCode)
        {
            return employeeRepository.GetByEmployeeCode(employeeCode);
        }

        public EmployeeModel GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            EmployeeModel employee = employeeRepository.GetByUsername(username.Trim());
            return employee != null && employee.IsActive ? employee : null;
        }

        public EmployeeModel Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            var employee = employeeRepository.GetByUsername(username.Trim());
            if (employee == null || !employee.IsActive)
                return null;

            if (!PasswordHasher.VerifyPassword(password, employee.PasswordHash))
                return null;

            return employee;
        }

        public int Add(EmployeeModel employee, IEnumerable<int> roleIds)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
                throw new ArgumentException("EmployeeCode is required.", nameof(employee));
            if (string.IsNullOrWhiteSpace(employee.Username))
                throw new ArgumentException("Username is required.", nameof(employee));
            if (string.IsNullOrWhiteSpace(employee.FullName))
                throw new ArgumentException("FullName is required.", nameof(employee));
            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
                throw new ArgumentException("Password is required for a new employee.", nameof(employee));
            employee.EmployeeCode = employee.EmployeeCode.Trim();
            employee.Username = employee.Username.Trim();
            employee.FullName = employee.FullName.Trim();
            if (employeeRepository.ExistsUsername(employee.Username.Trim()))
                throw new InvalidOperationException("This username already exists.");
            if (employeeRepository.ExistsEmployeeCode(employee.EmployeeCode))
                throw new InvalidOperationException("This employee code already exists.");

            employee.created_at = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                employee.PasswordHash = PasswordHasher.HashPassword(employee.PasswordHash);
            }

            var employeeId = employeeRepository.Insert(employee);
            if (employeeId <= 0)
                return 0;

            SaveRoleAssignments(employeeId, roleIds ?? Enumerable.Empty<int>());
            return employeeId;
        }

        public bool Update(EmployeeModel employee, IEnumerable<int> roleIds)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
                throw new ArgumentException("EmployeeCode is required.", nameof(employee));
            if (string.IsNullOrWhiteSpace(employee.Username))
                throw new ArgumentException("Username is required.", nameof(employee));
            if (string.IsNullOrWhiteSpace(employee.FullName))
                throw new ArgumentException("FullName is required.", nameof(employee));
            employee.EmployeeCode = employee.EmployeeCode.Trim();
            employee.Username = employee.Username.Trim();
            employee.FullName = employee.FullName.Trim();
            if (employeeRepository.ExistsUsername(employee.Username.Trim(), employee.Id))
                throw new InvalidOperationException("This username already exists.");
            if (employeeRepository.ExistsEmployeeCode(employee.EmployeeCode, employee.Id))
                throw new InvalidOperationException("This employee code already exists.");

            if (string.IsNullOrWhiteSpace(employee.PasswordHash))
            {
                EmployeeModel existingEmployee = employeeRepository.GetById(employee.Id);
                if (existingEmployee == null)
                    return false;
                employee.PasswordHash = existingEmployee.PasswordHash;
            }
            else
            {
                employee.PasswordHash = PasswordHasher.HashPassword(employee.PasswordHash);
            }

            employee.updated_at = DateTime.UtcNow;
            var updated = employeeRepository.Update(employee);
            if (!updated)
                return false;

            SaveRoleAssignments(employee.Id, roleIds ?? Enumerable.Empty<int>());
            return true;
        }

        public bool Delete(int id)
        {
            return employeeRepository.SoftDelete(id);
        }

        public bool SetRoles(int employeeId, IEnumerable<int> roleIds)
        {
            if (employeeRepository.GetById(employeeId) == null)
                return false;

            SaveRoleAssignments(employeeId, roleIds ?? Enumerable.Empty<int>());
            return true;
        }

        public List<int> GetRoleIdsForEmployee(int employeeId)
        {
            return roleEmployeeRepository.GetByEmployeeId(employeeId).Select(item => item.RoleId).Distinct().ToList();
        }

        public List<RoleModel> GetRolesForEmployee(int employeeId)
        {
            var roleIds = GetRoleIdsForEmployee(employeeId);
            var result = new List<RoleModel>();
            foreach (var roleId in roleIds)
            {
                var role = roleRepository.GetById(roleId);
                if (role != null)
                    result.Add(role);
            }
            return result;
        }

        private void SaveRoleAssignments(int employeeId, IEnumerable<int> roleIds)
        {
            var ids = roleIds == null ? Enumerable.Empty<int>() : roleIds.Distinct().Where(id => id > 0).ToList();
            roleEmployeeRepository.DeleteByEmployeeId(employeeId);
            foreach (var roleId in ids)
            {
                if (roleRepository.GetById(roleId) != null)
                    roleEmployeeRepository.Insert(roleId, employeeId);
            }
        }
    }
}
