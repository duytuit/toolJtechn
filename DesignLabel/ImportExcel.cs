using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp5
{
    public partial class ImportExcel : Form
    {
        public ImportExcel()
        {
            InitializeComponent();
            lboxmay.DoubleClick += lboxmay_DoubleClick;
            LoadMachineCutList();
        }

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadExcelToGrid(openFileDialog.FileName);
            }
        }
        private void LoadExcelToGrid(string filePath)
        {
            try
            {
                DataTable dt = new DataTable();

                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);

                    // Lấy dòng đầu tiên làm tiêu đề
                    var firstRow = worksheet.FirstRowUsed();
                    int firstRowNumber = firstRow.RowNumber();

                    // Tạo cột DataTable
                    foreach (var cell in firstRow.CellsUsed())
                    {
                        string columnName = cell.GetString();

                        if (string.IsNullOrWhiteSpace(columnName))
                        {
                            columnName = "Column" + cell.Address.ColumnNumber;
                        }

                        // Tránh trùng tên cột
                        if (dt.Columns.Contains(columnName))
                        {
                            columnName += "_" + cell.Address.ColumnNumber;
                        }

                        dt.Columns.Add(columnName);
                    }

                    // Đọc dữ liệu từ dòng thứ 2
                    foreach (var row in worksheet.RowsUsed().Skip(1))
                    {
                        DataRow dataRow = dt.NewRow();

                        int columnIndex = 0;

                        foreach (var cell in row.Cells(1, dt.Columns.Count))
                        {
                            dataRow[columnIndex] = cell.GetValue<string>();
                            columnIndex++;
                        }

                        dt.Rows.Add(dataRow);
                    }
                }

                gvExcel.DataSource = dt;

                MessageBox.Show("Import Excel thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnAddConditition_Click(object sender, EventArgs e)
        {
            string machineCode = txtMaSP.Text.Trim();

            if (string.IsNullOrWhiteSpace(machineCode))
            {
                MessageBox.Show("Vui lòng nhập mã máy.");
                txtMaSP.Focus();
                return;
            }

            if (lboxmay.Items.Contains(machineCode))
            {
                MessageBox.Show("Mã máy đã tồn tại.");
                return;
            }

            lboxmay.Items.Add(machineCode);
            SaveMachineCutList();
            txtMaSP.Clear();
            txtMaSP.Focus();
        }

        private void LoadMachineCutList()
        {
            try
            {
                var configuration = Conditional.LoadMachineGroupConfiguration(Conditional.ConfigurationDirectory);
                foreach (string machineCode in configuration.Groups.SelectMany(group => group.Machines))
                {
                    lboxmay.Items.Add(machineCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc danh sách máy: " + ex.Message);
            }
        }

        private void SaveMachineCutList()
        {
            try
            {
                var configuration = Conditional.LoadMachineGroupConfiguration(Conditional.ConfigurationDirectory);
                var machineCodes = lboxmay.Items.Cast<string>().ToList();
                var retainedCodes = new HashSet<string>(machineCodes, StringComparer.OrdinalIgnoreCase);
                foreach (var group in configuration.Groups)
                    group.Machines = group.Machines.Where(retainedCodes.Remove).ToList();

                if (machineCodes.Any(code => configuration.Groups
                    .All(group => !group.Machines.Contains(code, StringComparer.OrdinalIgnoreCase))))
                {
                    if (configuration.Groups.Count == 0)
                        configuration.Groups.Add(new Conditional.MachineGroupData { Name = "Nhóm máy" });

                    var existingCodes = new HashSet<string>(
                        configuration.Groups.SelectMany(group => group.Machines),
                        StringComparer.OrdinalIgnoreCase);
                    configuration.Groups[0].Machines.AddRange(
                        machineCodes.Where(existingCodes.Add));
                }

                Conditional.SaveMachineGroupConfiguration(Conditional.ConfigurationDirectory, configuration);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu danh sách máy: " + ex.Message);
            }

        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (lboxmay.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn mã máy cần xóa.");
                return;
            }

            string machineCode = lboxmay.SelectedItem.ToString();
            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn xóa mã máy \"" + machineCode + "\"?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            lboxmay.Items.RemoveAt(lboxmay.SelectedIndex);
            SaveMachineCutList();
        }


        private void lboxmay_DoubleClick(object sender, EventArgs e)
        {
            if (lboxmay.SelectedIndex < 0)
            {
                return;
            }

            string machineName = lboxmay.SelectedItem.ToString();
            using (var conditionalForm = new Conditional(machineName))
            {
                conditionalForm.ShowDialog(this);
            }
        }
    }
}
