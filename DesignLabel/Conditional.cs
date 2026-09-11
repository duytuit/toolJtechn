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
        public Conditional()
        {
            InitializeComponent();
        }

        public Conditional(string machineName) : this()
        {
            lbMayCat.Text = machineName;
            LoadMachineCondition(machineName);
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

        private class ConditionData
        {
            public List<string> loai_day { get; set; }
            public List<string> tanshi { get; set; }
            public List<string> gom { get; set; }
        }
    }
}
