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
using Newtonsoft.Json.Linq;

namespace WindowsFormsApp5
{
    public partial class Conditional : Form
    {
        private static readonly string[] DefaultConditionOrder = { "loai_day", "tanshi", "gom", "soday" };
        internal static string ConfigurationDirectory
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        private readonly List<string> conditionOrder = new List<string>(DefaultConditionOrder);
        private readonly Dictionary<string, Tuple<Label, RichTextBox>> conditionColumns;
        private readonly TreeView machineGroupTree = new TreeView();
        private readonly Button btnAddParentGroup = new Button();
        private readonly CheckBox chkGroupBySensyu = new CheckBox();
        internal bool SplitIntoTwoGroups
        {
            get { return phantach2nhom.Checked; }
        }

        public Conditional()
        {
            InitializeComponent();
            conditionColumns = new Dictionary<string, Tuple<Label, RichTextBox>>(StringComparer.Ordinal)
            {
                { "loai_day", Tuple.Create(label1, richTextBox1) },
                { "tanshi", Tuple.Create(label2, richTextBox2) },
                { "gom", Tuple.Create(label3, richTextBox3) },
                { "soday", Tuple.Create(label5, richTextBox4) }
            };
            InitializeConditionColumnDragDrop();
            InitializeMachineGroupTree();
            numericUpDown1.Minimum = 0;
            numericUpDown1.Maximum = int.MaxValue;
            try
            {
                var settings = LoadAutomaticSettings(ConfigurationDirectory);
                numericUpDown1.Value = settings.MinimumOrderQuantity;
                phantach2nhom.Checked = settings.SplitIntoTwoGroups;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc ngưỡng máy tự động: " + ex.Message);
            }
            numericUpDown1.ValueChanged += (s, e) => SaveAutomaticSettings();
            phantach2nhom.CheckedChanged += (s, e) => SaveAutomaticSettings();
            LoadMachineCutList();
        }

        private void InitializeMachineGroupTree()
        {
            groupBox2.Text = "Nhóm cha / nhóm máy";
            tableLayoutPanel3.Controls.Remove(lboxmay);
            lboxmay.Dispose();
            tableLayoutPanel3.RowStyles.Clear();
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Absolute, 70F));
            tableLayoutPanel3.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            txtMaSP.Location = new Point(3, 2);
            txtMaSP.Size = new Size(120, 27);
            btnAddConditition.Location = new Point(126, 2);
            btnAddConditition.Size = new Size(82, 27);
            btnAddConditition.Text = "Thêm máy";
            btnXoa.Location = new Point(214, 2);
            btnXoa.Size = new Size(79, 27);

            btnAddParentGroup.Location = new Point(3, 34);
            btnAddParentGroup.Size = new Size(105, 27);
            btnAddParentGroup.Text = "Thêm nhóm cha";
            btnAddParentGroup.UseVisualStyleBackColor = true;
            btnAddParentGroup.Click += btnAddParentGroup_Click;
            panel3.Controls.Add(btnAddParentGroup);

            chkGroupBySensyu.Location = new Point(114, 38);
            chkGroupBySensyu.AutoSize = true;
            chkGroupBySensyu.Text = "Gom SENSYU";
            chkGroupBySensyu.Checked = true;
            chkGroupBySensyu.CheckedChanged += chkGroupBySensyu_CheckedChanged;
            panel3.Controls.Add(chkGroupBySensyu);

            machineGroupTree.Dock = DockStyle.Fill;
            machineGroupTree.HideSelection = false;
            machineGroupTree.AllowDrop = true;
            machineGroupTree.ItemHeight = 20;
            machineGroupTree.AfterSelect += machineGroupTree_AfterSelect;
            machineGroupTree.NodeMouseDoubleClick += machineGroupTree_NodeMouseDoubleClick;
            machineGroupTree.ItemDrag += machineGroupTree_ItemDrag;
            machineGroupTree.DragEnter += machineGroupTree_DragEnter;
            machineGroupTree.DragOver += machineGroupTree_DragOver;
            machineGroupTree.DragDrop += machineGroupTree_DragDrop;
            tableLayoutPanel3.Controls.Add(machineGroupTree, 0, 1);
        }

        private void machineGroupTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var node = e.Item as TreeNode;
            if (node != null)
                machineGroupTree.DoDragDrop(node, DragDropEffects.Move);
        }

        private void machineGroupTree_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(typeof(TreeNode)) ? DragDropEffects.Move : DragDropEffects.None;
        }

        private void machineGroupTree_DragOver(object sender, DragEventArgs e)
        {
            var source = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            var target = machineGroupTree.GetNodeAt(machineGroupTree.PointToClient(new Point(e.X, e.Y)));
            e.Effect = source != null && target != null &&
                !(source.Level == 0 && target.Level != 0) &&
                !IsNodeAncestor(source, target)
                ? DragDropEffects.Move
                : DragDropEffects.None;
        }

        private void machineGroupTree_DragDrop(object sender, DragEventArgs e)
        {
            var source = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            var target = machineGroupTree.GetNodeAt(machineGroupTree.PointToClient(new Point(e.X, e.Y)));
            if (source == null || target == null || source == target ||
                IsNodeAncestor(source, target))
                return;

            if (source.Level == 0 && target.Level != 0)
                return;

            TreeNodeCollection destination;
            int insertIndex;
            if (source.Level == 0)
            {
                destination = machineGroupTree.Nodes;
                insertIndex = target.Index + 1;
            }
            else if (target.Level == 0)
            {
                destination = target.Nodes;
                insertIndex = destination.Count;
            }
            else
            {
                destination = target.Parent.Nodes;
                insertIndex = destination.IndexOf(target) + 1;
            }

            var sourceCollection = source.Parent == null ? machineGroupTree.Nodes : source.Parent.Nodes;
            if (sourceCollection == destination && source.Index < insertIndex)
                insertIndex--;
            var movedNode = (TreeNode)source.Clone();
            source.Remove();
            destination.Insert(Math.Max(0, Math.Min(insertIndex, destination.Count)), movedNode);
            movedNode.EnsureVisible();
            machineGroupTree.SelectedNode = movedNode;
            SaveMachineCutList();
        }

        private static bool IsNodeAncestor(TreeNode possibleAncestor, TreeNode node)
        {
            for (var current = node.Parent; current != null; current = current.Parent)
            {
                if (current == possibleAncestor)
                    return true;
            }
            return false;
        }

        private void InitializeConditionColumnDragDrop()
        {
            foreach (var column in conditionColumns)
            {
                var header = column.Value.Item1;
                header.Cursor = Cursors.SizeAll;
                header.AllowDrop = true;
                header.MouseDown += (sender, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                        header.DoDragDrop(column.Key, DragDropEffects.Move);
                };
                header.DragEnter += (sender, e) =>
                {
                    if (e.Data.GetDataPresent(typeof(string)))
                        e.Effect = DragDropEffects.Move;
                };
                header.DragDrop += (sender, e) =>
                {
                    var source = e.Data.GetData(typeof(string)) as string;
                    if (source == null || source == column.Key)
                        return;

                    var sourceIndex = conditionOrder.IndexOf(source);
                    var targetIndex = conditionOrder.IndexOf(column.Key);
                    if (sourceIndex < 0 || targetIndex < 0)
                        return;

                    conditionOrder[sourceIndex] = column.Key;
                    conditionOrder[targetIndex] = source;
                    ApplyConditionColumnOrder();
                };
            }
        }

        private void ApplyConditionColumnOrder()
        {
            int[] leftPositions = { 3, 118, 245, 363 };
            int[] widths = { 110, 122, 114, 108 };
            for (int i = 0; i < conditionOrder.Count; i++)
            {
                var controls = conditionColumns[conditionOrder[i]];
                var header = controls.Item1;
                var textBox = controls.Item2;
                header.Location = new Point(leftPositions[i] + (widths[i] - header.Width) / 2, 51);
                textBox.Location = new Point(leftPositions[i], 67);
                textBox.Width = widths[i];
            }
        }
        private void LoadMachineCutList()
        {
            try
            {
                var configuration = LoadMachineGroupConfiguration(ConfigurationDirectory);
                foreach (var group in configuration.Groups)
                {
                    var parent = new TreeNode(group.Name) { Tag = group };
                    machineGroupTree.Nodes.Add(parent);
                    foreach (var machineCode in group.Machines)
                        parent.Nodes.Add(new TreeNode(machineCode) { Tag = machineCode });
                    parent.Expand();
                }

                if (machineGroupTree.Nodes.Count > 0)
                    machineGroupTree.SelectedNode = machineGroupTree.Nodes[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc danh sách máy: " + ex.Message);
            }
        }

        private void machineGroupTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var parent = e.Node.Level == 0 ? e.Node : e.Node.Parent;
            var group = parent == null ? null : parent.Tag as MachineGroupData;
            chkGroupBySensyu.Enabled = group != null;
            chkGroupBySensyu.Checked = group != null && group.GroupBySensyu;
        }

        private void chkGroupBySensyu_CheckedChanged(object sender, EventArgs e)
        {
            var selected = machineGroupTree.SelectedNode;
            var parent = selected == null || selected.Level == 0 ? selected : selected.Parent;
            var group = parent == null ? null : parent.Tag as MachineGroupData;
            if (group == null)
                return;

            group.GroupBySensyu = chkGroupBySensyu.Checked;
            SaveMachineCutList();
        }

        private void btnAddParentGroup_Click(object sender, EventArgs e)
        {
            var name = txtMaSP.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm cha.");
                txtMaSP.Focus();
                return;
            }

            if (machineGroupTree.Nodes.Cast<TreeNode>().Any(parentNode =>
                string.Equals(parentNode.Text, name, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Tên nhóm cha đã tồn tại.");
                return;
            }

            var group = new MachineGroupData { Name = name };
            var node = new TreeNode(group.Name) { Tag = group };
            machineGroupTree.Nodes.Add(node);
            machineGroupTree.SelectedNode = node;
            node.Expand();
            SaveMachineCutList();
            txtMaSP.Clear();
            txtMaSP.Focus();
        }

        private void machineGroupTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level != 1)
                return;

            string machineName = e.Node.Text;
            lbMayCat.Text = machineName;
            LoadMachineCondition(machineName);
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
                ConditionOrder = new List<string>(conditionOrder),
                IsAutomatic = checkBox1.Checked
            };

            string json = JsonConvert.SerializeObject(condition, Formatting.Indented);
            string filePath = Path.Combine(
                ConfigurationDirectory,
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
            conditionOrder.Clear();
            conditionOrder.AddRange(DefaultConditionOrder);
            ApplyConditionColumnOrder();
            foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
            {
                machineName = machineName.Replace(invalidCharacter, '_');
            }

            string filePath = Path.Combine(
                ConfigurationDirectory,
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
                conditionOrder.Clear();
                conditionOrder.AddRange(condition.GetConditionOrder());
                ApplyConditionColumnOrder();
                checkBox1.Checked = condition.IsAutomatic;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc file điều kiện: " + ex.Message);
            }
        }
        private void SaveAutomaticSettings()
        {
            try
            {
                var settings = new AutomaticPrintSettings
                {
                    MinimumOrderQuantity = numericUpDown1.Value,
                    SplitIntoTwoGroups = phantach2nhom.Checked
                };
                File.WriteAllText(Path.Combine(ConfigurationDirectory, "AutoMachineSettings.json"),
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

            [JsonProperty("split_into_two_groups")]
            public bool SplitIntoTwoGroups { get; set; }
        }

        internal static List<ConditionData> LoadPrintGroups(string directory)
        {
            var configuration = LoadMachineGroupConfiguration(directory);
            var result = new List<ConditionData>();
            foreach (var parent in configuration.Groups)
            {
                foreach (var name in parent.Machines)
                {
                    var fileName = name;
                    foreach (var character in Path.GetInvalidFileNameChars())
                        fileName = fileName.Replace(character, '_');

                    var path = Path.Combine(directory, fileName + ".json");
                    if (!File.Exists(path))
                        continue;

                    var condition = JsonConvert.DeserializeObject<ConditionData>(File.ReadAllText(path));
                    if (condition == null)
                        throw new InvalidDataException("Điều kiện nhóm " + name + " không hợp lệ.");

                    condition.Name = name;
                    condition.ParentName = parent.Name;
                    condition.GroupBySensyu = parent.GroupBySensyu;
                    result.Add(condition);
                }
            }
            return result;
        }

        internal static MachineGroupConfiguration LoadMachineGroupConfiguration(string directory)
        {
            var listPath = Path.Combine(directory, "ListMachineCut.json");
            if (!File.Exists(listPath))
                return new MachineGroupConfiguration();

            var token = JToken.Parse(File.ReadAllText(listPath));
            MachineGroupConfiguration configuration;
            if (token.Type == JTokenType.Array)
            {
                var legacyNames = token.ToObject<List<string>>();
                if (legacyNames == null)
                    throw new InvalidDataException("Danh sách nhóm điều kiện không hợp lệ.");
                configuration = new MachineGroupConfiguration();
                var legacyGroup = new MachineGroupData { Name = "Nhóm máy", GroupBySensyu = true };
                legacyGroup.Machines.AddRange(legacyNames);
                if (legacyGroup.Machines.Any(name => !string.IsNullOrWhiteSpace(name)))
                    configuration.Groups.Add(legacyGroup);
            }
            else if (token.Type == JTokenType.Object)
            {
                configuration = token.ToObject<MachineGroupConfiguration>();
                if (configuration == null || configuration.Groups == null)
                    throw new InvalidDataException("Cấu hình nhóm máy không hợp lệ.");
            }
            else
            {
                throw new InvalidDataException("Cấu hình nhóm máy không hợp lệ.");
            }

            var seenParents = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenMachines = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var validGroups = new List<MachineGroupData>();
            foreach (var group in configuration.Groups)
            {
                if (group == null || string.IsNullOrWhiteSpace(group.Name))
                    continue;

                group.Name = group.Name.Trim();
                if (!seenParents.Add(group.Name))
                    throw new InvalidDataException("Tên nhóm cha bị trùng: " + group.Name);

                group.Machines = (group.Machines ?? new List<string>())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name.Trim())
                    .Where(seenMachines.Add)
                    .ToList();
                validGroups.Add(group);
            }
            configuration.Groups = validGroups;
            return configuration;
        }

        internal static void SaveMachineGroupConfiguration(string directory, MachineGroupConfiguration configuration)
        {
            var path = Path.Combine(directory, "ListMachineCut.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(configuration, Formatting.Indented));
        }

        internal sealed class MachineGroupConfiguration
        {
            [JsonProperty("groups")]
            public List<MachineGroupData> Groups { get; set; } = new List<MachineGroupData>();
        }

        internal sealed class MachineGroupData
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("group_by_sensyu")]
            public bool GroupBySensyu { get; set; } = true;

            [JsonProperty("machines")]
            public List<string> Machines { get; set; } = new List<string>();
        }

        internal class ConditionData
        {
            [JsonIgnore]
            public string Name { get; set; }
            [JsonIgnore]
            public string ParentName { get; set; }
            [JsonIgnore]
            public bool GroupBySensyu { get; set; }
            public List<string> loai_day { get; set; }
            public List<string> tanshi { get; set; }
            public List<string> gom { get; set; }
            public List<string> soday { get; set; }
            [JsonProperty("condition_order")]
            public List<string> ConditionOrder { get; set; }
            [JsonProperty("is_automatic")]
            public bool IsAutomatic { get; set; }

            public List<string> GetConditionOrder()
            {
                var order = (ConditionOrder ?? new List<string>())
                    .Where(DefaultConditionOrder.Contains)
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
                order.AddRange(DefaultConditionOrder.Where(category => !order.Contains(category)));
                return order;
            }

            public int GetLabelMatchPriority(string senban, string wire, string terminalA, string terminalB, string sleeveA, string sleeveB)
            {
                return GetMatchPriority(senban, wire, terminalA, terminalB, sleeveA, sleeveB);
            }

            public string GetMatchCategory(int priority)
            {
                var order = GetConditionOrder();
                return priority >= 0 && priority < order.Count ? order[priority] : string.Empty;
            }

            public int GetMatchPriority(string senban, string wire, string terminalA, string terminalB, string sleeveA, string sleeveB)
            {
                var order = GetConditionOrder();
                var hasCondition = false;
                var matchesAllConditions = true;
                for (int priority = 0; priority < order.Count; priority++)
                {
                    List<string> codes;
                    string valueA = null;
                    string valueB = null;
                    bool partialMatch;
                    switch (order[priority])
                    {
                        case "loai_day":
                            codes = loai_day;
                            valueA = wire;
                            partialMatch = true;
                            break;
                        case "tanshi":
                            codes = tanshi;
                            valueA = terminalA;
                            valueB = terminalB;
                            partialMatch = false;
                            break;
                        case "gom":
                            codes = gom;
                            valueA = sleeveA;
                            valueB = sleeveB;
                            partialMatch = false;
                            break;
                        case "soday":
                            codes = soday;
                            valueA = senban;
                            partialMatch = true;
                            break;
                        default:
                            continue;
                    }

                    if (codes == null || !codes.Any(code => !string.IsNullOrWhiteSpace(code)))
                        continue;

                    hasCondition = true;
                    var matched = partialMatch
                        ? ContainsPartialCode(codes, valueA)
                        : ContainsCode(codes, valueA) || ContainsCode(codes, valueB);
                    if (!matched)
                        matchesAllConditions = false;
                }

                if (!hasCondition || !matchesAllConditions)
                    return -1;

                for (int priority = 0; priority < order.Count; priority++)
                {
                    switch (order[priority])
                    {
                        case "loai_day":
                            if (ContainsPartialCode(loai_day, wire))
                                return priority;
                            break;
                        case "tanshi":
                            if (ContainsCode(tanshi, terminalA) || ContainsCode(tanshi, terminalB))
                                return priority;
                            break;
                        case "gom":
                            if (ContainsCode(gom, sleeveA) || ContainsCode(gom, sleeveB))
                                return priority;
                            break;
                        case "soday":
                            if (ContainsPartialCode(soday, senban))
                                return priority;
                            break;
                    }
                }

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
            var selected = machineGroupTree.SelectedNode;
            if (selected == null)
            {
                MessageBox.Show("Vui lòng chọn nhóm cha hoặc nhóm máy cần xóa.");
                return;
            }

            var isParent = selected.Level == 0;
            string itemName = selected.Text;
            string prompt = isParent && selected.Nodes.Count > 0
                ? "Xóa nhóm cha \"" + itemName + "\" và toàn bộ liên kết nhóm máy con? File điều kiện của các nhóm máy sẽ được giữ lại."
                : "Bạn có chắc muốn xóa " + (isParent ? "nhóm cha " : "nhóm máy ") + "\"" + itemName + "\"?";
            DialogResult result = MessageBox.Show(
                prompt,
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                if (!isParent)
                {
                    string fileName = itemName;
                    foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
                        fileName = fileName.Replace(invalidCharacter, '_');

                    string filePath = Path.Combine(ConfigurationDirectory, fileName + ".json");
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }

                selected.Remove();
                lbMayCat.Text = "";
                SaveMachineCutList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xóa nhóm máy: " + ex.Message);
            }
        }

        private void SaveMachineCutList()
        {
            try
            {
                var configuration = new MachineGroupConfiguration();
                foreach (TreeNode parent in machineGroupTree.Nodes)
                {
                    var group = parent.Tag as MachineGroupData;
                    if (group == null)
                        continue;

                    group.Name = parent.Text;
                    group.Machines = parent.Nodes.Cast<TreeNode>()
                        .Select(node => node.Text)
                        .ToList();
                    configuration.Groups.Add(group);
                }

                SaveMachineGroupConfiguration(ConfigurationDirectory, configuration);
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

            if (machineGroupTree.Nodes.Cast<TreeNode>()
                .SelectMany(parentNode => parentNode.Nodes.Cast<TreeNode>())
                .Any(existingNode => string.Equals(existingNode.Text, machineCode, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Mã máy đã tồn tại.");
                return;
            }

            var parent = machineGroupTree.SelectedNode;
            if (parent != null && parent.Level != 0)
                parent = parent.Parent;
            if (parent == null)
            {
                var group = new MachineGroupData { Name = "Nhóm máy" };
                parent = new TreeNode(group.Name) { Tag = group };
                machineGroupTree.Nodes.Add(parent);
            }

            var machineNode = new TreeNode(machineCode) { Tag = machineCode };
            parent.Nodes.Add(machineNode);
            parent.Expand();
            machineGroupTree.SelectedNode = machineNode;
            SaveMachineCutList();
            txtMaSP.Clear();
            txtMaSP.Focus();
        }
    }
}
