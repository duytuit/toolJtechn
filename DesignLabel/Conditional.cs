using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WindowsFormsApp5
{
    public partial class Conditional : Form
    {
        private readonly string listMachineCutFilePath = Path.Combine(
          AppDomain.CurrentDomain.BaseDirectory,
          "ListMachineCut.json");
        public Conditional()
        {
            InitializeComponent();
            lboxmay.DoubleClick += lboxmay_DoubleClick;
            LoadMachineCutList();
        }
        private void LoadMachineCutList()
        {
            try
            {
                if (!File.Exists(listMachineCutFilePath))
                {
                    return;
                }

                var machineCodes = JsonConvert.DeserializeObject<List<string>>(
                    File.ReadAllText(listMachineCutFilePath));

                if (machineCodes == null)
                {
                    return;
                }

                foreach (string machineCode in machineCodes
                    .Where(code => !string.IsNullOrWhiteSpace(code))
                    .Select(code => code.Trim())
                    .Distinct())
                {
                    lboxmay.Items.Add(machineCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc danh sách máy: " + ex.Message);
            }
        }
        public Conditional(string machineName) : this()
        {
          
        }

        private void btnConditional_Click(object sender, EventArgs e)
        {
            string machineName = lbMayCat.Text.Trim();

            if (string.IsNullOrWhiteSpace(machineName))
            {
                MessageBox.Show("Không xác định được tên máy.");
                return;
            }

            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                machineName = machineName.Replace(invalidCharacter, '_');
            }

            var condition = new
            {
                loai_day = GetPartCodes(richTextBox1.Text),
                tanshi = GetPartCodes(richTextBox2.Text),
                gom = GetPartCodes(richTextBox3.Text)
            };

            string json = JsonConvert.SerializeObject(condition, Formatting.Indented);
            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                machineName + ".json");

            try
            {
                File.WriteAllText(filePath, json);
                MessageBox.Show("Đã lưu file " + machineName + ".json");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu file JSON: " + ex.Message);
            }
        }

        private List<string> GetPartCodes(string text)
        {
            return text
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .ToList();

        }

        private void LoadMachineCondition(string machineName)
        {
            richTextBox1.Text = "";
            richTextBox2.Text = "";
            richTextBox3.Text = "";
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                machineName = machineName.Replace(invalidCharacter, '_');
            }

            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                machineName + ".json");

            if (!File.Exists(filePath))
            {
                return;
            }

            try
            {
                var condition = JsonConvert.DeserializeObject<ConditionData>(File.ReadAllText(filePath));

                if (condition == null)
                {
                    return;
                }

                richTextBox1.Text = string.Join(",", condition.loai_day ?? new List<string>());
                richTextBox2.Text = string.Join(",", condition.tanshi ?? new List<string>());
                richTextBox3.Text = string.Join(",", condition.gom ?? new List<string>());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc file điều kiện: " + ex.Message);
            }
        }
        private void lboxmay_DoubleClick(object sender, EventArgs e)
        {
            if (lboxmay.SelectedIndex < 0)
            {
                return;
            }

            string machineName = lboxmay.SelectedItem.ToString();
            lbMayCat.Text = machineName;
            LoadMachineCondition(machineName);
        }
        private class ConditionData
        {
            public List<string> loai_day { get; set; }
            public List<string> tanshi { get; set; }
            public List<string> gom { get; set; }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            richTextBox2.Text = "";
            richTextBox3.Text = "";
            lbMayCat.Text = "";
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

            string fileName = machineCode;
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidCharacter, '_');
            }

            string filePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                fileName + ".json");

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                lboxmay.Items.RemoveAt(lboxmay.SelectedIndex);
                SaveMachineCutList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa dữ liệu máy: " + ex.Message);
            }
        }
        private void SaveMachineCutList()
        {
            try
            {
                var machineCodes = lboxmay.Items.Cast<string>().ToList();
                string json = JsonConvert.SerializeObject(machineCodes, Formatting.Indented);
                File.WriteAllText(listMachineCutFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu danh sách máy: " + ex.Message);
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
    }
}
