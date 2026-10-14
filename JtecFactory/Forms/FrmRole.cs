using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JtecFactory.Models;
using JtecFactory.Navigation;
using JtecFactory.Services;
using MenuItem = JtecFactory.Navigation.MenuItem;

namespace JtecFactory.Forms
{
    internal sealed class FrmRole : BaseFunctionForm
    {
        private readonly RoleService roleService = new RoleService();
        private readonly DataGridView roleGrid = new DataGridView();
        private readonly Button addButton = new Button();
        private readonly Button editButton = new Button();
        private readonly Button deleteButton = new Button();
        private readonly Button refreshButton = new Button();

        internal FrmRole(MenuItem menu, IPermissionService permissionService, Action backAction)
            : base(menu.Name, menu.Description, menu.Id, permissionService, backAction)
        {
            BuildLayout();
            ApplyPermissions(addButton, editButton, deleteButton, null);
            LoadRoles();
        }

        private void BuildLayout()
        {
            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(20)
            };
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };
            ConfigureButton(addButton, "Thêm nhóm");
            ConfigureButton(editButton, "Sửa nhóm");
            ConfigureButton(deleteButton, "Xóa nhóm");
            ConfigureButton(refreshButton, "Làm mới");
            addButton.Click += AddRole;
            editButton.Click += EditRole;
            deleteButton.Click += DeleteRole;
            refreshButton.Click += delegate { LoadRoles(); };
            toolbar.Controls.AddRange(new Control[] { addButton, editButton, deleteButton, refreshButton });

            roleGrid.Dock = DockStyle.Fill;
            roleGrid.AllowUserToAddRows = false;
            roleGrid.AllowUserToDeleteRows = false;
            roleGrid.AllowUserToResizeRows = false;
            roleGrid.AutoGenerateColumns = false;
            roleGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            roleGrid.BackgroundColor = Color.White;
            roleGrid.MultiSelect = false;
            roleGrid.ReadOnly = true;
            roleGrid.RowHeadersVisible = false;
            roleGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            roleGrid.Columns.Add(TextColumn("Code", "Mã nhóm"));
            roleGrid.Columns.Add(TextColumn("Name", "Tên nhóm"));
            roleGrid.Columns.Add(TextColumn("Description", "Mô tả"));
            roleGrid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsActive",
                HeaderText = "Đang hoạt động",
                FillWeight = 70,
                ReadOnly = true
            });
            roleGrid.CellDoubleClick += delegate { EditRole(this, EventArgs.Empty); };

            page.Controls.Add(toolbar, 0, 0);
            page.Controls.Add(roleGrid, 0, 1);
            ContentPanel.Controls.Add(page);
        }

        private static DataGridViewTextBoxColumn TextColumn(string propertyName, string header)
        {
            return new DataGridViewTextBoxColumn
            {
                DataPropertyName = propertyName,
                HeaderText = header,
                ReadOnly = true
            };
        }

        private static void ConfigureButton(Button button, string text)
        {
            button.Text = text;
            button.Width = 105;
            button.Height = 34;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(31, 41, 55);
            button.ForeColor = Color.White;
            button.Margin = new Padding(0, 2, 8, 2);
        }

        private void LoadRoles()
        {
            roleGrid.DataSource = null;
            roleGrid.DataSource = roleService.GetAll(true);
        }

        private RoleModel SelectedRole
        {
            get
            {
                return roleGrid.CurrentRow == null
                    ? null
                    : roleGrid.CurrentRow.DataBoundItem as RoleModel;
            }
        }

        private void AddRole(object sender, EventArgs e)
        {
            if (ShowCreateDialog(this).HasValue)
            {
                LoadRoles();
            }
        }

        internal static int? ShowCreateDialog(IWin32Window owner)
        {
            var roleService = new RoleService();
            using (var dialog = new RoleEditorDialog(null))
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK)
                    return null;

                try
                {
                    int roleId = roleService.Add(dialog.Role);
                    return roleId > 0 ? (int?)roleId : null;
                }
                catch (Exception exception)
                {
                    MessageBox.Show(owner, exception.Message, "Không thể tạo nhóm quyền",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
        }

        private void EditRole(object sender, EventArgs e)
        {
            RoleModel selected = SelectedRole;
            if (selected == null)
                return;

            using (var dialog = new RoleEditorDialog(selected))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                ExecuteAndRefresh(() =>
                {
                    if (!roleService.Update(dialog.Role))
                        throw new InvalidOperationException("Không thể cập nhật nhóm quyền.");
                });
            }
        }

        private void DeleteRole(object sender, EventArgs e)
        {
            RoleModel role = SelectedRole;
            if (role == null)
                return;

            if (MessageBox.Show(this, "Xóa nhóm quyền \"" + role.Name + "\"?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            ExecuteAndRefresh(() =>
            {
                if (!roleService.Delete(role.Id))
                    throw new InvalidOperationException("Không thể xóa nhóm quyền.");
            });
        }

        private void ExecuteAndRefresh(Action action)
        {
            try
            {
                action();
                LoadRoles();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Không thể lưu nhóm quyền",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private sealed class RoleEditorDialog : Form
        {
            private readonly bool isNew;
            private readonly TextBox codeTextBox = new TextBox();
            private readonly TextBox nameTextBox = new TextBox();
            private readonly TextBox descriptionTextBox = new TextBox();
            private readonly CheckBox activeCheckBox = new CheckBox();

            public RoleModel Role { get; private set; }

            public RoleEditorDialog(RoleModel role)
            {
                isNew = role == null;
                Role = role == null
                    ? new RoleModel { IsActive = true }
                    : new RoleModel
                    {
                        Id = role.Id,
                        Code = role.Code,
                        Name = role.Name,
                        Description = role.Description,
                        IsActive = role.IsActive,
                        CreatedAt = role.CreatedAt
                    };

                Text = isNew ? "Tạo nhóm quyền" : "Sửa nhóm quyền";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                ClientSize = new Size(460, 280);
                BuildDialog();
                PopulateFields();
            }

            private void BuildDialog()
            {
                var layout = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(18),
                    ColumnCount = 2,
                    RowCount = 5
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120F));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
                layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
                layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

                AddField(layout, 0, "Mã nhóm", codeTextBox);
                AddField(layout, 1, "Tên nhóm", nameTextBox);
                descriptionTextBox.Multiline = true;
                descriptionTextBox.ScrollBars = ScrollBars.Vertical;
                AddField(layout, 2, "Mô tả", descriptionTextBox);
                activeCheckBox.Text = "Đang hoạt động";
                activeCheckBox.AutoSize = true;
                layout.Controls.Add(activeCheckBox, 1, 3);

                var buttons = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(0, 6, 0, 0)
                };
                var saveButton = new Button { Text = "Lưu", Width = 90 };
                var cancelButton = new Button { Text = "Hủy", Width = 90, DialogResult = DialogResult.Cancel };
                saveButton.Click += Save;
                buttons.Controls.Add(cancelButton);
                buttons.Controls.Add(saveButton);
                layout.Controls.Add(buttons, 0, 4);
                layout.SetColumnSpan(buttons, 2);
                Controls.Add(layout);
                AcceptButton = saveButton;
                CancelButton = cancelButton;
            }

            private static void AddField(TableLayoutPanel layout, int row, string labelText, Control control)
            {
                layout.Controls.Add(new Label
                {
                    Text = labelText,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, row);
                control.Dock = DockStyle.Fill;
                layout.Controls.Add(control, 1, row);
            }

            private void PopulateFields()
            {
                codeTextBox.Text = Role.Code ?? string.Empty;
                codeTextBox.Enabled = isNew;
                nameTextBox.Text = Role.Name ?? string.Empty;
                descriptionTextBox.Text = Role.Description ?? string.Empty;
                activeCheckBox.Checked = Role.IsActive;
            }

            private void Save(object sender, EventArgs e)
            {
                if (string.IsNullOrWhiteSpace(codeTextBox.Text) ||
                    string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    MessageBox.Show(this, "Mã nhóm và tên nhóm là bắt buộc.",
                        "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Role.Code = codeTextBox.Text.Trim();
                Role.Name = nameTextBox.Text.Trim();
                Role.Description = descriptionTextBox.Text.Trim();
                Role.IsActive = activeCheckBox.Checked;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
