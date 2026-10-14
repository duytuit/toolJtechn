using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace WindowsFormsApp5
{
    public partial class PrintLotNo : Form
    {
        private Panel panelRulerTop;
        private Panel panelRulerLeft;
        private Panel designPanel;
        private int gridSize = 20;

        private List<LineObject> lines = new List<LineObject>();
        private int currentLineThickness = 2;

        private bool isResizing = false;
        private bool isDragging = false;
        private Control activeCtrl = null;
        private readonly List<Control> selectedControls = new List<Control>();
        private readonly List<LineObject> selectedLines = new List<LineObject>();
        private bool isSelecting = false;
        private bool isMovingSelection = false;
        private bool isResizingLine = false;
        private bool resizingLineStart = false;
        private LineObject activeLine;
        private Point selectionStart;
        private Point selectionCurrent;
        private Point selectionDragLast;
        private Point dragStartCanvas;
        private Size resizeStartSize;
        private Point resizeStart;

        private ComboBox cmbPaperSize;
        private int customPaperWidth = 210;  // mm, mặc định A4
        private int customPaperHeight = 297; // mm, mặc định A4
        private float dpi = 96f; // mặc định 96dpi
        private const int canvasScrollPadding = 160;
        private const int MaxVisiblePreviewPages = 3;
        private const int previewPadding = 12;
        private List<DataRow> _pagedRows = new List<DataRow>();
        private int _currentPageIndex = 0;
        private List<List<DataRow>> _allLotPages = new List<List<DataRow>>();
        private List<string> _requestedSeqNos = new List<string>();

        private bool isDrawingLine = false;
        private bool isLineDragging = false;
        private Point lineStart;
        private Point lineCurrentEnd;
        private readonly OracleHelper _oracle;
        private DataTable _currentLotData;
        private LayoutData _currentLayout;
        private readonly List<RenderedPage> _renderedPages = new List<RenderedPage>();
        private float _previewScale = 1f;

        public PrintLotNo()
        {
            InitializeComponent();
            AutoScroll = false;
            AutoScrollMinSize = Size.Empty;

            designPanel = plPriviewPageLotNo;
            if (designPanel != null)
            {
                ConfigureScrollableCanvas();
                SyncPreviewToPaperSize();
            }

            if (tableLayoutPanel1 != null)
            {
                tableLayoutPanel1.AutoScroll = false;
                tableLayoutPanel1.HorizontalScroll.Enabled = false;
                tableLayoutPanel1.HorizontalScroll.Visible = false;
                tableLayoutPanel1.Dock = DockStyle.Fill;
            }

            this.Resize += PrintLotNo_Resize;

            _oracle = new OracleHelper("User Id=JTC_USER;" + "Password=EX;" + "Data Source=192.168.207.14:1521/ORCL;");
            customPaperWidth = 210;
            customPaperHeight = 297;

            if (cmbPaperSize != null)
            {
                if (cmbPaperSize.Items.Contains("A4"))
                    cmbPaperSize.SelectedItem = "A4";
                else if (cmbPaperSize.Items.Count > 0)
                    cmbPaperSize.SelectedIndex = 0;
            }
        }

        private void PrintLotNo_Resize(object sender, EventArgs e)
        {
            if (designPanel == null)
                return;

            SyncPreviewToPaperSize();
        }

        private void btnLoadLotNo_Click(object sender, EventArgs e)
        {
            rtbWarning.Clear();

            var seqNos = ParseLotNos(rtbLotNo.Text);

            if (seqNos.Count == 0)
            {
                MessageBox.Show("Vui lòng nhập mỗi 発注SEQ trên một dòng.");
                return;
            }

            var seqList = string.Join("','", seqNos.Select(x => x.Replace("'", "''")));
            _requestedSeqNos = seqNos;

            string sql = $@"
                    SELECT
                        H10F.工程C,
                        H10F.業者C,
                        H10F.製番,
                        H10F.品目C,
                        H10F.発注SEQ,
                        H10F.発注数,
                        MP0M.段取時間,
						MP0M.加工時間,
                        H10F.備考1 AS H10F_備考1,
                        H10F.備考2 AS H10F_備考2,
                        H10F.品目K,
                        H10F.納期,
                        H10F.発注日,
                        H10F.注文書発行日,
                        M40M.品目C AS M40_品目C,
                        M40M.品目K AS M40_品目K,
                        M40M.本数 AS M40_本数,
                        M46M.*
                    FROM DFW_H10F H10F
                    LEFT JOIN DFW_M40M M40M
                        ON H10F.品目K = M40M.品目K
                    AND H10F.品目C = M40M.品目C
                    LEFT JOIN TAD_M46M M46M
                        ON H10F.品目K = M46M.品目K
                    AND H10F.品目C = M46M.品目C
                    LEFT JOIN DFW_MP0M MP0M
                        ON H10F.品目C = MP0M.親品目C
					AND H10F.工程C = MP0M.子品目C
                    WHERE EXISTS (
                        SELECT 1
                        FROM DFW_H10F X
                        WHERE X.製番 = H10F.製番
                        AND X.品目C = H10F.品目C
                        AND X.発注SEQ IN ('{seqList}')
                    )
                    ORDER BY H10F.製番";
            DataTable result = _oracle.ExecuteDataTable(sql);
            _currentLotData = result;

            if (result.Rows.Count == 0)
            {
                MessageBox.Show("Không tìm thấy dữ liệu theo発注SEQ hoặc không có mapping với M40/M46.");
                return;
            }

            _pagedRows = result.Rows.Cast<DataRow>().ToList();
            _currentPageIndex = 0;

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "form_chotlo.json");
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Không tìm thấy file form_chotlo.json tại đường dẫn: {filePath}");
                return;
            }

            RenderScrollableLayout(filePath, result);
        }
        private List<string> ParseLotNos(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return new List<string>();
            }

            return input
                .Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Distinct()
                .ToList();
        }

        private void RenderScrollableLayout(string filePath, DataTable data)
        {
            if (data == null || data.Rows.Count == 0 || designPanel == null)
                return;

            if (!File.Exists(filePath))
                return;

            var layout = JsonConvert.DeserializeObject<LayoutData>(File.ReadAllText(filePath));
            if (layout == null)
                return;

            _currentLayout = layout;
            _allLotPages = GetLotPages(data, _requestedSeqNos);
            UpdateWarning(_allLotPages);

            if (_allLotPages.Count == 0)
                return;

            _currentPageIndex = Math.Max(0, Math.Min(_currentPageIndex, _allLotPages.Count - 1));

            var pageSize = GetCanvasPaperSize();
            var pageHeight = pageSize.Height;
            var currentScrollY = Math.Max(0, -designPanel.AutoScrollPosition.Y);
            var totalHeight = Math.Max((int)Math.Ceiling(pageSize.Height * _previewScale * _allLotPages.Count) + previewPadding * 2, 300);

            designPanel.Controls.Clear();
            designPanel.BackColor = Color.White;
            designPanel.AutoScroll = true;
            designPanel.HorizontalScroll.Enabled = false;
            designPanel.HorizontalScroll.Visible = false;
            designPanel.VerticalScroll.SmallChange = 12;
            designPanel.VerticalScroll.LargeChange = 80;
            designPanel.AutoScrollMinSize = new Size(
                Math.Max(1, (int)Math.Ceiling(pageSize.Width * _previewScale) + previewPadding * 2),
                totalHeight);
            designPanel.AutoScrollPosition = new Point(0, currentScrollY);

            lines = new List<LineObject>();
            RenderAllPages(_currentLayout, pageHeight, pageSize.Width);

            this.Text = $"PrintLotNo - Trang {_currentPageIndex + 1}/{_allLotPages.Count}";
            designPanel.Invalidate();
        }

        private void RenderAllPages(LayoutData layout, int pageHeight, int pageWidth)
        {
            if (layout == null || designPanel == null || _allLotPages.Count == 0)
                return;

            designPanel.SuspendLayout();
            designPanel.Controls.Clear();
            lines = new List<LineObject>();
            _renderedPages.Clear();

            for (int pageIndex = 0; pageIndex < _allLotPages.Count; pageIndex++)
            {
                var lotRows = _allLotPages[pageIndex];
                var rowOffset = pageIndex * pageHeight;
                var mergedMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var conflictingPlaceholderKeys = GetConflictingPlaceholderKeys(lotRows);

                foreach (var row in lotRows)
                {
                    var prefix = GetRowPrefix(row);
                    if (string.IsNullOrWhiteSpace(prefix))
                        continue;

                    foreach (var pair in BuildReplaceMap(row, prefix))
                    {
                        mergedMap[pair.Key] = pair.Value;
                    }
                }

                var pageLines = new List<LineObject>();
                foreach (var line in layout.Lines ?? new List<LineObject>())
                {
                    var mapped = new LineObject
                    {
                        Start = new Point(line.Start.X, line.Start.Y + rowOffset),
                        End = new Point(line.End.X, line.End.Y + rowOffset),
                        Thickness = line.Thickness
                    };
                    lines.Add(mapped);
                    pageLines.Add(mapped);
                }

                var pageItems = new List<RenderedItem>();
                foreach (var item in layout.Items ?? new List<LayoutItem>())
                {
                    var resolvedText = item.ReplaceText == true ? ResolvePlaceholderText(item.Text, mergedMap) : item.Text ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(resolvedText) && (item.Type ?? string.Empty).Trim() != "Barcode" && (item.Type ?? string.Empty).Trim() != "QRCode")
                    {
                        continue;
                    }

                    var width = item.Width > 0 ? item.Width : 80;
                    var height = item.Height > 0 ? item.Height : 20;
                    var renderItem = new RenderedItem
                    {
                        Bounds = new Rectangle(item.X, item.Y + rowOffset, width, height),
                        Text = resolvedText,
                        Highlight = IsConflictingPlaceholder(item.Text, item.ReplaceText == true, conflictingPlaceholderKeys),
                        Font = new Font(SystemFonts.DefaultFont.FontFamily, item.FontSize ?? 8.25f, GetFontStyle(item.Bold ?? false, item.Italic ?? false)),
                        ForeColor = Color.Black,
                        Vertical = item.VerticalText ?? false,
                        Type = (item.Type ?? string.Empty).Trim(),
                        BarcodeImage = null,
                        PureBarcode = item.PureBarcode ?? true
                    };

                    pageItems.Add(renderItem);
                }

                _renderedPages.Add(new RenderedPage { OffsetY = rowOffset, Lines = pageLines, Items = pageItems });
            }

            designPanel.ResumeLayout(false);
            designPanel.Invalidate();
        }

        private Control CreateLayoutControl(LayoutItem item, Dictionary<string, string> pageMap, int rowOffset)
        {
            if (item == null)
                return null;

            var width = item.Width > 0 ? item.Width : 80;
            var height = item.Height > 0 ? item.Height : 20;
            var x = item.X;
            var y = item.Y + rowOffset;
            var resolvedText = item.ReplaceText == true ? ResolvePlaceholderText(item.Text, pageMap) : item.Text ?? string.Empty;

            switch ((item.Type ?? string.Empty).Trim())
            {
                case "Barcode":
                    if (string.IsNullOrWhiteSpace(resolvedText))
                        return null;

                    return new PictureBox
                    {
                        Image = CreateBarcodeImage(resolvedText, "Barcode", width, height, item.PureBarcode ?? true),
                        Location = new Point(x, y),
                        Size = new Size(width, height),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.None,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Tag = new ElementData { Type = "Barcode", Text = resolvedText, PureBarcode = item.PureBarcode ?? true, ReplaceText = item.ReplaceText ?? true }
                    };

                case "QRCode":
                    if (string.IsNullOrWhiteSpace(resolvedText))
                        return null;

                    return new PictureBox
                    {
                        Image = CreateBarcodeImage(resolvedText, "QRCode", width, height, true),
                        Location = new Point(x, y),
                        Size = new Size(width, height),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.None,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Tag = new ElementData { Type = "QRCode", Text = resolvedText, PureBarcode = true, ReplaceText = item.ReplaceText ?? true }
                    };

                default:
                    var label = new RotatableLabel
                    {
                        Text = resolvedText,
                        AutoSize = false,
                        BackColor = Color.White,
                        ForeColor = Color.Black,
                        BorderStyle = BorderStyle.None,
                        Location = new Point(x, y),
                        Size = new Size(width, height),
                        Font = new Font(SystemFonts.DefaultFont.FontFamily, item.FontSize ?? 8.25f, GetFontStyle(item.Bold ?? false, item.Italic ?? false)),
                        Tag = new ElementData { Type = "Label", Text = resolvedText, ReplaceText = item.ReplaceText ?? false, Bold = item.Bold ?? false, Italic = item.Italic ?? false }
                    };

                    if (item.VerticalText ?? false)
                    {
                        SetLabelOrientation(label, true);
                    }

                    return label;
            }
        }

        private static Bitmap CreateBarcodeImage(string text, string type, int width, int height, bool pureBarcode)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var format = string.Equals(type, "QRCode", StringComparison.OrdinalIgnoreCase)
                ? BarcodeFormat.QR_CODE
                : BarcodeFormat.CODE_128;

            var writer = new BarcodeWriter<Bitmap>
            {
                Format = format,
                Options = new EncodingOptions
                {
                    Width = Math.Max(1, width),
                    Height = Math.Max(1, height),
                    Margin = string.Equals(type, "QRCode", StringComparison.OrdinalIgnoreCase) ? 0 : 2,
                    PureBarcode = string.Equals(type, "QRCode", StringComparison.OrdinalIgnoreCase) ? true : pureBarcode
                },
                Renderer = new BitmapBarcodeRenderer()
            };

            return writer.Write(text);
        }

        private static string ResolvePlaceholderText(string text, Dictionary<string, string> pageMap)
        {
            if (pageMap == null)
                return text ?? string.Empty;

            var key = (text ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(key))
                return string.Empty;

            var normalizedKey = key.Trim('{', '}', ' ', '\t', '\r', '\n');
            if (string.IsNullOrWhiteSpace(normalizedKey))
                return string.Empty;

            return pageMap.TryGetValue(normalizedKey, out var value)
                ? value ?? string.Empty
                : string.Empty;
        }

        private static bool IsConflictingPlaceholder(string text, bool replaceText, HashSet<string> conflictingPlaceholderKeys)
        {
            if (!replaceText || conflictingPlaceholderKeys == null || conflictingPlaceholderKeys.Count == 0)
                return false;

            var key = (text ?? string.Empty).Trim().Trim('{', '}', ' ', '\t', '\r', '\n');
            return conflictingPlaceholderKeys.Contains(key);
        }

        private static HashSet<string> GetConflictingPlaceholderKeys(IEnumerable<DataRow> lotRows)
        {
            var rows = lotRows?.ToList() ?? new List<DataRow>();
            var hasDifferentMsps = rows
                .Select(row => GetCellValue(row, "品目C", "M40_品目C"))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Skip(1)
                .Any();
            var hasDifferentQuantities = rows
                .Select(row => GetCellValue(row, "発注数"))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Skip(1)
                .Any();
            var hasDifferentLotNumbers = rows
                .Select(row => GetCellValue(row, "製番"))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Skip(1)
                .Any();
            var conflictingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!hasDifferentMsps && !hasDifferentQuantities && !hasDifferentLotNumbers)
                return conflictingKeys;

            foreach (var row in rows)
            {
                var prefix = GetRowPrefix(row);
                if (string.IsNullOrWhiteSpace(prefix))
                    continue;

                if (hasDifferentMsps)
                    conflictingKeys.Add($"{prefix}_MSP");
                if (hasDifferentQuantities)
                    conflictingKeys.Add($"{prefix}_SoLuong");
                if (hasDifferentLotNumbers)
                    conflictingKeys.Add($"{prefix}_MaKH");
            }

            return conflictingKeys;
        }

        private void ConfigureScrollableCanvas()
        {
            if (designPanel == null)
                return;

            designPanel.Dock = DockStyle.Fill;
            designPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            designPanel.AutoScroll = true;
            designPanel.BackColor = Color.White;
            designPanel.HorizontalScroll.Enabled = false;
            designPanel.HorizontalScroll.Visible = false;
            designPanel.VerticalScroll.SmallChange = 12;
            designPanel.VerticalScroll.LargeChange = 80;
            designPanel.Paint += DesignPanel_Paint;
            EnableDoubleBuffering(designPanel);
        }

        private static void EnableDoubleBuffering(Control control)
        {
            if (control == null)
                return;

            var setStyleMethod = typeof(Control).GetMethod("SetStyle", BindingFlags.Instance | BindingFlags.NonPublic);
            setStyleMethod?.Invoke(control, new object[]
            {
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint,
                true
            });

            var doubleBufferedProperty = typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            if (doubleBufferedProperty != null && doubleBufferedProperty.CanWrite)
            {
                doubleBufferedProperty.SetValue(control, true);
            }
        }

        private void SyncPreviewToPaperSize()
        {
            if (designPanel == null)
                return;

            var currentScrollY = Math.Max(0, -designPanel.AutoScrollPosition.Y);
            var pageSize = GetCanvasPaperSize();
            var availableWidth = Math.Max(
                1,
                designPanel.ClientSize.Width - previewPadding * 2 - SystemInformation.VerticalScrollBarWidth);
            _previewScale = Math.Min(1f, availableWidth / (float)Math.Max(1, pageSize.Width));
            var previewWidth = Math.Max(1, (int)Math.Ceiling(pageSize.Width * _previewScale) + previewPadding * 2);
            var previewHeight = Math.Max(1, (int)Math.Ceiling(pageSize.Height * _previewScale) + previewPadding * 2);
            var totalHeight = _allLotPages != null && _allLotPages.Count > 0
                ? Math.Max(previewHeight, (int)Math.Ceiling(pageSize.Height * _previewScale * _allLotPages.Count) + previewPadding * 2)
                : previewHeight;

            designPanel.Location = new Point(0, 0);
            designPanel.Margin = Padding.Empty;
            designPanel.AutoScroll = true;
            designPanel.HorizontalScroll.Enabled = false;
            designPanel.HorizontalScroll.Visible = false;
            designPanel.VerticalScroll.Enabled = true;
            designPanel.VerticalScroll.Visible = true;

            designPanel.AutoScrollMinSize = new Size(previewWidth, totalHeight);

            designPanel.AutoScrollPosition = new Point(0, currentScrollY);

            designPanel.Invalidate();
        }

        private static string GetCellValue(DataRow row, params string[] columnNames)
        {
            foreach (var columnName in columnNames)
            {
                if (row == null || string.IsNullOrWhiteSpace(columnName))
                    continue;

                if (row.Table == null || row.Table.Columns.Contains(columnName) == false)
                    continue;

                var value = row[columnName]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }

        private static string GetGioCong(DataRow row)
        {
            var orderQuantity = decimal.Parse(GetCellValue(row, "発注数"));
            var processingTime = decimal.Parse(GetCellValue(row, "加工時間"));
            var setupTime = decimal.Parse(GetCellValue(row, "段取時間"));

            return ((orderQuantity * processingTime) + setupTime).ToString();
        }

        private static decimal ParseSeqValue(object value)
        {
            if (value == null)
                return decimal.MaxValue;

            if (decimal.TryParse(value.ToString().Trim(), out var result))
                return result;

            return decimal.MaxValue;
        }

        private static string GetRowPrefix(DataRow row)
        {
            return GetCellValue(row, "工程C");
        }

        private static string GetSupplierName(DataRow row)
        {
            switch ( GetCellValue(row, "業者C").Trim() )
            {
                case "2004":
                    return "Vtec";
                case "2001":
                    return "Việt An";
                case "2002":
                    return "Hòa Bình";
                case "2003":
                    return "Hà Nam";
                default:
                    return "Jtec";
            }
        }

        private Dictionary<string, string> BuildReplaceMap(DataRow row, string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
                return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var note1 = GetCellValue(row, "備考1");
            var note2 = GetCellValue(row, "備考2");
            var note3 = GetCellValue(row, "備考3");
            var note4 = GetCellValue(row, "備考4");
            var note5 = GetCellValue(row, "備考5");
            var note6 = GetCellValue(row, "備考6");

            var note = string.Join(
                Environment.NewLine,
                new[] { note1,note2, note3, note4, note5, note6 }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["MaVach"] = !string.IsNullOrWhiteSpace(GetCellValue(row, "発注SEQ")) ? $"0{GetCellValue(row, "発注SEQ")} 00" : "",
                ["MSP"] = GetCellValue(row, "品目C", "M40_品目C") ?? "",
                ["GhiChuA"] =  GetCellValue(row, "H10F_備考1") ?? "",
                ["GhiChuB"] =  GetCellValue(row, "H10F_備考2") ?? "",
                ["GhiChuC"] = note,
                ["SoLuong"] = GetCellValue(row, "発注数") ?? "",
                ["MaKH"] = GetCellValue(row, "製番") ?? "",
                ["NgayBatDau"] = GetCellValue(row, "発注日") ?? "",
                ["Lot"] = !string.IsNullOrWhiteSpace(GetCellValue(row, "発注SEQ")) ? $"{GetCellValue(row, "発注SEQ")} 0" : "",
                ["KyHan"] = GetCellValue(row, "納期") ?? "",
                ["SoDayCat"] = GetCellValue(row, "M40_本数") ?? "",
                ["GioCong"] = GetGioCong(row),
                ["NgayIn"] = DateTime.Now.ToString("yyyy/MM/dd"),
                ["MaCD"] = GetCellValue(row, "品目K") == "4" ? "4" : "1",
                ["Hang"] =  GetSupplierName(row)
            };

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in values)
            {
                var key = $"{prefix}_{pair.Key}";
                map[key] = pair.Value;
            }
            return map;
        }
        private void btnPrintLotNo_Click(object sender, EventArgs e)
        {
            if (_currentLotData == null || _currentLotData.Rows.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu label để xem trước khi in.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var lotPages = GetLotPages(_currentLotData, _requestedSeqNos);
            if (lotPages.Count == 0)
            {
                MessageBox.Show("Không có trang nào để xem trước.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "form_chotlo.json");
            if (!File.Exists(filePath))
            {
                MessageBox.Show($"Không tìm thấy file form_chotlo.json tại: {filePath}", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var layout = JsonConvert.DeserializeObject<LayoutData>(File.ReadAllText(filePath));
            if (layout == null)
            {
                MessageBox.Show("Template in không hợp lệ.", "Preview", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var previewDoc = new PrintDocument();
            var paper = GetPaperDimensionsInMillimeters();
            previewDoc.DefaultPageSettings.PaperSize = new PaperSize(
                "Custom",
                MillimetersToHundredthsOfAnInch((int)Math.Round((double)paper.Width)),
                MillimetersToHundredthsOfAnInch((int)Math.Round((double)paper.Height)));
            previewDoc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            previewDoc.DefaultPageSettings.Landscape = false;

            var pageIndex = 0;
            previewDoc.BeginPrint += (s, args) => pageIndex = 0;
            previewDoc.PrintPage += (s, args) =>
            {
                if (pageIndex >= lotPages.Count)
                {
                    args.HasMorePages = false;
                    return;
                }

                DrawPageToGraphics(args.Graphics, args.PageBounds, lotPages[pageIndex], layout);
                pageIndex++;
                args.HasMorePages = pageIndex < lotPages.Count;
            };

            using (var preview = new PrintPreviewDialog
            {
                Document = previewDoc,
                Width = 900,
                Height = 700,
                StartPosition = FormStartPosition.CenterParent
            })
            {
                preview.ShowDialog(this);
            }
        }

        private List<List<DataRow>> GetLotPages(DataTable data, IEnumerable<string> requestedSeqNos)
        {
            if (data == null || data.Rows.Count == 0)
                return new List<List<DataRow>>();

            var rows = data.Rows.Cast<DataRow>().ToList();
            var rowsByLot = rows
                .GroupBy(row => GetCellValue(row, "製番"), StringComparer.OrdinalIgnoreCase)
                .Where(group => !string.IsNullOrWhiteSpace(group.Key))
                .ToDictionary(
                    group => group.Key,
                    group => group.OrderBy(row => ParseSeqValue(row["発注SEQ"]))
                        .ToList(),
                    StringComparer.OrdinalIgnoreCase);

            var pages = new List<List<DataRow>>();
            var addedLots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var requestedSeqNo in requestedSeqNos ?? Enumerable.Empty<string>())
            {
                var requestedRow = rows.FirstOrDefault(row => string.Equals(
                    GetCellValue(row, "発注SEQ"),
                    requestedSeqNo,
                    StringComparison.OrdinalIgnoreCase));

                var lotNo = GetCellValue(requestedRow, "製番");
                if (string.IsNullOrWhiteSpace(lotNo) || !addedLots.Add(lotNo))
                    continue;

                if (rowsByLot.TryGetValue(lotNo, out var lotRows))
                    pages.Add(lotRows);
            }

            return pages;
        }

        private void UpdateWarning(IEnumerable<List<DataRow>> lotPages)
        {
            var warningSeqNos = new List<string>();
            var addedSeqNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var lotRows in lotPages ?? Enumerable.Empty<List<DataRow>>())
            {
                var hasDifferentMsps = lotRows
                    .Select(row => GetCellValue(row, "品目C", "M40_品目C"))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Skip(1)
                    .Any();
                var hasDifferentQuantities = lotRows
                    .Select(row => GetCellValue(row, "発注数"))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Skip(1)
                    .Any();
                var hasDifferentLotNumbers = lotRows
                    .Select(row => GetCellValue(row, "製番"))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Skip(1)
                    .Any();

                if (!hasDifferentMsps && !hasDifferentQuantities && !hasDifferentLotNumbers)
                    continue;

                foreach (var requestedSeqNo in _requestedSeqNos)
                {
                    var belongsToPage = lotRows.Any(row => string.Equals(
                        GetCellValue(row, "発注SEQ"),
                        requestedSeqNo,
                        StringComparison.OrdinalIgnoreCase));

                    if (belongsToPage && addedSeqNos.Add(requestedSeqNo))
                        warningSeqNos.Add(requestedSeqNo);
                }
            }

            rtbWarning.Text = string.Join(Environment.NewLine, warningSeqNos);
        }

        private void DrawPageToGraphics(Graphics g, Rectangle pageBounds, List<DataRow> lotRows, LayoutData layout)
        {
            if (lotRows == null || lotRows.Count == 0 || layout == null)
                return;

            g.Clear(Color.White);

            var pageWidth = GetCanvasPaperSize().Width;
            var pageHeight = GetCanvasPaperSize().Height;
            var margin = 20f;
            var scale = Math.Min(
                (pageBounds.Width - margin * 2f) / Math.Max(1, pageWidth),
                (pageBounds.Height - margin * 2f) / Math.Max(1, pageHeight));

            if (scale <= 0)
                scale = 1f;

            var offsetX = (pageBounds.Width - pageWidth * scale) / 2f;
            var offsetY = (pageBounds.Height - pageHeight * scale) / 2f;

            g.TranslateTransform(offsetX, offsetY);
            g.ScaleTransform(scale, scale);

            var currentLotNo = (lotRows.FirstOrDefault()? ["製番"]?.ToString() ?? "").Trim();
            var mergedMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var currentRows = lotRows
                .Where(r => string.Equals((r["製番"]?.ToString() ?? "").Trim(), currentLotNo, StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => ParseSeqValue(r["発注SEQ"]))
                .ToList();
            var conflictingPlaceholderKeys = GetConflictingPlaceholderKeys(currentRows);

            foreach (var row in currentRows)
            {
                var processCode = GetCellValue(row, "工程C");
                if (string.IsNullOrWhiteSpace(processCode))
                    continue;

                foreach (var pair in BuildReplaceMap(row, processCode))
                {
                    mergedMap[pair.Key] = pair.Value;
                }
            }

            foreach (var line in layout.Lines ?? new List<LineObject>())
            {
                using (var pen = new Pen(Color.Black, Math.Max(1, line.Thickness)))
                {
                    g.DrawLine(pen, line.Start, line.End);
                }
            }

            foreach (var item in layout.Items ?? new List<LayoutItem>())
            {
                var control = CreateLayoutControl(item, mergedMap, 0);
                if (control == null)
                    continue;

                if (control is Label label)
                {
                    if (IsConflictingPlaceholder(item.Text, item.ReplaceText == true, conflictingPlaceholderKeys))
                    {
                        using (var brush = new SolidBrush(Color.Gold))
                        {
                            g.FillRectangle(brush, control.Left, control.Top, control.Width, control.Height);
                        }
                    }

                    using (var brush = new SolidBrush(label.ForeColor))
                    {
                        g.DrawString(label.Text, label.Font, brush, control.Left, control.Top);
                    }
                }
                else if (control is PictureBox pic && pic.Image != null)
                {
                    g.DrawImage(pic.Image, new RectangleF(control.Left, control.Top, control.Width, control.Height));
                }
            }

            g.ResetTransform();
        }

        private Size GetPaperDimensionsInMillimeters()
        {
            var selectedPaper = cmbPaperSize?.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(selectedPaper))
                selectedPaper = "A4";

            switch (selectedPaper)
            {
                case "A4":
                    return new Size(210, 297);
                case "A5":
                    return new Size(148, 210);
                case "Letter":
                    return new Size(216, 279);
                default:
                    return new Size(210, 297);
            }
        }

        private static int MillimetersToHundredthsOfAnInch(int mm)
        {
            return (int)Math.Round(mm * 100f / 25.4f);
        }
        private Size GetCanvasPaperSize()
        {
            var paperSize = GetUnscaledCanvasPaperSize();
            return new Size(
                Math.Max(1, paperSize.Width),
                Math.Max(1, paperSize.Height));
        }

        private Size GetUnscaledCanvasPaperSize()
        {
            var selectedPaper = cmbPaperSize?.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(selectedPaper))
                selectedPaper = "A4";

            Size paperSize;
            switch (selectedPaper)
            {
                case "A4":
                    paperSize = new Size(MillimetersToPixels(210, 96f), MillimetersToPixels(297, 96f));
                    break;
                case "A5":
                    paperSize = new Size(MillimetersToPixels(148, 96f), MillimetersToPixels(210, 96f));
                    break;
                case "Letter":
                    paperSize = new Size(MillimetersToPixels(215.9f, 96f), MillimetersToPixels(279.4f, 96f));
                    break;
                default:
                    paperSize = new Size(MillimetersToPixels(customPaperWidth, dpi), MillimetersToPixels(customPaperHeight, dpi));
                    break;
            }

            return paperSize;
        }

        private int MillimetersToPixels(float millimeters, float targetDpi)
        {
            return (int)Math.Round(millimeters * targetDpi / 25.4f);
        }

        private FontStyle GetFontStyle(bool bold, bool italic)
        {
            FontStyle style = FontStyle.Regular;
            if (bold)
                style |= FontStyle.Bold;
            if (italic)
                style |= FontStyle.Italic;
            return style;
        }

        private void SetLabelOrientation(RotatableLabel label, bool vertical)
        {
            label.VerticalText = vertical;
            if (vertical)
            {
                var text = (label.Text ?? string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
                var characterSize = TextRenderer.MeasureText("W", label.Font);
                label.AutoSize = false;
                label.Width = characterSize.Width + label.Padding.Horizontal + 4;
                label.Height = Math.Max(characterSize.Height * Math.Max(text.Length, 1) + label.Padding.Vertical + 4, 20);
            }
            else
            {
                label.AutoSize = true;
            }
            label.Invalidate();
        }

        private void DesignPanel_Paint(object sender, PaintEventArgs e)
        {
            if (designPanel == null)
                return;

            var scrollOffset = designPanel.AutoScrollPosition;
            var pageHeight = GetCanvasPaperSize().Height;
            var logicalTop = Math.Max(0f, (-scrollOffset.Y - previewPadding) / Math.Max(0.01f, _previewScale));
            var logicalBottom = logicalTop + designPanel.ClientSize.Height / Math.Max(0.01f, _previewScale);
            e.Graphics.Clear(Color.White);
            e.Graphics.TranslateTransform(scrollOffset.X + previewPadding, scrollOffset.Y + previewPadding);
            e.Graphics.ScaleTransform(_previewScale, _previewScale);

            try
            {
                foreach (var page in _renderedPages)
                {
                    if (page.OffsetY + pageHeight < logicalTop || page.OffsetY > logicalBottom)
                        continue;

                    foreach (var line in page.Lines)
                    {
                        using (var pen = new Pen(Color.Black, line.Thickness))
                        {
                            e.Graphics.DrawLine(pen, line.Start, line.End);
                        }
                    }

                    foreach (var item in page.Items)
                    {
                        if (item.Bounds.Bottom < logicalTop || item.Bounds.Top > logicalBottom)
                            continue;

                        if (item.BarcodeImage == null &&
                            (item.Type == "Barcode" || item.Type == "QRCode") &&
                            !string.IsNullOrWhiteSpace(item.Text))
                        {
                            item.BarcodeImage = CreateBarcodeImage(
                                item.Text,
                                item.Type,
                                item.Bounds.Width,
                                item.Bounds.Height,
                                item.PureBarcode);
                        }

                        if (item.BarcodeImage != null)
                        {
                            if (item.Highlight)
                            {
                                using (var brush = new SolidBrush(Color.Gold))
                                    e.Graphics.FillRectangle(brush, item.Bounds);
                            }

                            e.Graphics.DrawImage(item.BarcodeImage, item.Bounds);
                            continue;
                        }

                        if (item.Highlight)
                        {
                            using (var brush = new SolidBrush(Color.Gold))
                                e.Graphics.FillRectangle(brush, item.Bounds);
                        }

                        if (item.Vertical)
                        {
                            using (var brush = new SolidBrush(item.ForeColor))
                            {
                                TextRenderer.DrawText(e.Graphics, item.Text ?? string.Empty, item.Font, item.Bounds, item.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                            }
                            continue;
                        }

                        using (var brush = new SolidBrush(item.ForeColor))
                        {
                            e.Graphics.DrawString(item.Text ?? string.Empty, item.Font, brush, item.Bounds.Location);
                        }
                    }
                }
            }
            finally
            {
                e.Graphics.ResetTransform();
            }
        }

        public class ElementData
        {
            public string Type { get; set; }
            public string Text { get; set; }
            public bool PureBarcode { get; set; } // Thêm dòng này
            public bool ReplaceText { get; set; }
            public bool Bold { get; set; }
            public bool Italic { get; set; }
            public string ImagePath { get; set; }
        }

        public class RenderedPage
        {
            public int OffsetY { get; set; }
            public List<LineObject> Lines { get; set; } = new List<LineObject>();
            public List<RenderedItem> Items { get; set; } = new List<RenderedItem>();
        }

        public class RenderedItem
        {
            public Rectangle Bounds { get; set; }
            public string Text { get; set; }
            public bool Highlight { get; set; }
            public Font Font { get; set; }
            public Color ForeColor { get; set; }
            public bool Vertical { get; set; }
            public bool PureBarcode { get; set; }
            public string Type { get; set; }
            public Bitmap BarcodeImage { get; set; }
        }
        public class LayoutItem
        {
            public string Type { get; set; }
            public string Text { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public float? FontSize { get; set; } // Thêm dòng này
            public bool? PureBarcode { get; set; }
            public bool? VerticalText { get; set; }
            public bool? ReplaceText { get; set; }
            public bool? Bold { get; set; }
            public bool? Italic { get; set; }
            public string ImagePath { get; set; }
        }
        public class LayoutData
        {
            public List<LayoutItem> Items { get; set; }
            public List<LineObject> Lines { get; set; }
            public string PaperSize { get; set; } // "A4", "A5", "Letter", "Tùy chỉnh"
            public int? PaperWidth { get; set; }  // mm
            public int? PaperHeight { get; set; } // mm
            public float? Dpi { get; set; }
        }
        public class LineObject { public Point Start { get; set; } public Point End { get; set; } public int Thickness { get; set; } }

        private sealed class RotatableLabel : Label
        {
            public bool VerticalText { get; set; }
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
                    {
                        bitmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);
                    }
                }

                return bitmap;
            }
        }

        private void btnMau_Click(object sender, EventArgs e)
        {
            using (var form = new Form1())
            {
                form.ShowDialog(this);
            }
        }

        private void btnOpenFormEdp_Click(object sender, EventArgs e)
        {
            using (var form = new PrintEDP())
            {
                form.ShowDialog(this);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var form = new ExportEDP())
            {
                form.ShowDialog(this);
            }
        }
    }
}
