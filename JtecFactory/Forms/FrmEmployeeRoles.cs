using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JtecFactory.Models;
using JtecFactory.Navigation;
using JtecFactory.Repositories;
using JtecFactory.Services;
using MenuItem = JtecFactory.Navigation.MenuItem;

namespace JtecFactory.Forms
{
    internal sealed class FrmEmployeeRoles : BaseFunctionForm
    {
        private readonly EmployeeService employeeService = new EmployeeService();
        private readonly RoleRepository roleRepository = new RoleRepository();
        private readonly TextBox employeeSearchTextBox = new TextBox();
        private readonly ComboBox employeeCombo = new ComboBox();
        private readonly CheckedListBox rolesList = new CheckedListBox();
        private readonly Button saveButton = new Button();
        private List<EmployeeModel> employees = new List<EmployeeModel>();
        private System.Collections.Generic.HashSet<int> originalRoleIds =
            new System.Collections.Generic.HashSet<int>();
        private bool loadingLists;

        internal FrmEmployeeRoles(MenuItem menu, IPermissionService permissionService, Action backAction)
            : base(menu.Name, menu.Description, menu.Id, permissionService, backAction)
        {
            BuildLayout();
            ApplyPermissions(null, null, null, null);
            LoadEmployeesAndRoles();
        }

        private void BuildLayout()
        {
            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(24)
            };
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 52F));

            var employeePicker = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            employeePicker.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            employeePicker.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250F));
            employeePicker.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            employeePicker.Controls.Add(new Label
            {
                Text = "Nhân viên",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            }, 0, 0);
            employeeSearchTextBox.Dock = DockStyle.Fill;
            employeeSearchTextBox.Font = new Font("Segoe UI", 9.5F);
            employeeSearchTextBox.Margin = new Padding(3, 5, 8, 5);
            employeeSearchTextBox.TextChanged += EmployeeSearchChanged;
            var employeeSearchPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            employeeSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44F));
            employeeSearchPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            employeeSearchPanel.Controls.Add(new Label
            {
                Text = "Tìm:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            employeeSearchPanel.Controls.Add(employeeSearchTextBox, 1, 0);
            employeePicker.Controls.Add(employeeSearchPanel, 1, 0);
            employeeCombo.Dock = DockStyle.Fill;
            employeeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            employeeCombo.DisplayMember = "FullName";
            employeeCombo.ValueMember = "Id";
            employeeCombo.Format += EmployeeComboFormat;
            employeeCombo.SelectedIndexChanged += EmployeeSelected;
            employeePicker.Controls.Add(employeeCombo, 2, 0);

            rolesList.Dock = DockStyle.Fill;
            rolesList.CheckOnClick = true;
            rolesList.BorderStyle = BorderStyle.FixedSingle;
            rolesList.Font = new Font("Segoe UI", 10F);
            rolesList.DisplayMember = "Name";
            rolesList.Enabled = PermissionService == null ||
                PermissionService.CanAdd(MenuId) ||
                PermissionService.CanEdit(MenuId) ||
                PermissionService.CanDelete(MenuId);

            saveButton.Text = "Lưu nhóm quyền";
            saveButton.Dock = DockStyle.Right;
            saveButton.Width = 150;
            saveButton.FlatStyle = FlatStyle.Flat;
            saveButton.BackColor = Color.FromArgb(31, 41, 55);
            saveButton.ForeColor = Color.White;
            saveButton.Enabled = PermissionService == null ||
                PermissionService.CanAdd(MenuId) ||
                PermissionService.CanEdit(MenuId) ||
                PermissionService.CanDelete(MenuId);
            saveButton.Click += SaveAssignments;

            page.Controls.Add(employeePicker, 0, 0);
            page.Controls.Add(rolesList, 0, 1);
            page.Controls.Add(saveButton, 0, 2);
            ContentPanel.Controls.Add(page);
        }

        private void LoadEmployeesAndRoles()
        {
            loadingLists = true;
            try
            {
                List<RoleModel> roles = roleRepository.GetAll(false);
                employees = employeeService.GetAll(false);
                if (roles == null || employees == null)
                    throw new InvalidOperationException("Repository returned no list for employees or roles.");

                rolesList.DataSource = null;
                rolesList.Items.Clear();
                rolesList.Items.AddRange(roles.Cast<object>().ToArray());
                RefreshEmployeeChoices(null);
            }
            catch (Exception exception)
            {
                saveButton.Enabled = false;
                MessageBox.Show(this, exception.Message, "Không thể tải nhân viên và nhóm quyền",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                loadingLists = false;
            }

            if (employeeCombo.Items.Count == 0)
            {
                saveButton.Enabled = false;
            }
        }

        private void EmployeeSearchChanged(object sender, EventArgs e)
        {
            EmployeeModel selectedEmployee = employeeCombo.SelectedItem as EmployeeModel;
            RefreshEmployeeChoices(selectedEmployee == null ? (int?)null : selectedEmployee.Id);
        }

        private void RefreshEmployeeChoices(int? selectedEmployeeId)
        {
            string search = employeeSearchTextBox.Text.Trim();
            List<EmployeeModel> filteredEmployees = string.IsNullOrEmpty(search)
                ? employees.ToList()
                : employees.Where(employee =>
                    Contains(employee.FullName, search) ||
                    Contains(employee.EmployeeCode, search) ||
                    Contains(employee.Username, search)).ToList();

            loadingLists = true;
            employeeCombo.BeginUpdate();
            try
            {
                employeeCombo.Items.Clear();
                employeeCombo.Items.AddRange(filteredEmployees.Cast<object>().ToArray());

                int selectedIndex = selectedEmployeeId.HasValue
                    ? filteredEmployees.FindIndex(employee => employee.Id == selectedEmployeeId.Value)
                    : -1;
                employeeCombo.SelectedIndex = selectedIndex >= 0
                    ? selectedIndex
                    : (employeeCombo.Items.Count > 0 ? 0 : -1);
            }
            finally
            {
                employeeCombo.EndUpdate();
                loadingLists = false;
            }

            try
            {
                LoadAssignedRoles();
            }
            catch (Exception exception)
            {
                saveButton.Enabled = false;
                MessageBox.Show(this, exception.ToString(), "Không thể tải nhóm quyền đã gán",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool Contains(string value, string search)
        {
            return !string.IsNullOrEmpty(value) &&
                value.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private static void EmployeeComboFormat(object sender, ListControlConvertEventArgs e)
        {
            EmployeeModel employee = e.ListItem as EmployeeModel;
            if (employee != null)
                e.Value = employee.EmployeeCode + " - " + employee.FullName;
        }

        private void EmployeeSelected(object sender, EventArgs e)
        {
            if (!loadingLists)
            {
                try
                {
                    LoadAssignedRoles();
                }
                catch (Exception exception)
                {
                    saveButton.Enabled = false;
                    MessageBox.Show(this, exception.ToString(), "Không thể tải nhóm quyền đã gán",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadAssignedRoles()
        {
            if (employeeCombo == null || rolesList == null)
                return;

            EmployeeModel employee = employeeCombo.SelectedItem as EmployeeModel;
            if (employee == null)
            {
                originalRoleIds.Clear();
                for (int index = 0; index < rolesList.Items.Count; index++)
                    rolesList.SetItemChecked(index, false);
                return;
            }

            var assignedRoleIds = employeeService.GetRoleIdsForEmployee(employee.Id);
            if (assignedRoleIds == null)
                throw new InvalidOperationException("Repository returned no role assignment list.");

            originalRoleIds = new System.Collections.Generic.HashSet<int>(assignedRoleIds);
            for (int index = 0; index < rolesList.Items.Count; index++)
            {
                RoleModel role = rolesList.Items[index] as RoleModel;
                rolesList.SetItemChecked(index, role != null && assignedRoleIds.Contains(role.Id));
            }
        }

        private void SaveAssignments(object sender, EventArgs e)
        {
            EmployeeModel employee = employeeCombo.SelectedItem as EmployeeModel;
            if (employee == null)
                return;

            var roleIds = rolesList.CheckedItems.Cast<RoleModel>().Select(role => role.Id).ToList();
            bool adding = roleIds.Any(id => !originalRoleIds.Contains(id));
            bool removing = originalRoleIds.Any(id => !roleIds.Contains(id));
            if (adding && PermissionService != null &&
                !PermissionService.CanAdd(MenuId) && !PermissionService.CanEdit(MenuId))
            {
                MessageBox.Show(this, "Bạn không có quyền gán nhóm quyền mới.",
                    "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (removing && PermissionService != null &&
                !PermissionService.CanDelete(MenuId) && !PermissionService.CanEdit(MenuId))
            {
                MessageBox.Show(this, "Bạn không có quyền gỡ nhóm quyền.",
                    "Không đủ quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (!employeeService.SetRoles(employee.Id, roleIds))
                    throw new InvalidOperationException("Không tìm thấy nhân viên hoặc nhân viên đã bị xóa.");

                MessageBox.Show(this, "Đã cập nhật nhóm quyền cho nhân viên.",
                    "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                originalRoleIds = new System.Collections.Generic.HashSet<int>(roleIds);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
