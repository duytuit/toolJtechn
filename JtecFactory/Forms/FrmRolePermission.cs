using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JtecFactory.Navigation;
using MenuItem = JtecFactory.Navigation.MenuItem;

namespace JtecFactory.Forms
{
    internal sealed class FrmRolePermission : BaseFunctionForm
    {
        private const int RolePermissionMenuId = 141;
        private const int RoleManagementMenuId = 143;

        private readonly IMenuService menuService;
        private readonly IPermissionService permissionService;
        private readonly ListBox rolesList;
        private readonly Button createRoleButton;
        private readonly TreeView menuTree;
        private readonly CheckBox viewPermission;
        private readonly CheckBox addPermission;
        private readonly CheckBox editPermission;
        private readonly CheckBox deletePermission;
        private readonly CheckBox exportPermission;
        private bool updating;
        private readonly bool canEditPermissions;

        internal FrmRolePermission(IMenuService menuService, IPermissionService permissionService, Action backAction)
            : base("Phân quyền nhóm", "Cấu hình quyền menu và thao tác", RolePermissionMenuId, permissionService, backAction)
        {
            this.menuService = menuService;
            this.permissionService = permissionService;
            canEditPermissions = permissionService == null || permissionService.CanEdit(RolePermissionMenuId);

            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 210,
                FixedPanel = FixedPanel.Panel1,
                BackColor = Color.White
            };
            rolesList = new ListBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10F),
                ItemHeight = 34
            };
            var rolesPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(12)
            };
            rolesPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            rolesPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            createRoleButton = new Button
            {
                Text = "Tạo nhóm quyền",
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(31, 41, 55),
                ForeColor = Color.White,
                Enabled = permissionService == null || permissionService.CanAdd(RoleManagementMenuId)
            };
            createRoleButton.Click += CreateRole;
            rolesPanel.Controls.Add(rolesList, 0, 0);
            rolesPanel.Controls.Add(createRoleButton, 0, 1);
            menuTree = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                CheckBoxes = true,
                Font = new Font("Segoe UI", 9F),
                HideSelection = false
            };
            menuTree.Enabled = canEditPermissions;
            TableLayoutPanel permissionPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 90,
                ColumnCount = 5,
                RowCount = 2,
                Padding = new Padding(0, 12, 0, 0)
            };
            for (int index = 0; index < 5; index++)
                permissionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            permissionPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            permissionPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));

            viewPermission = CreatePermissionCheckBox("View");
            addPermission = CreatePermissionCheckBox("Add");
            editPermission = CreatePermissionCheckBox("Edit");
            deletePermission = CreatePermissionCheckBox("Delete");
            exportPermission = CreatePermissionCheckBox("Export");
            viewPermission.Enabled = canEditPermissions;
            addPermission.Enabled = canEditPermissions;
            editPermission.Enabled = canEditPermissions;
            deletePermission.Enabled = canEditPermissions;
            exportPermission.Enabled = canEditPermissions;
            CheckBox[] permissionChecks = { viewPermission, addPermission, editPermission, deletePermission, exportPermission };
            for (int index = 0; index < permissionChecks.Length; index++)
            {
                Label label = new Label
                {
                    Text = permissionChecks[index].Text,
                    Dock = DockStyle.Fill,
                    ForeColor = Color.FromArgb(107, 114, 128),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                permissionPanel.Controls.Add(label, index, 0);
                permissionChecks[index].Text = string.Empty;
                permissionChecks[index].Dock = DockStyle.Fill;
                permissionPanel.Controls.Add(permissionChecks[index], index, 1);
                permissionChecks[index].CheckedChanged += PermissionChanged;
            }

            split.Panel1.Controls.Add(rolesPanel);
            split.Panel2.Controls.Add(menuTree);
            split.Panel2.Controls.Add(permissionPanel);
            ContentPanel.Controls.Add(split);

            ReloadRoles(null);
            rolesList.SelectedIndexChanged += RoleSelected;
            menuTree.AfterSelect += MenuSelected;
            menuTree.AfterCheck += MenuChecked;
            LoadRoleMenus();
        }

        private void CreateRole(object sender, EventArgs e)
        {
            int? createdRoleId = FrmRole.ShowCreateDialog(this);
            if (!createdRoleId.HasValue)
                return;

            ReloadRoles(createdRoleId.Value);
        }

        private void ReloadRoles(int? selectedRoleId)
        {
            var roles = permissionService.GetRoles().ToList();
            rolesList.DataSource = null;
            rolesList.DataSource = roles;
            if (selectedRoleId.HasValue)
            {
                int selectedIndex = roles.FindIndex(role => role.Id == selectedRoleId.Value);
                if (selectedIndex >= 0)
                    rolesList.SelectedIndex = selectedIndex;
            }
        }

        private static CheckBox CreatePermissionCheckBox(string text)
        {
            return new CheckBox { Text = text, AutoSize = false, BackColor = Color.White };
        }

        private void RoleSelected(object sender, EventArgs e)
        {
            LoadRoleMenus();
        }

        private void LoadRoleMenus()
        {
            RoleInfo role = rolesList.SelectedItem as RoleInfo;
            if (role == null)
                return;

            updating = true;
            menuTree.BeginUpdate();
            menuTree.Nodes.Clear();
            foreach (MenuItem parent in menuService.GetParentMenus())
            {
                MenuPermission parentPermission = permissionService.GetPermission(parent.Id, role.Id);
                TreeNode parentNode = new TreeNode(parent.Name)
                {
                    Tag = parent,
                    Checked = parentPermission.CanView
                };
                foreach (MenuItem child in menuService.GetChildMenus(parent.Id))
                {
                    MenuPermission childPermission = permissionService.GetPermission(child.Id, role.Id);
                    parentNode.Nodes.Add(new TreeNode(child.Name)
                    {
                        Tag = child,
                        Checked = childPermission.CanView
                    });
                }
                menuTree.Nodes.Add(parentNode);
            }
            menuTree.EndUpdate();
            updating = false;

            if (menuTree.Nodes.Count > 0)
                menuTree.SelectedNode = menuTree.Nodes[0];
            else
                ShowPermission(null);
        }

        private void MenuSelected(object sender, TreeViewEventArgs e)
        {
            ShowPermission(e.Node.Tag as MenuItem);
        }

        private void ShowPermission(MenuItem menu)
        {
            RoleInfo role = rolesList.SelectedItem as RoleInfo;
            if (menu == null || role == null)
            {
                viewPermission.Enabled = false;
                addPermission.Enabled = false;
                editPermission.Enabled = false;
                deletePermission.Enabled = false;
                exportPermission.Enabled = false;
                return;
            }

            updating = true;
            MenuPermission permission = permissionService.GetPermission(menu.Id, role.Id);
            viewPermission.Enabled = canEditPermissions;
            addPermission.Enabled = canEditPermissions;
            editPermission.Enabled = canEditPermissions;
            deletePermission.Enabled = canEditPermissions;
            exportPermission.Enabled = canEditPermissions;
            viewPermission.Checked = permission.CanView;
            addPermission.Checked = permission.CanAdd;
            editPermission.Checked = permission.CanEdit;
            deletePermission.Checked = permission.CanDelete;
            exportPermission.Checked = permission.CanExport;
            updating = false;
        }

        private void MenuChecked(object sender, TreeViewEventArgs e)
        {
            if (updating || !canEditPermissions)
                return;

            RoleInfo role = rolesList.SelectedItem as RoleInfo;
            MenuItem menu = e.Node.Tag as MenuItem;
            if (role == null || menu == null)
                return;

            try
            {
                MenuPermission permission = permissionService.GetPermission(menu.Id, role.Id);
                permission.CanView = e.Node.Checked;
                permissionService.SetPermission(permission);
                if (!IsDisposed && !menuTree.IsDisposed && menuTree.SelectedNode == e.Node)
                    ShowPermission(menu);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Không thể lưu phân quyền",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                LoadRoleMenus();
            }
        }

        private void PermissionChanged(object sender, EventArgs e)
        {
            if (updating || !canEditPermissions)
                return;

            RoleInfo role = rolesList.SelectedItem as RoleInfo;
            MenuItem menu = menuTree.SelectedNode == null ? null : menuTree.SelectedNode.Tag as MenuItem;
            if (role == null || menu == null)
                return;

            try
            {
                MenuPermission permission = permissionService.GetPermission(menu.Id, role.Id);
                permission.CanView = viewPermission.Checked;
                permission.CanAdd = addPermission.Checked;
                permission.CanEdit = editPermission.Checked;
                permission.CanDelete = deletePermission.Checked;
                permission.CanExport = exportPermission.Checked;
                permissionService.SetPermission(permission);

                if (IsDisposed || menuTree.IsDisposed || menuTree.SelectedNode == null)
                    return;

                updating = true;
                menuTree.SelectedNode.Checked = permission.CanView;
                updating = false;
            }
            catch (Exception exception)
            {
                updating = true;
                ShowPermission(menu);
                updating = false;
                MessageBox.Show(this, exception.Message, "Không thể lưu phân quyền",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}