using ClosedXML.Excel;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace WindowsFormsApp5
{
    public partial class PrintEDP : Form
    {
        private readonly OracleHelper _oracle;
        private readonly List<EdpRequest> _requests = new List<EdpRequest>();
        private readonly List<EdpLabel> _labels = new List<EdpLabel>();
        private List<EdpPrintPage> _printPages = new List<EdpPrintPage>();
        private List<EdpPrintPage> _defaultPrintPages = new List<EdpPrintPage>();
        private readonly HashSet<string> _prioritizedPrintGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Bitmap> _barcodeImages = new Dictionary<string, Bitmap>();
        private readonly Queue<string> _barcodeImageOrder = new Queue<string>();
        private readonly CheckedListBox _printGroupSelector = new CheckedListBox();
        private readonly CheckBox _groupByParentSelector = new CheckBox();
        private readonly Label _loadingStatus = new Label();
        private readonly ProgressBar _loadingProgress = new ProgressBar();
        private Form1.LayoutData _template;
        private bool? _splitIntoTwoGroups;
        private int _printIndex;
        private const int BarcodeImageCacheCapacity = 128;

        public PrintEDP()
        {
            InitializeComponent();
            InitializeLoadingProgress();
            InitializePrintGroupSelector();
            FormClosed += (s, e) =>
            {
                foreach (var image in _barcodeImages.Values)
                    image.Dispose();
                _barcodeImages.Clear();
                _barcodeImageOrder.Clear();
            };
            _oracle = new OracleHelper("User Id=JTC_USER;Password=EX;Data Source=192.168.207.14:1521/ORCL;");
            plPriviewPageLotNo.AutoScroll = true;
            plPriviewPageLotNo.Resize += PreviewPanel_Resize;
            plPriviewPageLotNo.Paint += PreviewPanel_Paint;
            LoadTemplate();
        }

        private void InitializeLoadingProgress()
        {
            _loadingStatus.AutoEllipsis = true;
            _loadingStatus.Location = new Point(6, 155);
            _loadingStatus.Size = new Size(165, 18);
            _loadingStatus.Visible = false;
            panel2.Controls.Add(_loadingStatus);

            _loadingProgress.Location = new Point(6, 175);
            _loadingProgress.Size = new Size(165, 14);
            _loadingProgress.MarqueeAnimationSpeed = 30;
            _loadingProgress.Style = ProgressBarStyle.Marquee;
            _loadingProgress.Visible = false;
            panel2.Controls.Add(_loadingProgress);

            rtbLotNo.Location = new Point(rtbLotNo.Left, 196);
            rtbLotNo.Height -= 37;
        }

        private void InitializePrintGroupSelector()
        {
            groupBox2.Text = "Nhóm in (tích để in trước)";
            _printGroupSelector.CheckOnClick = true;
            _printGroupSelector.Dock = DockStyle.Fill;
            _printGroupSelector.FormattingEnabled = true;
            _printGroupSelector.HorizontalScrollbar = true;
            _printGroupSelector.IntegralHeight = false;
            _printGroupSelector.ItemCheck += PrintGroupSelector_ItemCheck;
            groupBox2.Controls.Add(_printGroupSelector);

            _groupByParentSelector.AutoSize = true;
            _groupByParentSelector.Dock = DockStyle.Top;
            _groupByParentSelector.Text = "Tách nhóm cha + SENSYU";
            _groupByParentSelector.Checked = true;
            _groupByParentSelector.CheckedChanged += GroupByParentSelector_CheckedChanged;
            groupBox2.Controls.Add(_groupByParentSelector);
        }

        private void GroupByParentSelector_CheckedChanged(object sender, EventArgs e)
        {
            if (_labels.Count == 0)
                return;

            try
            {
                RebuildPrintPages();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể cập nhật cách gom nhóm EDP: " + ex.Message);
            }
        }

        private void PrintGroupSelector_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            BeginInvoke((Action)ApplyPrintGroupPriority);
        }

        private void RefreshPrintGroupSelector()
        {
            var groups = _printPages
                .GroupBy(page => GetPrintGroupName(page), StringComparer.OrdinalIgnoreCase)
                .Select(group => new PrintGroupOption
                {
                    Name = group.Key,
                    LabelCount = group.Where(page => !page.IsGroupCover).Sum(page => page.Labels.Count)
                })
                .ToList();
            var availableGroups = new HashSet<string>(groups.Select(group => group.Name),
                StringComparer.OrdinalIgnoreCase);
            if (groups.Count > 0)
                _prioritizedPrintGroups.IntersectWith(availableGroups);

            _printGroupSelector.BeginUpdate();
            try
            {
                _printGroupSelector.Items.Clear();
                foreach (var group in groups)
                    _printGroupSelector.Items.Add(group, _prioritizedPrintGroups.Contains(group.Name));
            }
            finally
            {
                _printGroupSelector.EndUpdate();
            }

            ApplyPrintGroupPriority();
        }

        private void ApplyPrintGroupPriority()
        {
            _prioritizedPrintGroups.Clear();
            foreach (var item in _printGroupSelector.CheckedItems)
                _prioritizedPrintGroups.Add(((PrintGroupOption)item).Name);

            var groupedPages = _defaultPrintPages
                .GroupBy(GetPrintGroupName, StringComparer.OrdinalIgnoreCase)
                .Select(group => new
                {
                    Name = group.Key,
                    Pages = group.ToList()
                })
                .OrderByDescending(group => _prioritizedPrintGroups.Contains(group.Name))
                .ToList();

            _printPages = groupedPages.SelectMany(group => group.Pages).ToList();
            UpdatePreviewScrollSize();
            plPriviewPageLotNo.Invalidate();
        }

        private static string GetPrintGroupName(EdpPrintPage page)
        {
            var groupName = page.PrintGroupName ?? page.GroupName;
            return string.IsNullOrWhiteSpace(groupName) ? "Chưa phân nhóm" : groupName;
        }

        private void SetLoadingProgress(bool isLoading, string status)
        {
            _loadingStatus.Text = status;
            _loadingStatus.Visible = isLoading;
            _loadingProgress.Visible = isLoading;
            btnLoadLotNo.Enabled = !isLoading;
            btnPrintLotNo.Enabled = !isLoading;
            btnExportExcel.Enabled = !isLoading;
            btnOpenCondition.Enabled = !isLoading;
        }

        private void LoadTemplate()
        {
            var filePath = Path.Combine(Conditional.ConfigurationDirectory, "edp.json");
            if (!File.Exists(filePath))
            {
                MessageBox.Show("Không tìm thấy mẫu edp.json: " + filePath);
                return;
            }

            try
            {
                _template = JsonConvert.DeserializeObject<Form1.LayoutData>(File.ReadAllText(filePath));
                if (_template == null || _template.Items == null || _template.PaperWidth.GetValueOrDefault() <= 0 ||
                    _template.PaperHeight.GetValueOrDefault() <= 0)
                    throw new InvalidDataException("Cấu trúc mẫu EDP không hợp lệ.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đọc mẫu EDP: " + ex.Message);
            }
        }

        private async void btnLoadLotNo_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                Title = "Chọn file chứa 発注SEQ và ký hiệu EDP"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    var requests = ReadRequests(dialog.FileName);
                    _requests.Clear();
                    _requests.AddRange(requests);
                    _labels.Clear();
                    _printPages.Clear();
                    _defaultPrintPages.Clear();
                    RefreshPrintGroupSelector();
                    rtbLotNo.Lines = _requests
                        .Select(request => request.Sequence + "    " + request.EdpSymbol + "    " + request.SoDay)
                        .ToArray();
                    groupBox1.Text = "Thống kê kết quả: " + _requests.Count + " mã";
                    UpdatePreviewScrollSize();
                    plPriviewPageLotNo.Invalidate();

                    try
                    {
                        await LoadEdpDataWithProgressAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Đã nhập Excel nhưng không thể tải dữ liệu EDP: " + ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể nhập Excel: " + ex.Message);
                }
            }
        }

        private static List<EdpRequest> ReadRequests(string filePath)
        {
            using (var workbook = new XLWorkbook(filePath))
            {
                var worksheet = workbook.Worksheet(1);
                var firstRow = worksheet.FirstRowUsed();
                if (firstRow == null)
                    throw new InvalidDataException("File Excel không có dữ liệu.");

                var headerColumns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var cell in firstRow.CellsUsed())
                {
                    var key = NormalizeHeader(cell.GetString());
                    if (!string.IsNullOrEmpty(key))
                        headerColumns[key] = cell.Address.ColumnNumber;
                }

                var sequenceColumn = FindColumn(headerColumns, "発注SEQ", "ORDERSEQ", "ORDERSEQUENCE", "SEQ", "LOT", "LOTNO", "ロット", "ロットNO", "製番");
                var symbolColumn = FindColumn(headerColumns, "EDP記号", "記号EDP", "EDP", "記号", "MARK");
                var soDayColumn = FindColumn(headerColumns, "soday", "SENBAN", "線番");
                var hasHeader = sequenceColumn.HasValue;
                if (!hasHeader && firstRow.Cell(1).GetValue<string>().Any(char.IsLetter))
                    throw new InvalidDataException("Không tìm thấy cột 発注SEQ. File Excel cần có tiêu đề 発注SEQ; ký hiệu EDP có thể dùng tiêu đề EDP hoặc 記号.");

                if (!hasHeader)
                    sequenceColumn = 1;

                var result = new List<EdpRequest>();
                var rows = hasHeader ? worksheet.RowsUsed().Where(row => row.RowNumber() > firstRow.RowNumber()) : worksheet.RowsUsed();
                foreach (var row in rows)
                {
                    var sequence = row.Cell(sequenceColumn.Value).GetFormattedString().Trim();
                    if (string.IsNullOrWhiteSpace(sequence))
                        continue;

                    var symbol = symbolColumn.HasValue
                        ? row.Cell(symbolColumn.Value).GetFormattedString().Trim()
                        : string.Empty;
                    result.Add(new EdpRequest
                    {
                        Sequence = sequence,
                        EdpSymbol = string.IsNullOrWhiteSpace(symbol) ? "I3" : symbol,
                        SoDay = soDayColumn.HasValue
                            ? row.Cell(soDayColumn.Value).GetFormattedString().Trim()
                            : string.Empty
                    });
                }

                if (result.Count == 0)
                    throw new InvalidDataException("Không có 発注SEQ hợp lệ trong file Excel.");

                return result
                    .GroupBy(request => new
                    {
                        Sequence = request.Sequence.ToUpperInvariant(),
                        SoDay = request.SoDay.ToUpperInvariant()
                    })
                    .Select(group => group.First())
                    .ToList();
            }
        }

        private static int? FindColumn(Dictionary<string, int> columns, params string[] aliases)
        {
            foreach (var alias in aliases)
            {
                int column;
                if (columns.TryGetValue(NormalizeHeader(alias), out column))
                    return column;
            }

            return null;
        }

        private static string NormalizeHeader(string value)
        {
            return (value ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("　", string.Empty).ToUpperInvariant();
        }

        private async Task LoadEdpDataWithProgressAsync()
        {
            SetLoadingProgress(true, "Đang tìm dữ liệu lot...");
            var progress = new Progress<string>(status => _loadingStatus.Text = status);
            try
            {
                await Task.Run(() => LoadEdpData(progress));
                RebuildPrintPages();
            }
            finally
            {
                SetLoadingProgress(false, string.Empty);
            }
        }

        private void LoadEdpData(IProgress<string> progress)
        {
            if (_template == null)
                throw new InvalidOperationException("Mẫu edp.json chưa được tải.");
            if (_requests.Count == 0)
                throw new InvalidOperationException("Vui lòng nhập file Excel trước.");

            var sequenceParameters = new List<OracleParameter>();
            var sequenceNames = new List<string>();
            for (var index = 0; index < _requests.Count; index++)
            {
                var name = ":seq" + index;
                sequenceNames.Add(name);
                sequenceParameters.Add(new OracleParameter(name, OracleDbType.Varchar2)
                {
                    Value = _requests[index].Sequence
                });
            }

            var orderSql = @"
                SELECT H10F.製番, H10F.品目C, H10F.発注SEQ, H10F.発注SEQ2, H10F.発注数, H10F.納期
                FROM DFW_H10F H10F
                WHERE H10F.発注SEQ IN (" + string.Join(",", sequenceNames) + ")";
            progress.Report("Đang tra cứu thông tin lot...");
            var orderRows = _oracle.ExecuteDataTable(orderSql, sequenceParameters.ToArray());
            if (orderRows.Rows.Count == 0)
                throw new InvalidOperationException("Không tìm thấy dữ liệu DFW_H10F theo 発注SEQ đã nhập.");

            var requestsBySequence = _requests
                .GroupBy(request => request.Sequence, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);
            var productCodeBySequence = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in orderRows.Rows)
            {
                var sequence = GetValue(row, "発注SEQ");
                var productCode = GetValue(row, "品目C");
                if (!string.IsNullOrWhiteSpace(sequence) && !string.IsNullOrWhiteSpace(productCode))
                    productCodeBySequence[sequence] = "3" + productCode;
            }

            var productCodes = productCodeBySequence.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (productCodes.Count == 0)
                throw new InvalidOperationException("Không có 品目C hợp lệ trong dữ liệu DFW_H10F.");

            var productParameters = new List<OracleParameter>();
            var structureConditions = new List<string>();
            foreach (var request in _requests)
            {
                string requestedProductCode;
                if (!productCodeBySequence.TryGetValue(request.Sequence, out requestedProductCode))
                    continue;

                var index = structureConditions.Count;
                var name = ":hin" + index;
                productParameters.Add(new OracleParameter(name, OracleDbType.Varchar2)
                {
                    Value = requestedProductCode
                });
                var condition = "C.HINCD = " + name;
                if (!string.IsNullOrWhiteSpace(request.SoDay))
                {
                    var senbanName = ":senban" + index;
                    condition += " AND TRIM(C.SENBAN) = " + senbanName;
                    productParameters.Add(new OracleParameter(senbanName, OracleDbType.Varchar2)
                    {
                        Value = request.SoDay
                    });
                }
                structureConditions.Add("(" + condition + ")");
            }

            var structureSql = @"
                WITH RequestedComponents AS (
                    SELECT C.HINCD, C.SENBAN, C.MARKNO, C.SENSYU, C.SENTYO, C.SURYO, C.TABA,
                           C.TWIST1, C.TWIST2, C.TWIST3, C.TASCDA, C.TASMEIA, C.TASCDB, C.TASMEIB, C.BIKO,
                           C.NOUKI, C.KAWAA, C.KAWAB, C.GUMCDA, C.GUMCDB, C.JOINTGA, C.JOINTGB,
                           C.INFOMEIA1, C.INFOMEIA2, C.INFOMEIB1, C.INFOMEIB2
                    FROM TDCJSIJI C
                    WHERE " + string.Join(" OR ", structureConditions) + @"
                ),
                ComponentParts AS (
                    SELECT DISTINCT TRIM(PartCode) AS PartCode
                    FROM (
                        SELECT TASCDA AS PartCode FROM RequestedComponents
                        UNION ALL
                        SELECT GUMCDA AS PartCode FROM RequestedComponents
                        UNION ALL
                        SELECT TASCDB AS PartCode FROM RequestedComponents
                        UNION ALL
                        SELECT GUMCDB AS PartCode FROM RequestedComponents
                    )
                    WHERE TRIM(PartCode) IS NOT NULL
                ),
                ShelfLocations AS (
                    SELECT TRIM(Z.品目K) || TRIM(Z.品目C) AS PartCode,
                           MAX(TRIM(Z.棚番)) AS ShelfNumber
                    FROM TAD_Z60M Z
                    INNER JOIN ComponentParts P
                        ON P.PartCode = TRIM(Z.品目K) || TRIM(Z.品目C)
                    WHERE TRIM(Z.場所C) = '1510'
                    GROUP BY TRIM(Z.品目K) || TRIM(Z.品目C)
                )
                SELECT C.HINCD, C.SENBAN, C.MARKNO, C.SENSYU, C.SENTYO, C.SURYO, C.TABA,
                       C.TWIST1, C.TWIST2, C.TWIST3, C.TASCDA, C.TASMEIA, C.TASCDB, C.TASMEIB, C.BIKO,
                       C.NOUKI, C.KAWAA, C.KAWAB, C.GUMCDA, C.GUMCDB, C.JOINTGA, C.JOINTGB,
                       C.INFOMEIA1, C.INFOMEIA2, C.INFOMEIB1, C.INFOMEIB2,
                       TA.ShelfNumber AS ShelfTerminalA, GA.ShelfNumber AS ShelfSleeveA,
                       TB.ShelfNumber AS ShelfTerminalB, GB.ShelfNumber AS ShelfSleeveB
                FROM RequestedComponents C
                LEFT JOIN ShelfLocations TA ON TA.PartCode = TRIM(C.TASCDA)
                LEFT JOIN ShelfLocations GA ON GA.PartCode = TRIM(C.GUMCDA)
                LEFT JOIN ShelfLocations TB ON TB.PartCode = TRIM(C.TASCDB)
                LEFT JOIN ShelfLocations GB ON GB.PartCode = TRIM(C.GUMCDB)";
            progress.Report("Đang tải cấu thành và vị trí kệ...");
            var structureRows = _oracle.ExecuteDataTable(structureSql, productParameters.ToArray());
            var structuresByProduct = structureRows.Rows.Cast<DataRow>()
                .GroupBy(row => GetValue(row, "HINCD"), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            _labels.Clear();
            progress.Report("Đang xử lý dữ liệu EDP...");
            foreach (DataRow orderRow in orderRows.Rows)
            {
                var sequence = GetValue(orderRow, "発注SEQ");
                string productCode;
                List<EdpRequest> requests;
                if (!productCodeBySequence.TryGetValue(sequence, out productCode) ||
                    !requestsBySequence.TryGetValue(sequence, out requests))
                    continue;

                List<DataRow> componentRows;
                if (!structuresByProduct.TryGetValue(productCode, out componentRows))
                    continue;

                foreach (var request in requests)
                {
                    foreach (var component in componentRows)
                    {
                        if (!string.IsNullOrWhiteSpace(request.SoDay) &&
                            !string.Equals(GetValue(component, "SENBAN"), request.SoDay, StringComparison.OrdinalIgnoreCase))
                            continue;

                        _labels.Add(CreateLabel(orderRow, component, productCode, request.EdpSymbol));
                    }
                }
            }

            if (_labels.Count == 0)
                throw new InvalidOperationException("Đã tìm thấy DFW_H10F nhưng không có cấu thành tương ứng trong TDCJSIJI.");
        }

        private void RebuildPrintPages(bool? splitIntoTwoGroups = null)
        {
            _printPages.Clear();
            var groups = Conditional.LoadPrintGroups(Conditional.ConfigurationDirectory);
            var settings = Conditional.LoadAutomaticSettings(Conditional.ConfigurationDirectory);
            var splitMode = splitIntoTwoGroups ?? _splitIntoTwoGroups ?? settings.SplitIntoTwoGroups;
            _splitIntoTwoGroups = splitMode;
            _printPages = BuildPrintPages(_labels, groups, settings.MinimumOrderQuantity, splitMode,
                _groupByParentSelector.Checked);
            _defaultPrintPages = new List<EdpPrintPage>(_printPages);
            groupBox1.Text = splitMode
                ? "Thống kê: Máy nhỏ " + _labels.Count(label => !MeetsAutomaticQuantity(label, settings.MinimumOrderQuantity)) +
                    " / Tự động " + _labels.Count(label => MeetsAutomaticQuantity(label, settings.MinimumOrderQuantity)) +
                    " EDP / " + _printPages.Count + " trang"
                : "Thống kê kết quả: " + _labels.Count + " EDP / " + _printPages.Count + " trang";
            RefreshPrintGroupSelector();
            UpdatePreviewScrollSize();
            plPriviewPageLotNo.Invalidate();
        }

        private static List<EdpPrintPage> BuildPrintPages(List<EdpLabel> labels, List<Conditional.ConditionData> groups,
            decimal automaticMinimumQuantity = 7m, bool splitIntoTwoGroups = false, bool groupByParent = false)
        {
            var pages = new List<EdpPrintPage>();
            foreach (var label in labels)
                label.MatchedCondition = string.Empty;

            if (splitIntoTwoGroups)
            {
                foreach (var isAutomatic in new[] { false, true })
                {
                    var categoryName = isAutomatic ? "Tự động" : "Máy nhỏ";
                    var categoryLabels = labels.Where(label =>
                        MeetsAutomaticQuantity(label, automaticMinimumQuantity) == isAutomatic).ToList();
                    AddGroupPages(pages, categoryName, categoryLabels);
                }
            }
            else
            {
                var remaining = new List<EdpLabel>(labels);
                var labelsByParent = new Dictionary<string, List<EdpLabel>>(StringComparer.OrdinalIgnoreCase);
                var labelsByMachine = new Dictionary<string, List<EdpLabel>>(StringComparer.OrdinalIgnoreCase);
                var parentOrder = groups.Select(condition => condition.ParentName)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                // Partition by quantity before matching; orders never cross between machine types.
                foreach (var group in groups.OrderByDescending(condition => condition.IsAutomatic))
                {
                    var matched = remaining.Where(label => group.IsAutomatic ==
                        MeetsAutomaticQuantity(label, automaticMinimumQuantity)).Select(label => new
                    {
                        Label = label,
                        Priority = group.GetLabelMatchPriority(label.Get("SENBAN"), label.Get("SENSYU"), label.Get("TASCDA"),
                            label.Get("TASCDB"), label.Get("GUMCDA"), label.Get("GUMCDB"))
                    }).Where(match => match.Priority >= 0).OrderBy(match => match.Priority)
                        .Select(match => match.Label).ToList();

                    foreach (var label in matched)
                    {
                        var priority = group.GetLabelMatchPriority(label.Get("SENBAN"), label.Get("SENSYU"), label.Get("TASCDA"),
                            label.Get("TASCDB"), label.Get("GUMCDA"), label.Get("GUMCDB"));
                        label.MatchedCondition = group.GetMatchCategory(priority);
                        if (group.IsAutomatic)
                            label.MatchedCondition = "発注数 >= " + automaticMinimumQuantity.ToString(
                                System.Globalization.CultureInfo.InvariantCulture) + " + " + label.MatchedCondition;
                    }
                    List<EdpLabel> parentLabels;
                    if (!labelsByParent.TryGetValue(group.ParentName, out parentLabels))
                    {
                        parentLabels = new List<EdpLabel>();
                        labelsByParent.Add(group.ParentName, parentLabels);
                    }
                    parentLabels.AddRange(matched);
                    List<EdpLabel> machineLabels;
                    if (!labelsByMachine.TryGetValue(group.Name, out machineLabels))
                    {
                        machineLabels = new List<EdpLabel>();
                        labelsByMachine.Add(group.Name, machineLabels);
                    }
                    machineLabels.AddRange(matched);
                    var assigned = new HashSet<EdpLabel>(matched);
                    remaining.RemoveAll(label => assigned.Contains(label));
                }

                if (groupByParent)
                {
                    foreach (var parentName in parentOrder)
                    {
                        List<EdpLabel> parentLabels;
                        if (!labelsByParent.TryGetValue(parentName, out parentLabels))
                            continue;

                        AddGroupPagesByWireType(pages, parentName, parentLabels, parentName);
                    }
                }
                else
                {
                    foreach (var group in groups)
                    {
                        List<EdpLabel> machineLabels;
                        if (!labelsByMachine.TryGetValue(group.Name, out machineLabels))
                            continue;

                        if (group.GroupBySensyu)
                            AddGroupPagesByWireType(pages, group.Name, machineLabels, group.Name);
                        else
                            AddGroupPages(pages, group.Name, machineLabels, group.Name);
                    }
                }

                AddGroupPagesByWireType(pages, "Chưa phân nhóm", remaining);
            }
            return pages;
        }

        private static bool MeetsAutomaticQuantity(EdpLabel label, decimal minimumQuantity)
        {
            decimal quantity;
            return decimal.TryParse(label.Get("OrderQuantity"),
                System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint,
                System.Globalization.CultureInfo.InvariantCulture, out quantity) && quantity >= minimumQuantity;
        }

        private static void AddGroupPages(List<EdpPrintPage> pages, string groupName, List<EdpLabel> labels)
        {
            AddGroupPages(pages, groupName, labels, groupName);
        }

        private static void AddGroupPages(List<EdpPrintPage> pages, string groupName, List<EdpLabel> labels,
            string printGroupName)
        {
            if (labels.Count == 0)
                return;

            if (!string.IsNullOrEmpty(groupName))
                pages.Add(new EdpPrintPage
                {
                    GroupName = groupName,
                    PrintGroupName = printGroupName,
                    IsGroupCover = true
                });

            for (var first = 0; first < labels.Count; first += 5)
                pages.Add(new EdpPrintPage
                {
                    GroupName = groupName,
                    PrintGroupName = printGroupName,
                    Labels = labels.Skip(first).Take(5).ToList()
                });
        }

        private static void AddGroupPagesByWireType(List<EdpPrintPage> pages, string groupName, List<EdpLabel> labels,
            string printGroupName = null)
        {
            foreach (var wireTypeGroup in labels
                .GroupBy(label => label.Get("SENSYU"), StringComparer.OrdinalIgnoreCase)
                .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
            {
                var wireType = string.IsNullOrWhiteSpace(wireTypeGroup.Key) ? "(trống)" : wireTypeGroup.Key;
                AddGroupPages(pages, groupName + " / SENSYU: " + wireType,
                    wireTypeGroup.ToList(), printGroupName ?? groupName);
            }
        }

        private static EdpLabel CreateLabel(DataRow orderRow, DataRow component, string productCode, string edpSymbol)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["LotNumber"] = GetValue(orderRow, "製番"),
                ["OrderSequence"] = GetValue(orderRow, "発注SEQ"),
                ["OrderQuantity"] = GetQrValue(orderRow, "発注数"),
                ["ProductCode"] = productCode,
                ["QrContent"] = BuildQrContent(orderRow, component, productCode),
                ["ProductCodePrefix"] = string.IsNullOrEmpty(productCode) ? string.Empty : productCode.Substring(0, 1),
                ["ProductCodeBody"] = string.IsNullOrEmpty(productCode) ? string.Empty : productCode.Substring(1),
                ["EdpSymbol"] = edpSymbol,
                ["SENBAN"] = GetValue(component, "SENBAN"),
                ["MARKNO"] = GetValue(component, "MARKNO"),
                ["SENSYU"] = GetDisplayCode(component, "SENSYU"),
                ["SENTYO"] = GetValue(component, "SENTYO"),
                ["SURYO"] = GetValue(component, "SURYO"),
                ["TABA"] = GetValue(component, "TABA"),
                ["TWIST1"] = GetValue(component, "TWIST1"),
                ["TWIST2"] = GetValue(component, "TWIST2"),
                ["TWIST3"] = GetValue(component, "TWIST3"),
                ["TASCDA"] = GetDisplayCode(component, "TASCDA"),
                ["TASMEIA"] = GetValue(component, "TASMEIA"),
                ["TASCDB"] = GetDisplayCode(component, "TASCDB"),
                ["TASMEIB"] = GetValue(component, "TASMEIB"),
                ["KAWAA"] = GetValue(component, "KAWAA"),
                ["KAWAB"] = GetValue(component, "KAWAB"),
                ["GUMCDA"] = GetDisplayCode(component, "GUMCDA"),
                ["GUMCDB"] = GetDisplayCode(component, "GUMCDB"),
                ["JOINTGA"] = GetValue(component, "JOINTGA"),
                ["JOINTGB"] = GetValue(component, "JOINTGB"),
                ["ShelfTerminalA"] = GetValue(component, "ShelfTerminalA"),
                ["ShelfSleeveA"] = GetValue(component, "ShelfSleeveA"),
                ["ShelfTerminalB"] = GetValue(component, "ShelfTerminalB"),
                ["ShelfSleeveB"] = GetValue(component, "ShelfSleeveB"),
                ["DueDate"] = GetDueDate(orderRow),
                ["ProcessingA"] = string.Join(Environment.NewLine,
                    new[] { GetValue(component, "INFOMEIA1"), GetValue(component, "INFOMEIA2") }
                        .Where(value => !string.IsNullOrWhiteSpace(value))),
                ["ProcessingB"] = string.Join(Environment.NewLine,
                    new[] { GetValue(component, "INFOMEIB1"), GetValue(component, "INFOMEIB2") }
                        .Where(value => !string.IsNullOrWhiteSpace(value))),
                ["BIKO"] = GetValue(component, "BIKO")
            };
            return new EdpLabel(values);
        }

        private static string BuildQrContent(DataRow orderRow, DataRow component, string productCode)
        {
            // QR uses complete database codes, before removing display prefixes.
            // The last two fields are reserved and remain empty, as in the sample.
            return string.Join(",", new[]
            {
                GetQrValue(component, "SENTYO"),
                GetQrValue(component, "KAWAA"),
                GetQrValue(component, "KAWAB"),
                GetQrValue(orderRow, "発注数"),
                GetQrValue(orderRow, "発注SEQ"),
                GetQrValue(orderRow, "発注SEQ2"),
                (productCode ?? string.Empty).Trim(),
                GetQrValue(component, "SENBAN"),
                GetQrValue(component, "SENSYU"),
                GetQrValue(component, "TASCDA"),
                GetQrValue(component, "GUMCDA"),
                GetQrValue(component, "TASCDB"),
                GetQrValue(component, "GUMCDB"),
                string.Empty,
                string.Empty
            });
        }

        private static string GetQrValue(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToString(row[columnName], System.Globalization.CultureInfo.InvariantCulture).Trim()
                : string.Empty;
        }

        private static string GetDisplayCode(DataRow component, string columnName)
        {
            var value = GetValue(component, columnName);
            return value.Length > 0 ? value.Substring(1) : string.Empty;
        }

        private static string GetDueDate(DataRow orderRow)
        {
            if (orderRow.Table.Columns.Contains("納期") && orderRow["納期"] is DateTime)
                return ((DateTime)orderRow["納期"]).ToString("yy/MM/dd");

            var value = GetValue(orderRow, "納期");
            DateTime date;
            return DateTime.TryParseExact(value,
                new[] { "yyyyMMdd", "yyyy/MM/dd", "yyyy-MM-dd", "yy/MM/dd", "yyMMdd" },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out date)
                ? date.ToString("yy/MM/dd")
                : value;
        }

        private static string GetValue(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToString(row[columnName]).Trim()
                : string.Empty;
        }

        private async void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (_labels.Count == 0)
                    await LoadEdpDataWithProgressAsync();
                else
                    RebuildPrintPages();

                using (var dialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    DefaultExt = "xlsx",
                    AddExtension = true,
                    FileName = "EDP_KetQuaDieuKien.xlsx",
                    Title = "Xuất kết quả áp dụng điều kiện"
                })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK)
                        return;

                    SaveConditionResults(_printPages, dialog.FileName);
                    MessageBox.Show("Đã xuất " + _labels.Count + " EDP / " + _printPages.Count + " trang in.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xuất Excel kết quả điều kiện: " + ex.Message);
            }
        }

        private static void SaveConditionResults(List<EdpPrintPage> pages, string filePath)
        {
            var details = new DataTable();
            details.Columns.Add("STT", typeof(int));
            details.Columns.Add("Nhóm");
            details.Columns.Add("Điều kiện khớp");
            details.Columns.Add("Trang A4", typeof(int));
            details.Columns.Add("Vị trí trên trang", typeof(int));
            var fields = new[]
            {
                new[] { "発注SEQ", "OrderSequence" }, new[] { "Ký hiệu EDP", "EdpSymbol" },
                new[] { "品番", "ProductCode" }, new[] { "線番 (soday)", "SENBAN" },
                new[] { "Loại dây", "SENSYU" }, new[] { "Chiều dài", "SENTYO" },
                new[] { "Số lượng (発注数)", "OrderQuantity" }, new[] { "束", "TABA" },
                new[] { "Cos A", "TASCDA" }, new[] { "Cos B", "TASCDB" },
                new[] { "Gôm A", "GUMCDA" }, new[] { "Gôm B", "GUMCDB" },
                new[] { "G A", "JOINTGA" }, new[] { "G B", "JOINTGB" },
                new[] { "Kệ cos A", "ShelfTerminalA" }, new[] { "Kệ cos B", "ShelfTerminalB" },
                new[] { "Kệ gôm A", "ShelfSleeveA" }, new[] { "Kệ gôm B", "ShelfSleeveB" },
                new[] { "Tuốt vỏ A", "KAWAA" }, new[] { "Tuốt vỏ B", "KAWAB" },
                new[] { "Xử lý A", "ProcessingA" }, new[] { "Xử lý B", "ProcessingB" },
                new[] { "納期", "DueDate" }, new[] { "備考", "BIKO" },
                new[] { "Nội dung QR", "QrContent" }
            };
            foreach (var field in fields)
                details.Columns.Add(field[0]);

            var printPages = new DataTable();
            printPages.Columns.Add("Trang A4", typeof(int));
            printPages.Columns.Add("Nhóm");
            printPages.Columns.Add("Loại trang");
            printPages.Columns.Add("Số EDP", typeof(int));

            for (var pageIndex = 0; pageIndex < pages.Count; pageIndex++)
            {
                var page = pages[pageIndex];
                var groupName = page.GroupName ?? "Không áp dụng nhóm";
                printPages.Rows.Add(pageIndex + 1, groupName,
                    page.IsGroupCover ? "Trang tên nhóm" : "Trang EDP", page.Labels.Count);

                for (var slot = 0; slot < page.Labels.Count; slot++)
                {
                    var label = page.Labels[slot];
                    var values = new List<object>
                    {
                        details.Rows.Count + 1, groupName, label.MatchedCondition,
                        pageIndex + 1, slot + 1
                    };
                    values.AddRange(fields.Select(field => (object)label.Get(field[1])));
                    details.Rows.Add(values.ToArray());
                }
            }

            using (var workbook = new XLWorkbook())
            {
                var resultSheet = workbook.Worksheets.Add("Kết quả điều kiện");
                resultSheet.Cell(1, 1).InsertTable(details, "EdpConditionResults", true);
                resultSheet.SheetView.FreezeRows(1);
                resultSheet.SheetView.FreezeColumns(2);
                resultSheet.Columns().AdjustToContents(10, 35);
                // Keep identifiers as text, including leading zeroes and long codes.
                resultSheet.Range(2, 6, Math.Max(2, details.Rows.Count + 1), details.Columns.Count)
                    .Style.NumberFormat.Format = "@";
                for (var index = 0; index < fields.Length; index++)
                {
                    if (fields[index][1] == "ProcessingA" || fields[index][1] == "ProcessingB")
                        resultSheet.Column(index + 6).Style.Alignment.WrapText = true;
                }
                resultSheet.Rows().AdjustToContents();

                var pageSheet = workbook.Worksheets.Add("Trang in");
                pageSheet.Cell(1, 1).InsertTable(printPages, "EdpPrintPages", true);
                pageSheet.SheetView.FreezeRows(1);
                pageSheet.Columns().AdjustToContents(10, 35);
                workbook.SaveAs(filePath);
            }
        }

        private async void btnPrintLotNo_Click(object sender, EventArgs e)
        {
            try
            {
                if (_labels.Count == 0)
                    await LoadEdpDataWithProgressAsync();
                else
                    RebuildPrintPages();

                _printIndex = 0;
                using (var document = new PrintDocument())
                {
                    document.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
                    document.DefaultPageSettings.Landscape = false;
                    document.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                    document.OriginAtMargins = false;
                    document.BeginPrint += (s, args) => _printIndex = 0;
                    document.PrintPage += PrintDocument_PrintPage;
                    using (var preview = new PrintPreviewDialog
                    {
                        Document = document,
                        Width = 1000,
                        Height = 750
                    })
                    {
                        preview.ShowDialog(this);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải hoặc in EDP: " + ex.Message);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.PageUnit = GraphicsUnit.Millimeter;
            DrawA4Page(e.Graphics, _printIndex);
            _printIndex++;
            e.HasMorePages = _printIndex < _printPages.Count;
        }

        private void PreviewPanel_Paint(object sender, PaintEventArgs e)
        {
            if (_template == null)
                return;

            var scale = (plPriviewPageLotNo.ClientSize.Width - 16f) / 210f;
            if (scale <= 0)
                return;

            const float pageWidthMm = 210f;
            const float pageHeightMm = 297f;
            const float pageGapPixels = 16f;
            const float pageMarginPixels = 8f;
            var pageCount = Math.Max(1, _printPages.Count);
            var pageWidthPixels = pageWidthMm * scale;
            var pageHeightPixels = pageHeightMm * scale;
            var left = (plPriviewPageLotNo.ClientSize.Width - pageWidthPixels) / 2f;
            var scrollPosition = plPriviewPageLotNo.AutoScrollPosition;
            var pageStridePixels = pageHeightPixels + pageGapPixels;
            var viewportTop = -scrollPosition.Y;
            var viewportBottom = viewportTop + plPriviewPageLotNo.ClientSize.Height;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var firstPage = Math.Max(0, (int)Math.Floor(
                (viewportTop - pageMarginPixels - pageHeightPixels) / pageStridePixels) + 1);
            var lastPage = Math.Min(pageCount - 1, (int)Math.Ceiling(
                (viewportBottom - pageMarginPixels) / pageStridePixels) - 1);
            for (var pageIndex = firstPage; pageIndex <= lastPage; pageIndex++)
            {
                var pageTop = pageMarginPixels + scrollPosition.Y + pageIndex * pageStridePixels;
                if (pageTop >= plPriviewPageLotNo.ClientSize.Height || pageTop + pageHeightPixels <= 0)
                    continue;

                var state = e.Graphics.Save();
                using (var transform = new System.Drawing.Drawing2D.Matrix(
                    scale,
                    0,
                    0,
                    scale,
                    left + scrollPosition.X,
                    pageMarginPixels + scrollPosition.Y + pageIndex * (pageHeightPixels + pageGapPixels)))
                {
                    e.Graphics.Transform = transform;
                }

                DrawA4Page(e.Graphics, pageIndex);
                e.Graphics.Restore(state);
            }
        }

        private void PreviewPanel_Resize(object sender, EventArgs e)
        {
            UpdatePreviewScrollSize();
            plPriviewPageLotNo.Invalidate();
        }

        private void UpdatePreviewScrollSize()
        {
            var scale = (plPriviewPageLotNo.ClientSize.Width - 16f) / 210f;
            if (scale <= 0)
                return;

            const float pageHeightMm = 297f;
            const float pageGapPixels = 16f;
            const float pageMarginPixels = 8f;
            var pageCount = Math.Max(1, _printPages.Count);
            var contentHeight = 2 * pageMarginPixels +
                pageCount * pageHeightMm * scale +
                (pageCount - 1) * pageGapPixels;
            plPriviewPageLotNo.AutoScrollMinSize = new Size(
                0,
                (int)Math.Ceiling(contentHeight));
        }

        private void DrawA4Page(Graphics graphics, int pageIndex)
        {
            using (var pageBrush = new SolidBrush(Color.White))
            {
                graphics.FillRectangle(pageBrush, 0, 0, 210, 297);
            }

            if (pageIndex < 0 || pageIndex >= _printPages.Count)
                return;

            var page = _printPages[pageIndex];
            if (page.IsGroupCover)
            {
                DrawGroupCoverTitle(graphics, page.GroupName);
                return;
            }

            const int labelsPerPage = 5;
            const float pageWidthMm = 210f;
            const float pageHeightMm = 297f;
            const float gapMm = 6f;
            var labelWidthMm = PixelsToMillimeters(GetLabelWidthPixels());
            var labelHeightMm = PixelsToMillimeters(GetLabelHeightPixels());
            var totalHeightMm = labelsPerPage * labelHeightMm + (labelsPerPage - 1) * gapMm;
            if (labelWidthMm > pageWidthMm || totalHeightMm > pageHeightMm)
                throw new InvalidDataException("Kích thước EDP không vừa khổ A4 khi in 5 nhãn.");

            var count = page.Labels.Count;
            for (var index = 0; index < count; index++)
            {
                var x = (pageWidthMm - labelWidthMm) / 2f;
                var y = index * (labelHeightMm + gapMm);
                DrawLabel(graphics, x, y, page.Labels[index]);
            }
        }

        private static void DrawGroupCoverTitle(Graphics graphics, string title)
        {
            const float horizontalMarginMm = 10f;
            const float titleWidthMm = 190f;
            const float titleHeightMm = 10f;
            const float topMarginMm = 8f;
            const float bottomMarginMm = 8f;
            var titleText = title ?? string.Empty;
            var fontSize = 12f;

            using (var format = new StringFormat(StringFormat.GenericTypographic)
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            })
            using (var brush = new SolidBrush(Color.Black))
            {
                Font font;
                while (true)
                {
                    font = new Font("Arial", fontSize, FontStyle.Bold, GraphicsUnit.World);
                    if (graphics.MeasureString(titleText, font, int.MaxValue, format).Width <= titleWidthMm ||
                        fontSize <= 2f)
                        break;

                    font.Dispose();
                    fontSize -= 0.25f;
                }

                using (font)
                {
                    var topBounds = new RectangleF(
                        horizontalMarginMm, topMarginMm, titleWidthMm, titleHeightMm);
                    var bottomBounds = new RectangleF(
                        horizontalMarginMm, 297f - bottomMarginMm - titleHeightMm,
                        titleWidthMm, titleHeightMm);
                    graphics.DrawString(titleText, font, brush, topBounds, format);

                    var state = graphics.Save();
                    graphics.TranslateTransform(
                        bottomBounds.Left + bottomBounds.Width / 2f,
                        bottomBounds.Top + bottomBounds.Height / 2f);
                    graphics.RotateTransform(180f);
                    graphics.DrawString(titleText, font, brush,
                        new RectangleF(-bottomBounds.Width / 2f, -bottomBounds.Height / 2f,
                            bottomBounds.Width, bottomBounds.Height),
                        format);
                    graphics.Restore(state);
                }
            }
        }

        private void DrawLabel(Graphics graphics, float x, float y, EdpLabel label)
        {
            var state = graphics.Save();
            graphics.TranslateTransform(x, y);
            foreach (var line in _template.Lines ?? new List<Form1.LineObject>())
            {
                using (var linePen = new Pen(Color.Black, Math.Max(0.15f, PixelsToMillimeters(line.Thickness))))
                {
                    graphics.DrawLine(
                        linePen,
                        PixelsToMillimeters(line.Start.X),
                        PixelsToMillimeters(line.Start.Y),
                        PixelsToMillimeters(line.End.X),
                        PixelsToMillimeters(line.End.Y));
                }
            }

            foreach (var item in _template.Items)
                DrawLayoutItem(graphics, item, label);
            graphics.Restore(state);
        }

        private void DrawLayoutItem(Graphics graphics, Form1.LayoutItem item, EdpLabel label)
        {
            if (item == null)
                return;

            var x = PixelsToMillimeters(item.X);
            var y = PixelsToMillimeters(item.Y);
            var width = PixelsToMillimeters(item.Width);
            var height = PixelsToMillimeters(item.Height);
            var text = item.ReplaceText == true ? label.Get(GetPlaceholderKey(item.Text)) : item.Text;

            if (string.Equals(item.Type, "Label", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(text))
                    return;

                var style = FontStyle.Regular;
                if (item.Bold == true)
                    style |= FontStyle.Bold;
                if (item.Italic == true)
                    style |= FontStyle.Italic;

                var fontSizePoints = Math.Max(4f, item.FontSize ?? 8.25f);
                var fontSizeMillimeters = fontSizePoints * 25.4f / 72f;
                using (var font = new Font("MS Gothic", fontSizeMillimeters, style, GraphicsUnit.World))
                using (var brush = new SolidBrush(Color.Black))
                {
                    if (item.VerticalText == true)
                    {
                        var currentY = y;
                        foreach (var character in text.Replace("\r", string.Empty).Replace("\n", string.Empty))
                        {
                            if (currentY >= y + height)
                                break;
                            graphics.DrawString(character.ToString(), font, brush, new RectangleF(x, currentY, width, y + height - currentY));
                            currentY += font.GetHeight(graphics);
                        }
                    }
                    else
                    {
                        using (var format = new StringFormat(StringFormat.GenericTypographic)
                        {
                            Trimming = StringTrimming.EllipsisCharacter,
                            FormatFlags = item.Text == "{ProcessingA}" || item.Text == "{ProcessingB}"
                                ? (StringFormatFlags)0
                                : StringFormatFlags.NoWrap
                        })
                        {
                            graphics.DrawString(text, font, brush, new RectangleF(x, y, width, height), format);
                        }
                    }
                }
                return;
            }

            if (string.Equals(item.Type, "Barcode", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.Type, "QRCode", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(text) || width <= 0 || height <= 0)
                    return;

                var format = string.Equals(item.Type, "QRCode", StringComparison.OrdinalIgnoreCase)
                    ? BarcodeFormat.QR_CODE
                    : BarcodeFormat.CODE_128;
                var image = GetBarcodeImage(text, format, Math.Max(1, item.Width), Math.Max(1, item.Height));
                graphics.DrawImage(image, x, y, width, height);
                return;
            }

            if (string.Equals(item.Type, "Image", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(item.ImagePath) && File.Exists(item.ImagePath) && width > 0 && height > 0)
            {
                using (var image = Image.FromFile(item.ImagePath))
                    graphics.DrawImage(image, x, y, width, height);
            }
        }

        private float PixelsToMillimeters(float pixels)
        {
            var dpi = _template.Dpi.GetValueOrDefault(96f);
            return pixels * 25.4f / (dpi > 0 ? dpi : 96f);
        }

        private int GetLabelWidthPixels()
        {
            var width = (_template.Lines ?? new List<Form1.LineObject>())
                .Select(line => Math.Max(line.Start.X, line.End.X))
                .DefaultIfEmpty(0)
                .Max();
            return Math.Max(width, (_template.Items ?? new List<Form1.LayoutItem>())
                .Select(item => item.X + item.Width)
                .DefaultIfEmpty(0)
                .Max());
        }

        private int GetLabelHeightPixels()
        {
            var height = (_template.Lines ?? new List<Form1.LineObject>())
                .Select(line => Math.Max(line.Start.Y, line.End.Y) + 1)
                .DefaultIfEmpty(0)
                .Max();
            return Math.Max(height, (_template.Items ?? new List<Form1.LayoutItem>())
                .Select(item => item.Y + item.Height)
                .DefaultIfEmpty(0)
                .Max());
        }

        private static string GetPlaceholderKey(string text)
        {
            return (text ?? string.Empty).Trim().Trim('{', '}').Trim();
        }

        private static Bitmap CreateBarcodeImage(string value, BarcodeFormat format, int width, int height)
        {
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = format,
                Options = new EncodingOptions
                {
                    Width = Math.Max(1, width),
                    Height = Math.Max(1, height),
                    Margin = format == BarcodeFormat.QR_CODE ? 0 : 2,
                    PureBarcode = true
                },
                Renderer = new BitmapBarcodeRenderer()
            };
            return writer.Write(value);
        }

        private Bitmap GetBarcodeImage(string value, BarcodeFormat format, int width, int height)
        {
            var key = format + "|" + width + "|" + height + "|" + value;
            Bitmap image;
            if (_barcodeImages.TryGetValue(key, out image))
                return image;

            image = CreateBarcodeImage(value, format, width, height);
            _barcodeImages.Add(key, image);
            _barcodeImageOrder.Enqueue(key);

            if (_barcodeImageOrder.Count > BarcodeImageCacheCapacity)
            {
                var oldestKey = _barcodeImageOrder.Dequeue();
                Bitmap oldestImage;
                if (_barcodeImages.TryGetValue(oldestKey, out oldestImage))
                {
                    _barcodeImages.Remove(oldestKey);
                    oldestImage.Dispose();
                }
            }

            return image;
        }

        private void btnOpenCondition_Click(object sender, EventArgs e)
        {
            bool splitIntoTwoGroups;
            using (var form = new Conditional())
            {
                form.ShowDialog(this);
                splitIntoTwoGroups = form.SplitIntoTwoGroups;
            }
            try
            {
                RebuildPrintPages(splitIntoTwoGroups);
            }
            catch (Exception ex)
            {
                UpdatePreviewScrollSize();
                plPriviewPageLotNo.Invalidate();
                MessageBox.Show("Không thể áp dụng nhóm điều kiện: " + ex.Message);
            }
        }

        private sealed class EdpPrintPage
        {
            public string GroupName { get; set; }
            public string PrintGroupName { get; set; }
            public bool IsGroupCover { get; set; }
            public List<EdpLabel> Labels { get; set; } = new List<EdpLabel>();
        }

        private sealed class PrintGroupOption
        {
            public string Name { get; set; }
            public int LabelCount { get; set; }

            public override string ToString()
            {
                return Name + " (" + LabelCount + " EDP)";
            }
        }

        private sealed class EdpRequest
        {
            public string Sequence { get; set; }
            public string EdpSymbol { get; set; }
            public string SoDay { get; set; }
        }

        private sealed class EdpLabel
        {
            private readonly Dictionary<string, string> _values;
            public string MatchedCondition { get; set; }

            public EdpLabel(Dictionary<string, string> values)
            {
                _values = values;
            }

            public string Get(string key)
            {
                string value;
                return _values.TryGetValue(key ?? string.Empty, out value) ? value : string.Empty;
            }
        }

        private sealed class BitmapBarcodeRenderer : IBarcodeRenderer<Bitmap>
        {
            public Bitmap Render(BitMatrix matrix, BarcodeFormat format, string content)
            {
                return RenderMatrix(matrix);
            }

            public Bitmap Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options)
            {
                return RenderMatrix(matrix);
            }

            private static Bitmap RenderMatrix(BitMatrix matrix)
            {
                var bitmap = new Bitmap(matrix.Width, matrix.Height, PixelFormat.Format32bppArgb);
                var bounds = new Rectangle(0, 0, matrix.Width, matrix.Height);
                var data = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                try
                {
                    var row = new int[matrix.Width];
                    for (var y = 0; y < matrix.Height; y++)
                    {
                        for (var x = 0; x < matrix.Width; x++)
                            row[x] = matrix[x, y] ? unchecked((int)0xFF000000) : unchecked((int)0xFFFFFFFF);
                        Marshal.Copy(row, 0, IntPtr.Add(data.Scan0, y * data.Stride), row.Length);
                    }
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }

                return bitmap;
            }
        }
    }
}
