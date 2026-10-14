using System;
using System.Collections.Generic;
using System.Linq;
using JtecFactory.Models;
using JtecFactory.Repositories;

namespace JtecFactory.Navigation
{
    public interface IMenuService
    {
        IEnumerable<MenuItem> GetParentMenus();
        IEnumerable<MenuItem> GetChildMenus(int parentId);
        IEnumerable<MenuItem> GetAllMenus();
    }

    public interface IPermissionService
    {
        event EventHandler PermissionsChanged;
        int CurrentRoleId { get; }
        bool CanView(int menuId);
        bool CanAdd(int menuId);
        bool CanEdit(int menuId);
        bool CanDelete(int menuId);
        bool CanExport(int menuId);
        IEnumerable<RoleInfo> GetRoles();
        MenuPermission GetPermission(int menuId, int roleId);
        void SetPermission(MenuPermission permission);
        void SetCurrentRole(int roleId);
    }

    public sealed class MockMenuService : IMenuService
    {
        private readonly List<MenuItem> menus = new List<MenuItem>
        {
            Parent(1, "dashboard", "Trang chủ", "Production overview", "\u25A6", "FrmDashboard", 1),
            Parent(10, "cut", "Cắt", "Products and customers", "\u25A3", null, 2),
            Child(11, 10, "product", "Products", "Manage product information", "\u25A3", "FrmProduct", 1),
            Parent(20, "dap", "Dập", "Production operations", "\u2692", null, 3),
            Child(21, 20, "cutting", "C\u1EAFt", "Cutting operation", "\u2702", null, 1),
            Parent(30, "cam", "Cắm", "Orders and customer activity", "\u25C8", null, 4),
            Child(31, 30, "orders", "Orders", "Review and manage orders", "\u2637", null, 1),
            Parent(40, "banga", "Bàn Gá", "Stock and inventory operations", "\u25A6", null, 5),
            Child(41, 40, "inventory", "Inventory", "View current stock levels", "\u25A4", null, 1),
            Child(42, 40, "import", "Import", "Receive stock into inventory", "\u21E5", null, 2),
            Child(43, 40, "export", "Export", "Issue stock from inventory", "\u21E5", null, 3, FormOpenMode.NewWindow),
            Parent(50, "buredo", "Buredo", "Production and quality reports", "\u25A5", null, 6),
            Child(52, 50, "quality-report", "Quality Report", "Inspection and defect summaries", "\u2713", null, 2),
            Parent(60, "laprap", "Lắp Ráp", "Users, roles and settings", "\u2699", null, 7),
            Child(61, 60, "quality-report", "Quality Report", "Inspection and defect summaries", "\u2713", null, 2),
            Parent(70, "thongmach", "Thông Mạch", "Users, roles and settings", "\u2699", null, 7),
            Child(71, 70, "quality-report", "Quality Report", "Inspection and defect summaries", "\u2713", null, 2),
            Parent(80, "kiemtra", "Kiểm tra", "Users, roles and settings", "\u2699", null, 7),
            Child(81, 80, "render-qr-ngoaiquan", "QR Ngoại Quan", "Quét mã đặt hàng và render QR", "\u25A6", "FrmRenderQRNgoaiQuan", 3),
            Parent(90, "kho", "Kho", "Users, roles and settings", "\u2699", null, 7),
            Child(91, 90, "quality-report", "Quality Report", "Inspection and defect summaries", "\u2713", null, 2),
            Parent(100, "ga", "GA", "Users, roles and settings", "\u2699", null, 7),
            Child(101, 100, "quality-report", "Quality Report", "Inspection and defect summaries", "\u2713", null, 2),
            Parent(110, "qlsx", "QLSX", "Users, roles and settings", "\u2699", null, 7),
            Child(111, 110, "quality-report", "Quality Report", "Inspection and defect summaries", "\u2713", null, 2),
            Parent(120, "congcu", "Công Cụ", "Users, roles and settings", "\u2699", null, 7),
            Child(121, 120, "quality-report", "Quality Report", "Inspection and defect summaries", "\u2713", null, 2),
            Parent(130, "nhanvien", "Nhân Viên", "Quản lý nhân viên và nhóm quyền", "\u2699", null, 7),
            Child(131, 130, "employee", "Danh sách nhân viên", "Thêm, sửa, xóa nhân viên", "\u2637", "FrmEmployee", 1),
            Child(132, 130, "employee_roles", "Nhân viên - nhóm quyền", "Gán nhóm quyền cho nhân viên", "\u2699", "FrmEmployeeRoles", 2),
            Parent(140, "caidat", "Cài Đặt", "Users, roles and settings", "\u2699", null, 7),
            Child(141, 140, "role_permission", "Phân quyền nhóm", "Cấu hình quyền menu và thao tác", "\u2637", "FrmRolePermission", 1),
            Child(143, 140, "role", "Quản lý nhóm quyền", "Tạo, sửa và xóa nhóm quyền", "\u2699", "FrmRole", 2),
            Child(142, 140, "settings", "Settings", "Application preferences", "\u2699", null, 3),
        };

        public IEnumerable<MenuItem> GetParentMenus()
        {
            return menus.Where(menu => !menu.ParentId.HasValue && menu.IsVisible).OrderBy(menu => menu.SortOrder).ToArray();
        }

        public IEnumerable<MenuItem> GetChildMenus(int parentId)
        {
            return menus.Where(menu => menu.ParentId == parentId && menu.IsVisible).OrderBy(menu => menu.SortOrder).ToArray();
        }

        public IEnumerable<MenuItem> GetAllMenus()
        {
            return menus.Where(menu => menu.IsVisible).OrderBy(menu => menu.SortOrder).ToArray();
        }

        private static MenuItem Parent(int id, string code, string name, string description, string icon, string formName, int sortOrder)
        {
            return Create(id, null, code, name, description, icon, formName, sortOrder, FormOpenMode.Embedded);
        }

        private static MenuItem Child(int id, int parentId, string code, string name, string description, string icon, string formName, int sortOrder, FormOpenMode openMode = FormOpenMode.Embedded)
        {
            return Create(id, parentId, code, name, description, icon, formName, sortOrder, openMode);
        }

        private static MenuItem Create(int id, int? parentId, string code, string name, string description, string icon, string formName, int sortOrder, FormOpenMode openMode)
        {
            return new MenuItem
            {
                Id = id,
                ParentId = parentId,
                Code = code,
                Name = name,
                Description = description,
                Icon = icon,
                FormName = formName,
                OpenMode = openMode,
                SortOrder = sortOrder,
                IsVisible = true
            };
        }
    }

    public sealed class MockPermissionService : IPermissionService
    {
        private readonly Dictionary<int, Dictionary<int, MenuPermission>> permissions = new Dictionary<int, Dictionary<int, MenuPermission>>();
        private readonly List<RoleInfo> roles = new List<RoleInfo>
        {
            new RoleInfo { Id = 1, Name = "Administrator" },
            new RoleInfo { Id = 2, Name = "Production User" },
            new RoleInfo { Id = 3, Name = "Warehouse User" }
        };

        public event EventHandler PermissionsChanged;

        public int CurrentRoleId { get; private set; }

        public MockPermissionService(IMenuService menuService)
        {
            CurrentRoleId = 1;
            foreach (RoleInfo role in roles)
            {
                Dictionary<int, MenuPermission> rolePermissions = new Dictionary<int, MenuPermission>();
                foreach (MenuItem menu in menuService.GetAllMenus())
                {
                    bool canView = role.Id == 1 || HasRoleAccess(role.Id, menu);
                    rolePermissions[menu.Id] = new MenuPermission
                    {
                        MenuId = menu.Id,
                        RoleId = role.Id,
                        CanView = canView,
                        CanAdd = role.Id == 1 || (role.Id == 2 && menu.ParentId == 20),
                        CanEdit = role.Id == 1 || (role.Id == 2 && menu.ParentId == 20),
                        CanDelete = role.Id == 1,
                        CanExport = role.Id == 1 || (role.Id == 3 && menu.ParentId == 40)
                    };
                }
                permissions.Add(role.Id, rolePermissions);
            }
        }

        public bool CanView(int menuId) { return GetCurrentPermission(menuId).CanView; }
        public bool CanAdd(int menuId) { return GetCurrentPermission(menuId).CanAdd; }
        public bool CanEdit(int menuId) { return GetCurrentPermission(menuId).CanEdit; }
        public bool CanDelete(int menuId) { return GetCurrentPermission(menuId).CanDelete; }
        public bool CanExport(int menuId) { return GetCurrentPermission(menuId).CanExport; }

        public IEnumerable<RoleInfo> GetRoles()
        {
            return roles.ToArray();
        }

        public MenuPermission GetPermission(int menuId, int roleId)
        {
            Dictionary<int, MenuPermission> rolePermissions;
            MenuPermission permission;
            if (permissions.TryGetValue(roleId, out rolePermissions) && rolePermissions.TryGetValue(menuId, out permission))
                return permission.Copy();
            return new MenuPermission { MenuId = menuId, RoleId = roleId };
        }

        public void SetPermission(MenuPermission permission)
        {
            Dictionary<int, MenuPermission> rolePermissions;
            if (!permissions.TryGetValue(permission.RoleId, out rolePermissions))
                return;

            rolePermissions[permission.MenuId] = permission.Copy();
            EventHandler handler = PermissionsChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public void SetCurrentRole(int roleId)
        {
            if (roles.All(role => role.Id != roleId) || CurrentRoleId == roleId)
                return;

            CurrentRoleId = roleId;
            EventHandler handler = PermissionsChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private MenuPermission GetCurrentPermission(int menuId)
        {
            return GetPermission(menuId, CurrentRoleId);
        }

        private static bool HasRoleAccess(int roleId, MenuItem menu)
        {
            if (roleId == 2)
                return menu.Id == 1 || menu.ParentId == 20 || menu.Id == 20 || menu.ParentId == 50 || menu.Id == 50;
            if (roleId == 3)
                return menu.Id == 1 || menu.ParentId == 40 || menu.Id == 40 || menu.Id == 11 || menu.Id == 10 || menu.ParentId == 50 || menu.Id == 50;
            return false;
        }
    }

    public sealed class MenuService : IMenuService
    {
        private readonly MockMenuService innerService = new MockMenuService();

        public IEnumerable<MenuItem> GetParentMenus()
        {
            return innerService.GetParentMenus();
        }

        public IEnumerable<MenuItem> GetChildMenus(int parentId)
        {
            return innerService.GetChildMenus(parentId);
        }

        public IEnumerable<MenuItem> GetAllMenus()
        {
            return innerService.GetAllMenus();
        }
    }

    public sealed class PermissionService : IPermissionService
    {
        private readonly IMenuService menuService;
        private readonly RoleRepository roleRepository = new RoleRepository();
        private readonly EmployeeRepository employeeRepository = new EmployeeRepository();
        private readonly RoleEmployeeRepository roleEmployeeRepository = new RoleEmployeeRepository();
        private readonly PermissionRepository permissionRepository = new PermissionRepository();
        private readonly RolePermissionRepository rolePermissionRepository = new RolePermissionRepository();
        private readonly bool administratorMode;

        public event EventHandler PermissionsChanged;

        public int CurrentRoleId { get; private set; }

        public PermissionService(IMenuService menuService = null, bool administratorMode = false)
        {
            this.menuService = menuService ?? new MenuService();
            this.administratorMode = administratorMode;
            CurrentRoleId = GetFirstAvailableRoleId();
        }

        public bool CanView(int menuId)
        {
            return administratorMode || GetPermission(menuId, CurrentRoleId).CanView;
        }

        public bool CanAdd(int menuId)
        {
            return administratorMode || GetPermission(menuId, CurrentRoleId).CanAdd;
        }

        public bool CanEdit(int menuId)
        {
            return administratorMode || GetPermission(menuId, CurrentRoleId).CanEdit;
        }

        public bool CanDelete(int menuId)
        {
            return administratorMode || GetPermission(menuId, CurrentRoleId).CanDelete;
        }

        public bool CanExport(int menuId)
        {
            return administratorMode || GetPermission(menuId, CurrentRoleId).CanExport;
        }

        public IEnumerable<RoleInfo> GetRoles()
        {
            return roleRepository.GetAll(false).Select(role => new RoleInfo
            {
                Id = role.Id,
                Name = role.Name,
                Code = role.Code,
                Description = role.Description,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt,
                UpdatedAt = role.UpdatedAt
            }).ToList();
        }

        public MenuPermission GetPermission(int menuId, int roleId)
        {
            var permission = new MenuPermission { MenuId = menuId, RoleId = roleId };
            var menu = menuService.GetAllMenus().FirstOrDefault(item => item.Id == menuId);
            if (menu == null)
                return permission;

            var rolePermissionIds = rolePermissionRepository.GetByRoleId(roleId).Select(item => item.PermissionId).ToList();
            var permissionsForRole = permissionRepository.GetAll(false)
                .Where(item => rolePermissionIds.Contains(item.Id))
                .ToList();

            if (!menu.ParentId.HasValue)
            {
                List<MenuItem> children = menuService.GetChildMenus(menu.Id).ToList();
                if (children.Count > 0)
                {
                    permission.CanView = children.Any(child =>
                    {
                        string childViewCode = GetPermissionCodeBase(child) + "_VIEW";
                        return permissionsForRole.Any(item =>
                            string.Equals(item.Code, childViewCode, StringComparison.OrdinalIgnoreCase));
                    });
                    return permission;
                }
            }

            var normalizedMenuCode = GetPermissionCodeBase(menu);
            permission.CanView = permissionsForRole.Any(item => string.Equals(item.Code, normalizedMenuCode + "_VIEW", StringComparison.OrdinalIgnoreCase));
            permission.CanAdd = permissionsForRole.Any(item => string.Equals(item.Code, normalizedMenuCode + "_ADD", StringComparison.OrdinalIgnoreCase));
            permission.CanEdit = permissionsForRole.Any(item => string.Equals(item.Code, normalizedMenuCode + "_EDIT", StringComparison.OrdinalIgnoreCase));
            permission.CanDelete = permissionsForRole.Any(item => string.Equals(item.Code, normalizedMenuCode + "_DELETE", StringComparison.OrdinalIgnoreCase));
            permission.CanExport = permissionsForRole.Any(item => string.Equals(item.Code, normalizedMenuCode + "_EXPORT", StringComparison.OrdinalIgnoreCase));

            if (roleId == 0 || !permissionsForRole.Any())
            {
                permission.CanView = false;
                permission.CanAdd = false;
                permission.CanEdit = false;
                permission.CanDelete = false;
                permission.CanExport = false;
            }

            return permission;
        }

        public void SetPermission(MenuPermission permission)
        {
            var role = roleRepository.GetById(permission.RoleId);
            if (role == null)
                return;

            var menu = menuService.GetAllMenus().FirstOrDefault(item => item.Id == permission.MenuId);
            if (menu == null)
                return;

            if (!menu.ParentId.HasValue && menuService.GetChildMenus(menu.Id).Any())
            {
                foreach (MenuItem child in menuService.GetChildMenus(menu.Id))
                {
                    MenuPermission childPermission = GetPermission(child.Id, permission.RoleId);
                    childPermission.CanView = permission.CanView;
                    SavePermissionActions(childPermission);
                }
            }
            else
            {
                SavePermissionActions(permission);
            }

            EventHandler handler = PermissionsChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void SavePermissionActions(MenuPermission permission)
        {
            var rolePermissionRecords = rolePermissionRepository.GetByRoleId(permission.RoleId);
            var rolePermissionIds = rolePermissionRecords.Select(item => item.PermissionId).ToList();
            var allPermissionRecords = permissionRepository.GetAll(false).ToList();
            MenuItem menu = menuService.GetAllMenus().First(item => item.Id == permission.MenuId);
            var normalizedBase = GetPermissionCodeBase(menu);
            var permissionCodes = new[]
            {
                normalizedBase + "_VIEW",
                normalizedBase + "_ADD",
                normalizedBase + "_EDIT",
                normalizedBase + "_DELETE",
                normalizedBase + "_EXPORT"
            };
            var permissionNames = new[] { "Xem", "Thêm", "Sửa", "Xóa", "Xuất" };
            bool[] desired =
            {
                permission.CanView,
                permission.CanAdd,
                permission.CanEdit,
                permission.CanDelete,
                permission.CanExport
            };

            for (int index = 0; index < permissionCodes.Length; index++)
            {
                PermissionModel record = allPermissionRecords.FirstOrDefault(item =>
                    string.Equals(item.Code, permissionCodes[index], StringComparison.OrdinalIgnoreCase));

                if (record == null && desired[index])
                {
                    int permissionId = permissionRepository.EnsureActive(new PermissionModel
                    {
                        Code = permissionCodes[index],
                        Name = menu.Name + " - " + permissionNames[index],
                        Description = permissionNames[index] + " quyền cho " + menu.Name,
                        MenuCode = GetPermissionMenuCode(menu),
                        FormName = menu.FormName,
                        IsActive = true
                    });
                    record = new PermissionModel
                    {
                        Id = permissionId,
                        Code = permissionCodes[index],
                        IsActive = true
                    };
                    allPermissionRecords.Add(record);
                }
                else if (record == null)
                {
                    record = permissionRepository.GetByCode(permissionCodes[index], true);
                    if (record == null)
                        continue;
                }

                bool assigned = rolePermissionIds.Contains(record.Id);
                if (desired[index] && !assigned)
                {
                    rolePermissionRepository.Insert(permission.RoleId, record.Id);
                    rolePermissionIds.Add(record.Id);
                }
                else if (!desired[index] && assigned)
                {
                    RolePermissionModel relation = rolePermissionRecords.First(item => item.PermissionId == record.Id);
                    rolePermissionRepository.DeleteById(relation.Id);
                }
            }
        }

        public void SetCurrentRole(int roleId)
        {
            if ((roleId != 0 && roleRepository.GetById(roleId) == null) || CurrentRoleId == roleId)
                return;

            CurrentRoleId = roleId;
            EventHandler handler = PermissionsChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public bool HasPermission(int employeeId, string permissionCode)
        {
            var employee = employeeRepository.GetById(employeeId);
            if (employee == null)
                return false;

            var roleIds = roleEmployeeRepository.GetByEmployeeId(employeeId).Select(item => item.RoleId).Distinct().ToList();
            foreach (var roleId in roleIds)
            {
                var permissionIds = rolePermissionRepository.GetByRoleId(roleId).Select(item => item.PermissionId).ToList();
                foreach (var permissionId in permissionIds)
                {
                    var lookupPermission = permissionRepository.GetById(permissionId);
                    if (lookupPermission != null && string.Equals(lookupPermission.Code, permissionCode, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        public bool CanView(int employeeId, string menuCode)
        {
            return HasPermission(employeeId, NormalizeMenuCode(menuCode) + "_VIEW");
        }

        public bool CanAdd(int employeeId, string menuCode)
        {
            return HasPermission(employeeId, NormalizeMenuCode(menuCode) + "_ADD");
        }

        public bool CanEdit(int employeeId, string menuCode)
        {
            return HasPermission(employeeId, NormalizeMenuCode(menuCode) + "_EDIT");
        }

        public bool CanDelete(int employeeId, string menuCode)
        {
            return HasPermission(employeeId, NormalizeMenuCode(menuCode) + "_DELETE");
        }

        public bool CanExport(int employeeId, string menuCode)
        {
            return HasPermission(employeeId, NormalizeMenuCode(menuCode) + "_EXPORT");
        }

        private int GetFirstAvailableRoleId()
        {
            var role = roleRepository.GetAll(false).OrderBy(item => item.Id).FirstOrDefault();
            return role != null ? role.Id : 0;
        }

        private string GetPermissionCodeBase(MenuItem menu)
        {
            string childCode = NormalizeMenuCode(menu.Code);
            if (!menu.ParentId.HasValue)
                return childCode;

            List<MenuItem> menus = menuService.GetAllMenus().ToList();
            bool hasDuplicateCode = menus.Count(item => item.ParentId.HasValue &&
                string.Equals(NormalizeMenuCode(item.Code), childCode, StringComparison.OrdinalIgnoreCase)) > 1;
            if (!hasDuplicateCode)
                return childCode;

            MenuItem parent = menus.FirstOrDefault(item => item.Id == menu.ParentId.Value);
            return parent == null ? childCode : NormalizeMenuCode(parent.Code) + "_" + childCode;
        }

        private string GetPermissionMenuCode(MenuItem menu)
        {
            string childCode = NormalizeMenuCode(menu.Code);
            string permissionCodeBase = GetPermissionCodeBase(menu);
            if (string.Equals(childCode, permissionCodeBase, StringComparison.OrdinalIgnoreCase) || !menu.ParentId.HasValue)
                return menu.Code;

            MenuItem parent = menuService.GetAllMenus().FirstOrDefault(item => item.Id == menu.ParentId.Value);
            return parent == null ? menu.Code : parent.Code + "." + menu.Code;
        }

        private static string NormalizeMenuCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var normalized = value.Trim();
            normalized = normalized.Replace("-", "_");
            normalized = normalized.Replace(" ", "_");
            normalized = normalized.Replace(".", "_");
            normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "[^A-Za-z0-9_]", "");
            return normalized.ToUpperInvariant();
        }
    }
}