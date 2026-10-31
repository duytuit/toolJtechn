using ClosedXML.Excel;
using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
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
        private Form1.LayoutData _template;
        private int _printIndex;

        public PrintEDP()
        {
            InitializeComponent();
            _oracle = new OracleHelper("User Id=JTC_USER;Password=EX;Data Source=192.168.207.14:1521/ORCL;");
            plPriviewPageLotNo.AutoScroll = true;
            plPriviewPageLotNo.Resize += PreviewPanel_Resize;
            plPriviewPageLotNo.Paint += PreviewPanel_Paint;
            LoadTemplate();
        }

        private void LoadTemplate()
        {
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "edp.json");
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

        private void btnLoadLotNo_Click(object sender, EventArgs e)
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
                    rtbLotNo.Lines = _requests
                        .Select(request => request.Sequence + "    " + request.EdpSymbol + "    " + request.SoDay)
                        .ToArray();
                    groupBox1.Text = "Thống kê kết quả: " + _requests.Count + " mã";
                    UpdatePreviewScrollSize();
                    plPriviewPageLotNo.Invalidate();

                    try
                    {
                        LoadEdpData();
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

        private void LoadEdpData()
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
                SELECT H10F.製番, H10F.品目C, H10F.発注SEQ, H10F.発注SEQ2, H10F.発注数
                FROM DFW_H10F H10F
                WHERE H10F.発注SEQ IN (" + string.Join(",", sequenceNames) + ")";
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
                WITH ShelfLocations AS (
                    SELECT TRIM(品目K) || TRIM(品目C) AS PartCode,
                           MAX(TRIM(棚番)) AS ShelfNumber
                    FROM TAD_Z60M
                    WHERE TRIM(場所C) = '1510'
                    GROUP BY TRIM(品目K) || TRIM(品目C)
                )
                SELECT C.HINCD, C.SENBAN, C.MARKNO, C.SENSYU, C.SENTYO, C.SURYO, C.TABA,
                       C.TWIST1, C.TWIST2, C.TWIST3, C.TASCDA, C.TASMEIA, C.TASCDB, C.TASMEIB, C.BIKO,
                       C.NOUKI, C.KAWAA, C.KAWAB, C.GUMCDA, C.GUMCDB,
                       C.INFOMEIA1, C.INFOMEIA2, C.INFOMEIB1, C.INFOMEIB2,
                       TA.ShelfNumber AS ShelfTerminalA, GA.ShelfNumber AS ShelfSleeveA,
                       TB.ShelfNumber AS ShelfTerminalB, GB.ShelfNumber AS ShelfSleeveB
                FROM TDCJSIJI C
                LEFT JOIN ShelfLocations TA ON TA.PartCode = TRIM(C.TASCDA)
                LEFT JOIN ShelfLocations GA ON GA.PartCode = TRIM(C.GUMCDA)
                LEFT JOIN ShelfLocations TB ON TB.PartCode = TRIM(C.TASCDB)
                LEFT JOIN ShelfLocations GB ON GB.PartCode = TRIM(C.GUMCDB)
                WHERE " + string.Join(" OR ", structureConditions);
            var structureRows = _oracle.ExecuteDataTable(structureSql, productParameters.ToArray());
            var structuresByProduct = structureRows.Rows.Cast<DataRow>()
                .GroupBy(row => GetValue(row, "HINCD"), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            _labels.Clear();
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

            RebuildPrintPages();
        }

        private void RebuildPrintPages()
        {
            _printPages.Clear();
            var groups = Conditional.LoadPrintGroups(AppDomain.CurrentDomain.BaseDirectory);
            _printPages = BuildPrintPages(_labels, groups);
            groupBox1.Text = "Thống kê kết quả: " + _labels.Count + " EDP / " + _printPages.Count + " trang";
            UpdatePreviewScrollSize();
            plPriviewPageLotNo.Invalidate();
        }

        private static List<EdpPrintPage> BuildPrintPages(List<EdpLabel> labels, List<Conditional.ConditionData> groups)
        {
            var pages = new List<EdpPrintPage>();
            var remaining = new List<EdpLabel>(labels);
            foreach (var label in labels)
                label.MatchedCondition = string.Empty;
            foreach (var group in groups)
            {
                var matched = remaining.Select(label => new
                {
                    Label = label,
                    Priority = group.GetMatchPriority(label.Get("SENSYU"), label.Get("TASCDA"),
                        label.Get("TASCDB"), label.Get("GUMCDA"), label.Get("GUMCDB"))
                }).Where(match => match.Priority >= 0).OrderBy(match => match.Priority)
                    .Select(match => match.Label).ToList();

                foreach (var label in matched)
                {
                    var priority = group.GetMatchPriority(label.Get("SENSYU"), label.Get("TASCDA"),
                        label.Get("TASCDB"), label.Get("GUMCDA"), label.Get("GUMCDB"));
                    label.MatchedCondition = new[] { "loai_day", "tanshi", "gom" }[priority];
                }
                AddGroupPages(pages, group.Name, matched);
                var assigned = new HashSet<EdpLabel>(matched);
                remaining.RemoveAll(label => assigned.Contains(label));
            }

            AddGroupPages(pages, groups.Count > 0 ? "Chưa phân nhóm" : null, remaining);
            return pages;
        }

        private static void AddGroupPages(List<EdpPrintPage> pages, string groupName, List<EdpLabel> labels)
        {
            if (labels.Count == 0)
                return;

            if (!string.IsNullOrEmpty(groupName))
                pages.Add(new EdpPrintPage { GroupName = groupName, IsGroupCover = true });

            for (var first = 0; first < labels.Count; first += 5)
                pages.Add(new EdpPrintPage
                {
                    GroupName = groupName,
                    Labels = labels.Skip(first).Take(5).ToList()
                });
        }

        private static EdpLabel CreateLabel(DataRow orderRow, DataRow component, string productCode, string edpSymbol)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["LotNumber"] = GetValue(orderRow, "製番"),
                ["OrderSequence"] = GetValue(orderRow, "発注SEQ"),
                ["OrderQuantity"] = GetValue(orderRow, "発注数"),
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
                ["ShelfTerminalA"] = GetValue(component, "ShelfTerminalA"),
                ["ShelfSleeveA"] = GetValue(component, "ShelfSleeveA"),
                ["ShelfTerminalB"] = GetValue(component, "ShelfTerminalB"),
                ["ShelfSleeveB"] = GetValue(component, "ShelfSleeveB"),
                ["DueDate"] = GetDueDate(component),
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

        private static string GetDueDate(DataRow component)
        {
            if (component.Table.Columns.Contains("NOUKI") && component["NOUKI"] is DateTime)
                return ((DateTime)component["NOUKI"]).ToString("yy/MM/dd");

            var value = GetValue(component, "NOUKI");
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

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (_labels.Count == 0)
                    LoadEdpData();
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

        private void btnPrintLotNo_Click(object sender, EventArgs e)
        {
            try
            {
                if (_labels.Count == 0)
                    LoadEdpData();
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
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
            {
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
                using (var font = new Font("Arial", 16f, FontStyle.Bold, GraphicsUnit.World))
                using (var brush = new SolidBrush(Color.Black))
                using (var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                })
                    graphics.DrawString(page.GroupName, font, brush, new RectangleF(10, 10, 190, 277), format);
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
                using (var image = CreateBarcodeImage(text, format, Math.Max(1, item.Width), Math.Max(1, item.Height)))
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

        private void btnOpenCondition_Click(object sender, EventArgs e)
        {
            using (var form = new Conditional())
            {
                form.ShowDialog(this);
            }
            try
            {
                RebuildPrintPages();
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
            public bool IsGroupCover { get; set; }
            public List<EdpLabel> Labels { get; set; } = new List<EdpLabel>();
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
                var bitmap = new Bitmap(matrix.Width, matrix.Height);
                for (var y = 0; y < matrix.Height; y++)
                {
                    for (var x = 0; x < matrix.Width; x++)
                        bitmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);
                }

                return bitmap;
            }
        }
    }
}
