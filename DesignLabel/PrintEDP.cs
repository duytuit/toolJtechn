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
                    rtbLotNo.Lines = _requests
                        .Select(request => request.Sequence + "    " + request.EdpSymbol)
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
                        EdpSymbol = string.IsNullOrWhiteSpace(symbol) ? "I3" : symbol
                    });
                }

                if (result.Count == 0)
                    throw new InvalidDataException("Không có 発注SEQ hợp lệ trong file Excel.");

                return result
                    .GroupBy(request => request.Sequence, StringComparer.OrdinalIgnoreCase)
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
                SELECT H10F.製番, H10F.品目C, H10F.発注SEQ, H10F.発注数
                FROM DFW_H10F H10F
                WHERE H10F.発注SEQ IN (" + string.Join(",", sequenceNames) + ")";
            var orderRows = _oracle.ExecuteDataTable(orderSql, sequenceParameters.ToArray());
            if (orderRows.Rows.Count == 0)
                throw new InvalidOperationException("Không tìm thấy dữ liệu DFW_H10F theo 発注SEQ đã nhập.");

            var requestBySequence = _requests.ToDictionary(request => request.Sequence, StringComparer.OrdinalIgnoreCase);
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
            var productNames = new List<string>();
            for (var index = 0; index < productCodes.Count; index++)
            {
                var name = ":hin" + index;
                productNames.Add(name);
                productParameters.Add(new OracleParameter(name, OracleDbType.Varchar2)
                {
                    Value = productCodes[index]
                });
            }

            var structureSql = @"
                SELECT HINCD, SENBAN, MARKNO, SENSYU, SENTYO, SURYO, TABA,
                       TWIST1, TWIST2, TWIST3, TASCDA, TASMEIA, TASCDB, TASMEIB, BIKO
                FROM TDCJSIJI
                WHERE HINCD IN (" + string.Join(",", productNames) + ")";
            var structureRows = _oracle.ExecuteDataTable(structureSql, productParameters.ToArray());
            var structuresByProduct = structureRows.Rows.Cast<DataRow>()
                .GroupBy(row => GetValue(row, "HINCD"), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

            _labels.Clear();
            foreach (DataRow orderRow in orderRows.Rows)
            {
                var sequence = GetValue(orderRow, "発注SEQ");
                string productCode;
                EdpRequest request;
                if (!productCodeBySequence.TryGetValue(sequence, out productCode) ||
                    !requestBySequence.TryGetValue(sequence, out request))
                    continue;

                List<DataRow> componentRows;
                if (!structuresByProduct.TryGetValue(productCode, out componentRows))
                    continue;

                foreach (var component in componentRows)
                    _labels.Add(CreateLabel(orderRow, component, productCode, request.EdpSymbol));
            }

            if (_labels.Count == 0)
                throw new InvalidOperationException("Đã tìm thấy DFW_H10F nhưng không có cấu thành tương ứng trong TDCJSIJI.");

            groupBox1.Text = "Thống kê kết quả: " + _labels.Count + " EDP";
            UpdatePreviewScrollSize();
            plPriviewPageLotNo.Invalidate();
        }

        private static EdpLabel CreateLabel(DataRow orderRow, DataRow component, string productCode, string edpSymbol)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["LotNumber"] = GetValue(orderRow, "製番"),
                ["OrderSequence"] = GetValue(orderRow, "発注SEQ"),
                ["OrderQuantity"] = GetValue(orderRow, "発注数"),
                ["ProductCode"] = productCode,
                ["EdpSymbol"] = edpSymbol,
                ["SENBAN"] = GetValue(component, "SENBAN"),
                ["MARKNO"] = GetValue(component, "MARKNO"),
                ["SENSYU"] = GetValue(component, "SENSYU"),
                ["SENTYO"] = GetValue(component, "SENTYO"),
                ["SURYO"] = GetValue(component, "SURYO"),
                ["TABA"] = GetValue(component, "TABA"),
                ["TWIST1"] = GetValue(component, "TWIST1"),
                ["TWIST2"] = GetValue(component, "TWIST2"),
                ["TWIST3"] = GetValue(component, "TWIST3"),
                ["TASCDA"] = GetValue(component, "TASCDA"),
                ["TASMEIA"] = GetValue(component, "TASMEIA"),
                ["TASCDB"] = GetValue(component, "TASCDB"),
                ["TASMEIB"] = GetValue(component, "TASMEIB"),
                ["BIKO"] = GetValue(component, "BIKO")
            };
            return new EdpLabel(values);
        }

        private static string GetValue(DataRow row, string columnName)
        {
            return row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value
                ? Convert.ToString(row[columnName]).Trim()
                : string.Empty;
        }

        private void btnPrintLotNo_Click(object sender, EventArgs e)
        {
            try
            {
                if (_labels.Count == 0)
                    LoadEdpData();

                _printIndex = 0;
                using (var document = new PrintDocument())
                {
                    document.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169);
                    document.DefaultPageSettings.Landscape = false;
                    document.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
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
            _printIndex += Math.Min(5, _labels.Count - _printIndex);
            e.HasMorePages = _printIndex < _labels.Count;
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
            var pageCount = Math.Max(1, (_labels.Count + 4) / 5);
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

                DrawA4Page(e.Graphics, pageIndex * 5);
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
            var pageCount = Math.Max(1, (_labels.Count + 4) / 5);
            var contentHeight = 2 * pageMarginPixels +
                pageCount * pageHeightMm * scale +
                (pageCount - 1) * pageGapPixels;
            plPriviewPageLotNo.AutoScrollMinSize = new Size(
                0,
                (int)Math.Ceiling(contentHeight));
        }

        private void DrawA4Page(Graphics graphics, int firstLabel)
        {
            using (var pageBrush = new SolidBrush(Color.White))
            using (var pagePen = new Pen(Color.Gray, 0.25f))
            {
                graphics.FillRectangle(pageBrush, 0, 0, 210, 297);
                graphics.DrawRectangle(pagePen, 0, 0, 210, 297);
            }

            const int labelsPerPage = 5;
            const float pageWidthMm = 210f;
            const float pageHeightMm = 297f;
            var labelWidthMm = PixelsToMillimeters(GetLabelWidthPixels());
            var labelHeightMm = PixelsToMillimeters(GetLabelHeightPixels());
            if (labelWidthMm > pageWidthMm || labelHeightMm * labelsPerPage > pageHeightMm)
                throw new InvalidDataException("Kích thước EDP không vừa khổ A4 khi in 5 nhãn.");

            var gapMm = (pageHeightMm - labelsPerPage * labelHeightMm) / (labelsPerPage + 1);
            var count = Math.Min(labelsPerPage, _labels.Count - firstLabel);
            for (var index = 0; index < count; index++)
            {
                var x = (pageWidthMm - labelWidthMm) / 2f;
                var y = gapMm + index * (labelHeightMm + gapMm);
                DrawLabel(graphics, x, y, _labels[firstLabel + index]);
            }
        }

        private void DrawLabel(Graphics graphics, float x, float y, EdpLabel label)
        {
            var state = graphics.Save();
            graphics.TranslateTransform(x, y);
            var labelWidthMm = PixelsToMillimeters(GetLabelWidthPixels());
            var labelHeightMm = PixelsToMillimeters(GetLabelHeightPixels());
            using (var borderPen = new Pen(Color.Black, 0.22f))
            {
                graphics.DrawRectangle(borderPen, 0, 0, labelWidthMm, labelHeightMm);
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
            }
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
                            FormatFlags = StringFormatFlags.NoWrap
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
        }

        private sealed class EdpRequest
        {
            public string Sequence { get; set; }
            public string EdpSymbol { get; set; }
        }

        private sealed class EdpLabel
        {
            private readonly Dictionary<string, string> _values;

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
