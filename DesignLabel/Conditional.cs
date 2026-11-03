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
            numericUpDown1.Minimum = 0;
            numericUpDown1.Maximum = int.MaxValue;
            try
            {
                numericUpDown1.Value = LoadAutomaticSettings(AppDomain.CurrentDomain.BaseDirectory).MinimumOrderQuantity;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc ngưỡng máy tự động: " + ex.Message);
            }
            numericUpDown1.ValueChanged += (s, e) => SaveAutomaticSettings();
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

            var condition = new ConditionData
            {
                loai_day = GetPartCodes(richTextBox1.Text),
                tanshi = GetPartCodes(richTextBox2.Text),
                gom = GetPartCodes(richTextBox3.Text),
                soday = GetPartCodes(richTextBox4.Text),
                IsAutomatic = checkBox1.Checked
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
                .Split(new[] { ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(code => code.Trim())
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .ToList();

        }

        private void LoadMachineCondition(string machineName)
        {
            richTextBox1.Text = "";
            richTextBox2.Text = "";
            richTextBox3.Text = "";
            richTextBox4.Text = "";
            checkBox1.Checked = false;
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
                richTextBox4.Text = string.Join(",", condition.soday ?? new List<string>());
                checkBox1.Checked = condition.IsAutomatic;
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
        private void SaveAutomaticSettings()
        {
            try
            {
                var settings = new AutomaticPrintSettings { MinimumOrderQuantity = numericUpDown1.Value };
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoMachineSettings.json"),
                    JsonConvert.SerializeObject(settings, Formatting.Indented));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu ngưỡng máy tự động: " + ex.Message);
            }
        }

        internal static AutomaticPrintSettings LoadAutomaticSettings(string directory)
        {
            var path = Path.Combine(directory, "AutoMachineSettings.json");
            var settings = File.Exists(path)
                ? JsonConvert.DeserializeObject<AutomaticPrintSettings>(File.ReadAllText(path))
                : new AutomaticPrintSettings();
            if (settings == null || settings.MinimumOrderQuantity < 0 || settings.MinimumOrderQuantity > int.MaxValue)
                throw new InvalidDataException("Ngưỡng số lượng máy tự động không hợp lệ.");
            return settings;
        }

        internal class AutomaticPrintSettings
        {
            [JsonProperty("min_order_quantity")]
            public decimal MinimumOrderQuantity { get; set; } = 7m;
        }

        internal static List<ConditionData> LoadPrintGroups(string directory)
        {
            var listPath = Path.Combine(directory, "ListMachineCut.json");
            if (!File.Exists(listPath))
                return new List<ConditionData>();

            var names = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(listPath));
            if (names == null)
                throw new InvalidDataException("Danh sách nhóm điều kiện không hợp lệ.");

            var result = new List<ConditionData>();
            foreach (var name in names.Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var fileName = name;
                foreach (var character in Path.GetInvalidFileNameChars())
                    fileName = fileName.Replace(character, '_');

                var path = Path.Combine(directory, fileName + ".json");
                // A newly added group has no condition file until its first update.
                if (!File.Exists(path))
                    continue;

                var condition = JsonConvert.DeserializeObject<ConditionData>(File.ReadAllText(path));
                if (condition == null)
                    throw new InvalidDataException("Điều kiện nhóm " + name + " không hợp lệ.");

                condition.Name = name;
                result.Add(condition);
            }
            return result;
        }

        internal class ConditionData
        {
            [JsonIgnore]
            public string Name { get; set; }
            public List<string> loai_day { get; set; }
            public List<string> tanshi { get; set; }
            public List<string> gom { get; set; }
            public List<string> soday { get; set; }
            [JsonProperty("is_automatic")]
            public bool IsAutomatic { get; set; }

            public int GetLabelMatchPriority(string senban, string wire, string terminalA, string terminalB, string sleeveA, string sleeveB)
            {
                var priority = GetMatchPriority(wire, terminalA, terminalB, sleeveA, sleeveB);
                if (priority >= 0)
                    return priority;
                return ContainsPartialCode(soday, senban) ? 3 : -1;
            }

            public int GetMatchPriority(string wire, string terminalA, string terminalB, string sleeveA, string sleeveB)
            {
                if (ContainsPartialCode(loai_day, wire))
                    return 0;
                if (ContainsCode(tanshi, terminalA) || ContainsCode(tanshi, terminalB))
                    return 1;
                if (ContainsCode(gom, sleeveA) || ContainsCode(gom, sleeveB))
                    return 2;
                return -1;
            }

            private static bool ContainsPartialCode(List<string> codes, string value)
            {
                var normalized = NormalizeCode(value);
                return normalized.Length > 0 && codes != null && codes.Any(code =>
                {
                    var condition = NormalizeCode(code);
                    return condition.Length > 0 && normalized.Contains(condition);
                });
            }

            private static bool ContainsCode(List<string> codes, string value)
            {
                var normalized = NormalizeCode(value);
                return normalized.Length > 0 && codes != null &&
                    codes.Any(code => NormalizeCode(code) == normalized);
            }

            private static string NormalizeCode(string value)
            {
                return new string((value ?? string.Empty).Where(character => !char.IsWhiteSpace(character))
                    .Select(char.ToUpperInvariant).ToArray());
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
            richTextBox2.Text = "";
            richTextBox3.Text = "";
            richTextBox4.Text = "";
            checkBox1.Checked = false;
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
