using System;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using JtecFactory.Models;
using JtecFactory.Navigation;
using JtecFactory.Services;
using MenuItem = JtecFactory.Navigation.MenuItem;

namespace JtecFactory.Forms
{
    internal sealed class FrmEmployee : BaseFunctionForm
    {
        private static readonly string[] Departments =
        {
            "PD-QLSX",
            "GA- Humanresorce",
            "PD-Cắt",
            "PD-Lắp ráp",
            "PD-Kĩ thuật",
            "PD-Buredo",
            "PD-KTNQ",
            "PD-Dập",
            "PD-KTTM",
            "PD-Cắm",
            "PD-Kho xuất",
            "PD-Kho Nhập"
        };

        private readonly EmployeeService employeeService = new EmployeeService();
        private readonly RoleService roleService = new RoleService();
        private readonly DataGridView employeeGrid = new DataGridView();
        private readonly Button addButton = new Button();
        private readonly Button editButton = new Button();
        private readonly Button deleteButton = new Button();
        private readonly Button refreshButton = new Button();
        private readonly Button syncExcelButton = new Button();
        private readonly TextBox searchTextBox = new TextBox();
        private readonly ComboBox statusFilter = new ComboBox();
        private List<EmployeeModel> employees = new List<EmployeeModel>();

        internal FrmEmployee(MenuItem menu, IPermissionService permissionService, Action backAction)
            : base(menu.Name, menu.Description, menu.Id, permissionService, backAction)
        {
            BuildLayout();
            ApplyPermissions(addButton, editButton, deleteButton, null);
            syncExcelButton.Visible = permissionService == null ||
                (permissionService.CanAdd(menu.Id) && permissionService.CanEdit(menu.Id));
            LoadEmployees();
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
            ConfigureButton(addButton, "Thêm");
            ConfigureButton(editButton, "Sửa");
            ConfigureButton(deleteButton, "Xóa");
            ConfigureButton(refreshButton, "Làm mới");
            ConfigureButton(syncExcelButton, "Đồng bộ Excel");
            syncExcelButton.Width = 130;
            searchTextBox.Width = 190;
            searchTextBox.Height = 34;
            searchTextBox.Font = new Font("Segoe UI", 9.5F);
            searchTextBox.Margin = new Padding(8, 5, 8, 2);
            searchTextBox.TextChanged += EmployeeFilterChanged;
            statusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            statusFilter.Width = 145;
            statusFilter.Height = 34;
            statusFilter.Font = new Font("Segoe UI", 9.5F);
            statusFilter.Margin = new Padding(0, 5, 8, 2);
            statusFilter.Items.AddRange(new object[] { "Tất cả trạng thái", "Đang hoạt động", "Ngừng hoạt động" });
            statusFilter.SelectedIndex = 0;
            statusFilter.SelectedIndexChanged += EmployeeFilterChanged;
            addButton.Click += AddEmployee;
            editButton.Click += EditEmployee;
            deleteButton.Click += DeleteEmployee;
            refreshButton.Click += delegate { LoadEmployees(); };
            syncExcelButton.Click += SyncEmployeesFromExcel;
            toolbar.Controls.AddRange(new Control[]
            {
                addButton, editButton, deleteButton, refreshButton, syncExcelButton,
                new Label
                {
                    Text = "Tìm:",
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(4, 10, 0, 0)
                },
                searchTextBox, statusFilter
            });

            employeeGrid.Dock = DockStyle.Fill;
            employeeGrid.AllowUserToAddRows = false;
            employeeGrid.AllowUserToDeleteRows = false;
            employeeGrid.AllowUserToResizeRows = false;
            employeeGrid.AutoGenerateColumns = false;
            employeeGrid.BackgroundColor = Color.White;
            employeeGrid.BorderStyle = BorderStyle.FixedSingle;
            employeeGrid.MultiSelect = false;
            employeeGrid.ReadOnly = true;
            employeeGrid.RowHeadersVisible = false;
            employeeGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            employeeGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            employeeGrid.Columns.Add(TextColumn("EmployeeCode", "Mã nhân viên"));
            employeeGrid.Columns.Add(TextColumn("Username", "Tài khoản"));
            employeeGrid.Columns.Add(TextColumn("FullName", "Họ và tên"));
            employeeGrid.Columns.Add(TextColumn("Department", "Bộ phận"));
            employeeGrid.Columns.Add(DateColumn("DateOfBirth", "Ngày sinh"));
            employeeGrid.Columns.Add(TextColumn("Gender", "Giới tính"));
            employeeGrid.Columns.Add(TextColumn("Position", "Chức vụ"));
            employeeGrid.Columns.Add(DateColumn("JoinDate", "Ngày vào"));
            employeeGrid.Columns.Add(DateColumn("ProbationEndDate", "Kết thúc thử việc"));
            employeeGrid.Columns.Add(DateColumn("LeaveDate", "Ngày nghỉ"));
            employeeGrid.Columns.Add(TextColumn("Email", "Email"));
            employeeGrid.Columns.Add(TextColumn("Phone", "Điện thoại"));
            employeeGrid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsActive",
                HeaderText = "Đang hoạt động",
                FillWeight = 75,
                ReadOnly = true
            });
            employeeGrid.CellDoubleClick += delegate { EditEmployee(this, EventArgs.Empty); };

            page.Controls.Add(toolbar, 0, 0);
            page.Controls.Add(employeeGrid, 0, 1);
            ContentPanel.Controls.Add(page);
        }

        private static DataGridViewTextBoxColumn TextColumn(string property, string title)
        {
            return new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                HeaderText = title,
                ReadOnly = true
            };
        }

        private static DataGridViewTextBoxColumn DateColumn(string property, string title)
        {
            var column = TextColumn(property, title);
            column.DefaultCellStyle.Format = "dd/MM/yyyy";
            return column;
        }

        private static void ConfigureButton(Button button, string text)
        {
            button.Text = text;
            button.Width = 100;
            button.Height = 34;
            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = Color.FromArgb(37, 99, 235);
            button.ForeColor = Color.White;
            button.Margin = new Padding(0, 2, 8, 2);
        }

        private void LoadEmployees()
        {
            employees = employeeService.GetAll(true);
            ApplyEmployeeFilter();
        }

        private async void SyncEmployeesFromExcel(object sender, EventArgs e)
        {
            const string filePath = @"\\192.168.207.6\jtecdata\QUAN LY SAN XUAT\2.Quản Lý Người-Giờ công-Thành tích\DSCN\Theo dõi nhân sự nhà máy.xlsx";
            if (MessageBox.Show(this, "Đồng bộ danh sách nhân sự từ file Excel? Tài khoản trùng mã sẽ được cập nhật.",
                    "Xác nhận đồng bộ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            syncExcelButton.Enabled = false;
            syncExcelButton.Text = "Đang đọc file...";
            Cursor previousCursor = Cursor;
            Cursor = Cursors.WaitCursor;
            try
            {
                var progress = new Progress<EmployeeSyncProgress>(status =>
                {
                    syncExcelButton.Text = status.Message;
                    if (status.RefreshGrid)
                    {
                        try
                        {
                            LoadEmployees();
                        }
                        catch (Exception exception)
                        {
                            ShowError(exception);
                        }
                    }
                });
                EmployeeSyncResult result = await Task.Run(() => SynchronizeEmployeesFromExcel(filePath, progress));
                LoadEmployees();
                string summary = string.Format("Đã thêm: {0}\nĐã cập nhật: {1}\nRole bộ phận mới: {2}\nBỏ qua: {3}\nLỗi: {4}",
                    result.Added, result.Updated, result.RolesCreated, result.Skipped, result.Errors.Count);
                if (result.Errors.Count > 0)
                    summary += "\n\n" + string.Join("\n", result.Errors.Take(5));
                MessageBox.Show(this, summary, "Kết quả đồng bộ", MessageBoxButtons.OK,
                    result.Errors.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
            finally
            {
                Cursor = previousCursor;
                syncExcelButton.Text = "Đồng bộ Excel";
                syncExcelButton.Enabled = true;
            }
        }

        private EmployeeSyncResult SynchronizeEmployeesFromExcel(string filePath, IProgress<EmployeeSyncProgress> progress)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Không tìm thấy file Excel hoặc không có quyền truy cập.", filePath);

            var result = new EmployeeSyncResult();
            var processedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var departmentRoles = roleService.GetAll();
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var headerRow = worksheet.RowsUsed().FirstOrDefault(row =>
                    row.CellsUsed().Any(cell => NormalizeHeader(cell.GetString()) == "macode") &&
                    row.CellsUsed().Any(cell => NormalizeHeader(cell.GetString()) == "hovaten"));
                if (headerRow == null)
                    throw new InvalidDataException("Không tìm thấy hàng tiêu đề có cột 'Mã Code' và 'Họ Và Tên'.");

                var columns = headerRow.CellsUsed().ToDictionary(
                    cell => NormalizeHeader(cell.GetString()), cell => cell.Address.ColumnNumber);
                int codeColumn = columns["macode"];
                int nameColumn = columns["hovaten"];
                int statusColumn = columns["tinhtrang"];
                var dataRows = worksheet.RowsUsed().Where(row => row.RowNumber() >= 26).ToList();
                int totalRows = dataRows.Count;
                for (int rowIndex = 0; rowIndex < totalRows; rowIndex++)
                {
                    var row = dataRows[rowIndex];
                    if (rowIndex > 0 && rowIndex % 50 == 0)
                        ReportSyncProgress(progress, rowIndex, totalRows, result, rowIndex % 500 == 0);

                    string code = worksheet.Cell(row.RowNumber(), codeColumn).GetFormattedString().Trim();
                    string fullName = worksheet.Cell(row.RowNumber(), nameColumn).GetString().Trim();
                    string workStatus = NormalizeHeader(worksheet.Cell(row.RowNumber(), statusColumn).GetString());
                    if (workStatus != "lamviec" || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(fullName))
                    {
                        result.Skipped++;
                        continue;
                    }
                    if (!processedCodes.Add(code))
                    {
                        result.Skipped++;
                        continue;
                    }

                    try
                    {
                        string department = ReadText(worksheet, row.RowNumber(), columns, "bophan");
                        RoleModel departmentRole = null;
                        if (!string.IsNullOrWhiteSpace(department))
                        {
                            bool roleCreated;
                            departmentRole = GetOrCreateDepartmentRole(department, departmentRoles, out roleCreated);
                            if (roleCreated)
                                result.RolesCreated++;
                        }

                        var employee = employeeService.GetByEmployeeCode(code);
                        bool isNew = employee == null;
                        if (isNew)
                        {
                            employee = new EmployeeModel
                            {
                                EmployeeCode = code,
                                Username = code,
                                PasswordHash = Guid.NewGuid().ToString("N"),
                                IsActive = true
                            };
                        }

                        employee.EmployeeCode = code;
                        employee.Username = code;
                        employee.FullName = fullName;
                        employee.Department = department;
                        employee.DateOfBirth = ReadDate(worksheet, row.RowNumber(), columns, "ngaysinh");
                        employee.Gender = ReadText(worksheet, row.RowNumber(), columns, "gioitinh");
                        employee.Position = ReadText(worksheet, row.RowNumber(), columns, "chucvu");
                        employee.JoinDate = ReadDate(worksheet, row.RowNumber(), columns, "ngayvao");
                        employee.ProbationEndDate = ReadDate(worksheet, row.RowNumber(), columns, "ngayketthucthuviec");
                        employee.LeaveDate = ReadDate(worksheet, row.RowNumber(), columns, "ngaynghi");
                        employee.IsActive = !employee.LeaveDate.HasValue || employee.LeaveDate.Value.Date >= DateTime.Today;

                        if (isNew)
                        {
                            IEnumerable<int> roleIds = departmentRole == null
                                ? Enumerable.Empty<int>()
                                : new[] { departmentRole.Id };
                            employeeService.Add(employee, roleIds);
                            result.Added++;
                        }
                        else
                        {
                            employee.PasswordHash = null;
                            List<int> roleIds = ReplaceDepartmentRoleAssignment(
                                employeeService.GetRoleIdsForEmployee(employee.Id), departmentRole, departmentRoles);
                            if (!employeeService.Update(employee, roleIds))
                                throw new InvalidOperationException("Không thể cập nhật nhân viên.");
                            result.Updated++;
                        }
                    }
                    catch (Exception exception)
                    {
                        result.Errors.Add("Mã " + code + ": " + exception.Message);
                    }
                }
            }

            ReportSyncProgress(progress, result.Added + result.Updated + result.Skipped + result.Errors.Count,
                result.Added + result.Updated + result.Skipped + result.Errors.Count, result, true);
            return result;
        }

        private static void ReportSyncProgress(IProgress<EmployeeSyncProgress> progress, int completed, int total,
            EmployeeSyncResult result, bool refreshGrid)
        {
            progress.Report(new EmployeeSyncProgress
            {
                Message = string.Format("Đã xử lý {0}/{1} | Thêm {2} | Sửa {3} | Lỗi {4}",
                    completed, total, result.Added, result.Updated, result.Errors.Count),
                RefreshGrid = refreshGrid
            });
        }

        private sealed class EmployeeSyncProgress
        {
            public string Message { get; set; }
            public bool RefreshGrid { get; set; }
        }

        private sealed class EmployeeSyncResult
        {
            public int Added { get; set; }
            public int Updated { get; set; }
            public int RolesCreated { get; set; }
            public int Skipped { get; set; }
            public List<string> Errors { get; } = new List<string>();
        }

        private RoleModel GetOrCreateDepartmentRole(string department, List<RoleModel> roles, out bool created)
        {
            string roleName = department.Trim();
            if (roleName.Length > 100)
                roleName = roleName.Substring(0, 100);

            RoleModel existingRole = roles.FirstOrDefault(role =>
                string.Equals(role.Name, roleName, StringComparison.OrdinalIgnoreCase));
            if (existingRole != null)
            {
                created = false;
                return existingRole;
            }

            string baseCode = CreateDepartmentRoleCode(department);
            string code = baseCode;
            int suffix = 2;
            while (true)
            {
                existingRole = roles.FirstOrDefault(role =>
                    string.Equals(role.Code, code, StringComparison.OrdinalIgnoreCase));
                if (existingRole == null)
                    break;
                if (string.Equals(existingRole.Name, roleName, StringComparison.OrdinalIgnoreCase))
                {
                    created = false;
                    return existingRole;
                }

                string suffixText = "_" + (suffix++).ToString(CultureInfo.InvariantCulture);
                code = baseCode.Substring(0, Math.Min(baseCode.Length, 50 - suffixText.Length)) + suffixText;
            }

            var newRole = new RoleModel
            {
                Name = roleName,
                Code = code,
                Description = "Tự động tạo khi đồng bộ nhân viên theo bộ phận.",
                IsActive = true
            };
            newRole.Id = roleService.Add(newRole);
            if (newRole.Id <= 0)
                throw new InvalidOperationException("Không thể tạo role cho bộ phận " + roleName + ".");

            roles.Add(newRole);
            created = true;
            return newRole;
        }

        private static string CreateDepartmentRoleCode(string department)
        {
            string value = department.Replace('đ', 'd').Replace('Đ', 'D');
            string slug = new string(NormalizeHeader(value)
                .Where(character => character <= 127)
                .ToArray()).ToUpperInvariant();
            if (string.IsNullOrEmpty(slug))
                slug = "DEPARTMENT";

            string code = "DEPT_" + slug;
            return code.Length <= 50 ? code : code.Substring(0, 50);
        }

        private static List<int> ReplaceDepartmentRoleAssignment(IEnumerable<int> currentRoleIds,
            RoleModel departmentRole, IEnumerable<RoleModel> knownRoles)
        {
            var roles = knownRoles.ToList();
            var roleIds = currentRoleIds
                .Where(id => id > 0)
                .Where(id =>
                {
                    RoleModel role = roles.FirstOrDefault(item => item.Id == id);
                    return role == null || !role.Code.StartsWith("DEPT_", StringComparison.OrdinalIgnoreCase);
                })
                .Distinct()
                .ToList();

            if (departmentRole != null && !roleIds.Contains(departmentRole.Id))
                roleIds.Add(departmentRole.Id);

            return roleIds;
        }

        private static string ReadText(IXLWorksheet worksheet, int rowNumber, IDictionary<string, int> columns, string columnName)
        {
            int columnNumber;
            return columns.TryGetValue(columnName, out columnNumber)
                ? worksheet.Cell(rowNumber, columnNumber).GetString().Trim()
                : null;
        }

        private static DateTime? ReadDate(IXLWorksheet worksheet, int rowNumber, IDictionary<string, int> columns, string columnName)
        {
            int columnNumber;
            if (!columns.TryGetValue(columnName, out columnNumber))
                return null;

            string value = worksheet.Cell(rowNumber, columnNumber).GetFormattedString().Trim();
            DateTime date;
            return DateTime.TryParse(value, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out date)
                ? date.Date
                : (DateTime?)null;
        }

        private static string NormalizeHeader(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string normalized = value.Trim().Normalize(NormalizationForm.FormD);
            var result = new StringBuilder();
            foreach (char character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(character))
                    result.Append(char.ToLowerInvariant(character));
            }
            return result.ToString();
        }

        private void EmployeeFilterChanged(object sender, EventArgs e)
        {
            ApplyEmployeeFilter();
        }

        private void ApplyEmployeeFilter()
        {
            string search = searchTextBox.Text.Trim();
            IEnumerable<EmployeeModel> filteredEmployees = employees;

            if (!string.IsNullOrEmpty(search))
            {
                filteredEmployees = filteredEmployees.Where(employee =>
                    Contains(employee.EmployeeCode, search) ||
                    Contains(employee.Username, search) ||
                    Contains(employee.FullName, search) ||
                    Contains(employee.Email, search) ||
                    Contains(employee.Phone, search));
            }

            if (statusFilter.SelectedIndex == 1)
                filteredEmployees = filteredEmployees.Where(employee => employee.IsActive);
            else if (statusFilter.SelectedIndex == 2)
                filteredEmployees = filteredEmployees.Where(employee => !employee.IsActive);

            employeeGrid.DataSource = null;
            employeeGrid.DataSource = filteredEmployees.ToList();
        }

        private static bool Contains(string value, string search)
        {
            return !string.IsNullOrEmpty(value) &&
                value.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0;
        }

        private EmployeeModel SelectedEmployee
        {
            get
            {
                return employeeGrid.CurrentRow == null
                    ? null
                    : employeeGrid.CurrentRow.DataBoundItem as EmployeeModel;
            }
        }

        private void AddEmployee(object sender, EventArgs e)
        {
            using (var dialog = new EmployeeEditorDialog(null))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    List<RoleModel> roles = roleService.GetAll();
                    RoleModel departmentRole = null;
                    if (!string.IsNullOrWhiteSpace(dialog.Employee.Department))
                    {
                        bool roleCreated;
                        departmentRole = GetOrCreateDepartmentRole(dialog.Employee.Department, roles, out roleCreated);
                    }
                    IEnumerable<int> roleIds = departmentRole == null
                        ? Enumerable.Empty<int>()
                        : new[] { departmentRole.Id };
                    employeeService.Add(dialog.Employee, roleIds);
                    LoadEmployees();
                }
                catch (Exception exception)
                {
                    ShowError(exception);
                }
            }
        }

        private void EditEmployee(object sender, EventArgs e)
        {
            EmployeeModel selected = SelectedEmployee;
            if (selected == null)
                return;

            EmployeeModel employee = employeeService.GetById(selected.Id);
            if (employee == null)
            {
                LoadEmployees();
                return;
            }

            using (var dialog = new EmployeeEditorDialog(employee))
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    List<RoleModel> roles = roleService.GetAll();
                    RoleModel departmentRole = null;
                    if (!string.IsNullOrWhiteSpace(dialog.Employee.Department))
                    {
                        bool roleCreated;
                        departmentRole = GetOrCreateDepartmentRole(dialog.Employee.Department, roles, out roleCreated);
                    }
                    List<int> roleIds = ReplaceDepartmentRoleAssignment(
                        employeeService.GetRoleIdsForEmployee(employee.Id), departmentRole, roles);
                    if (!employeeService.Update(dialog.Employee, roleIds))
                        throw new InvalidOperationException("Không thể cập nhật nhân viên.");
                    LoadEmployees();
                }
                catch (Exception exception)
                {
                    ShowError(exception);
                }
            }
        }

        private void DeleteEmployee(object sender, EventArgs e)
        {
            EmployeeModel employee = SelectedEmployee;
            if (employee == null)
                return;

            if (MessageBox.Show(this, "Xóa nhân viên " + employee.FullName + "?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            try
            {
                if (!employeeService.Delete(employee.Id))
                    throw new InvalidOperationException("Không thể xóa nhân viên.");
                LoadEmployees();
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
        }

        private void ShowError(Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private sealed class EmployeeEditorDialog : Form
        {
            private readonly bool isNew;
            private readonly TextBox codeTextBox = new TextBox();
            private readonly TextBox fullNameTextBox = new TextBox();
            private readonly ComboBox departmentComboBox = new ComboBox();
            private readonly DateTimePicker dateOfBirthPicker = new DateTimePicker();
            private readonly ComboBox genderComboBox = new ComboBox();
            private readonly TextBox positionTextBox = new TextBox();
            private readonly DateTimePicker joinDatePicker = new DateTimePicker();
            private readonly DateTimePicker probationEndDatePicker = new DateTimePicker();
            private readonly DateTimePicker leaveDatePicker = new DateTimePicker();
            private readonly TextBox phoneTextBox = new TextBox();
            private readonly CheckBox activeCheckBox = new CheckBox();

            public EmployeeModel Employee { get; private set; }

            public EmployeeEditorDialog(EmployeeModel employee)
            {
                isNew = employee == null;
                Employee = employee == null
                    ? new EmployeeModel { IsActive = true }
                    : new EmployeeModel
                    {
                        Id = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        Username = employee.Username,
                        FullName = employee.FullName,
                        Email = employee.Email,
                        Phone = employee.Phone,
                        Department = employee.Department,
                        DateOfBirth = employee.DateOfBirth,
                        Gender = employee.Gender,
                        Position = employee.Position,
                        JoinDate = employee.JoinDate,
                        ProbationEndDate = employee.ProbationEndDate,
                        LeaveDate = employee.LeaveDate,
                        IsActive = employee.IsActive
                    };

                Text = isNew ? "Thêm nhân viên" : "Sửa nhân viên";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                ShowInTaskbar = false;
                ClientSize = new Size(560, 500);
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
                    RowCount = 11
                };
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                for (int index = 0; index < 11; index++)
                    layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

                AddField(layout, 0, "Mã nhân viên", codeTextBox);
                AddField(layout, 1, "Họ và tên", fullNameTextBox);
                departmentComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                departmentComboBox.Items.AddRange(Departments);
                if (!string.IsNullOrWhiteSpace(Employee.Department) &&
                    !Departments.Any(department => string.Equals(department, Employee.Department, StringComparison.OrdinalIgnoreCase)))
                {
                    departmentComboBox.Items.Add(Employee.Department);
                }
                AddField(layout, 2, "Bộ phận", departmentComboBox);
                ConfigureDatePicker(dateOfBirthPicker);
                AddField(layout, 3, "Ngày sinh", dateOfBirthPicker);
                genderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                genderComboBox.Items.AddRange(new object[] { "Nam", "Nữ" });
                if (!string.IsNullOrWhiteSpace(Employee.Gender) &&
                    !genderComboBox.Items.Cast<string>().Any(gender => string.Equals(gender, Employee.Gender, StringComparison.OrdinalIgnoreCase)))
                {
                    genderComboBox.Items.Add(Employee.Gender);
                }
                AddField(layout, 4, "Giới tính", genderComboBox);
                AddField(layout, 5, "Chức vụ", positionTextBox);
                ConfigureDatePicker(joinDatePicker);
                AddField(layout, 6, "Ngày vào", joinDatePicker);
                ConfigureDatePicker(probationEndDatePicker);
                AddField(layout, 7, "Kết thúc thử việc", probationEndDatePicker);
                ConfigureDatePicker(leaveDatePicker);
                AddField(layout, 8, "Ngày nghỉ", leaveDatePicker);
                AddField(layout, 9, "Điện thoại", phoneTextBox);
                activeCheckBox.Text = "Đang hoạt động";
                activeCheckBox.AutoSize = true;
                layout.Controls.Add(new Label
                {
                    Text = "Trạng thái",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                }, 0, 10);
                layout.Controls.Add(activeCheckBox, 1, 10);

                var buttons = new FlowLayoutPanel
                {
                    Dock = DockStyle.Bottom,
                    Height = 48,
                    FlowDirection = FlowDirection.RightToLeft,
                    Padding = new Padding(8)
                };
                var saveButton = new Button
                {
                    Text = "Lưu",
                    Width = 90,
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    DialogResult = DialogResult.None
                };
                var cancelButton = new Button
                {
                    Text = "Hủy",
                    Width = 90,
                    BackColor = Color.FromArgb(37, 99, 235),
                    ForeColor = Color.White,
                    DialogResult = DialogResult.Cancel
                };
                saveButton.Click += Save;
                buttons.Controls.Add(cancelButton);
                buttons.Controls.Add(saveButton);
                Controls.Add(layout);
                Controls.Add(buttons);
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

            private static void ConfigureDatePicker(DateTimePicker picker)
            {
                picker.Format = DateTimePickerFormat.Custom;
                picker.CustomFormat = "dd/MM/yyyy";
                picker.ShowCheckBox = true;
            }

            private void PopulateFields()
            {
                codeTextBox.Text = Employee.EmployeeCode ?? string.Empty;
                fullNameTextBox.Text = Employee.FullName ?? string.Empty;
                departmentComboBox.SelectedItem = Employee.Department;
                genderComboBox.SelectedItem = Employee.Gender;
                positionTextBox.Text = Employee.Position ?? string.Empty;
                SetDatePicker(dateOfBirthPicker, Employee.DateOfBirth);
                SetDatePicker(joinDatePicker, Employee.JoinDate);
                SetDatePicker(probationEndDatePicker, Employee.ProbationEndDate);
                SetDatePicker(leaveDatePicker, Employee.LeaveDate);
                phoneTextBox.Text = Employee.Phone ?? string.Empty;
                activeCheckBox.Checked = Employee.IsActive;
            }

            private static void SetDatePicker(DateTimePicker picker, DateTime? value)
            {
                picker.Checked = value.HasValue;
                picker.Value = value.HasValue ? value.Value : DateTime.Today;
            }

            private void Save(object sender, EventArgs e)
            {
                if (string.IsNullOrWhiteSpace(codeTextBox.Text) ||
                    string.IsNullOrWhiteSpace(fullNameTextBox.Text))
                {
                    MessageBox.Show(this, "Mã nhân viên và họ tên là bắt buộc.",
                        "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Employee.EmployeeCode = codeTextBox.Text.Trim();
                Employee.Username = Employee.EmployeeCode;
                Employee.FullName = fullNameTextBox.Text.Trim();
                Employee.Department = departmentComboBox.SelectedItem as string;
                Employee.DateOfBirth = dateOfBirthPicker.Checked ? (DateTime?)dateOfBirthPicker.Value.Date : null;
                Employee.Gender = genderComboBox.SelectedItem as string;
                Employee.Position = positionTextBox.Text.Trim();
                Employee.JoinDate = joinDatePicker.Checked ? (DateTime?)joinDatePicker.Value.Date : null;
                Employee.ProbationEndDate = probationEndDatePicker.Checked ? (DateTime?)probationEndDatePicker.Value.Date : null;
                Employee.LeaveDate = leaveDatePicker.Checked ? (DateTime?)leaveDatePicker.Value.Date : null;
                Employee.PasswordHash = isNew ? Guid.NewGuid().ToString("N") : null;
                Employee.Phone = phoneTextBox.Text.Trim();
                Employee.IsActive = activeCheckBox.Checked;
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
