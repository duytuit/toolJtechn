using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesignLable
{
    public partial class Form1 : Form
    {
        private Panel panelRulerTop;
        private Panel panelRulerLeft;
        private Panel designPanel;
        private int gridSize = 20;

        private List<LineObject> lines = new List<LineObject>();
        private bool isDrawingLine = false;
        private Point lineStart;
        private int currentLineThickness = 2;

        private bool isResizing = false;
        private bool isDragging = false;
        private Control activeCtrl = null;
        private Point dragOffset;
        private Size resizeStartSize;
        private Point resizeStart;

        private ComboBox cmbPaperSize;
        private int customPaperWidth = 1000;  // pixel
        private int customPaperHeight = 1000; // pixel
        private float dpi = 96f; // mặc định 96dpi

        public Form1()
        {
            this.Text = "Label Designer";
            this.Size = new Size(1100, 850);
            InitUI();
        }

        private void InitUI()
        {
            // Top ruler
            panelRulerTop = new Panel { Height = 20, Dock = DockStyle.Top, BackColor = Color.White };
            panelRulerTop.Paint += DrawTopRuler;
            // Left ruler
            panelRulerLeft = new Panel { Width = 40, Dock = DockStyle.Left, BackColor = Color.White };
            panelRulerLeft.Paint += DrawLeftRuler;

            // Design panel
            designPanel = new Panel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                Dock = DockStyle.Fill
            };
            designPanel.Paint += DrawGrid;
            designPanel.MouseDown += DesignPanel_MouseDown;
            designPanel.MouseDown += DesignPanel_DrawLine_MouseDown;
            designPanel.MouseUp += DesignPanel_DrawLine_MouseUp;

            // Control panel on the right
            int ctrlX = 900;
            var btnSave = new Button { Text = "Lưu bố cục", Location = new Point(ctrlX, 30), Size = new Size(150, 30) };
            btnSave.Click += BtnSave_Click;
            var btnLoad = new Button { Text = "Tải bố cục", Location = new Point(ctrlX, 70), Size = new Size(150, 30) };
            btnLoad.Click += BtnLoad_Click;

            var chkDrawLine = new CheckBox { Text = "Vẽ đoạn thẳng", Location = new Point(ctrlX, 110) };
            chkDrawLine.CheckedChanged += (s, e) => isDrawingLine = chkDrawLine.Checked;

            var numThickness = new NumericUpDown { Location = new Point(ctrlX, 140), Minimum = 1, Maximum = 10, Value = currentLineThickness, Width = 150 };
            numThickness.ValueChanged += (s, e) => currentLineThickness = (int)numThickness.Value;

            var lblPaper = new Label { Text = "Khổ giấy:", Location = new Point(ctrlX, 180), AutoSize = true };
            cmbPaperSize = new ComboBox { Location = new Point(ctrlX, 200), Size = new Size(150, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPaperSize.Items.AddRange(new object[] { "A4", "A5", "Letter", "Tùy chỉnh" });
            cmbPaperSize.SelectedIndex = 0;
            cmbPaperSize.SelectedIndexChanged += (s, e) => designPanel.Invalidate(); // Thêm dòng này

            var btnPrint = new Button { Text = "In tem", Location = new Point(ctrlX, 250), Size = new Size(150, 30) };
            btnPrint.Click += BtnPrint_Click;

            // Add controls to form
            this.Controls.Add(btnSave);
            this.Controls.Add(btnLoad);
            this.Controls.Add(chkDrawLine);
            this.Controls.Add(numThickness);
            this.Controls.Add(lblPaper);
            this.Controls.Add(cmbPaperSize);
            this.Controls.Add(btnPrint);
            this.Controls.Add(designPanel);
            this.Controls.Add(panelRulerLeft);
            this.Controls.Add(panelRulerTop);

            // Thêm các ô nhập kích thước khổ giấy
            var lblCustomSize = new Label { Text = "Rộng (mm):", Location = new Point(ctrlX, 290), AutoSize = true };
            var txtCustomWidth = new TextBox { Location = new Point(ctrlX + 70, 288), Width = 60, Text = "100" };
            var lblCustomHeight = new Label { Text = "Cao (mm):", Location = new Point(ctrlX, 320), AutoSize = true };
            var txtCustomHeight = new TextBox { Location = new Point(ctrlX + 70, 318), Width = 60, Text = "100" };
            var lblDpi = new Label { Text = "DPI:", Location = new Point(ctrlX, 350), AutoSize = true };
            var txtDpi = new TextBox { Location = new Point(ctrlX + 70, 348), Width = 60, Text = "96" };

            // Sự kiện cập nhật giá trị khi thay đổi text
            txtCustomWidth.TextChanged += (s, e) => {
                if (int.TryParse(txtCustomWidth.Text, out int val) && val > 0)
                {
                    customPaperWidth = val;
                    designPanel.Invalidate();
                }
            };
            txtCustomHeight.TextChanged += (s, e) => {
                if (int.TryParse(txtCustomHeight.Text, out int val) && val > 0)
                {
                    customPaperHeight = val;
                    designPanel.Invalidate();
                }
            };
            txtDpi.TextChanged += (s, e) => {
                if (float.TryParse(txtDpi.Text, out float val) && val > 0)
                {
                    dpi = val;
                    designPanel.Invalidate();
                }
            };

            this.Controls.Add(lblCustomSize);
            this.Controls.Add(txtCustomWidth);
            this.Controls.Add(lblCustomHeight);
            this.Controls.Add(txtCustomHeight);
            this.Controls.Add(lblDpi);
            this.Controls.Add(txtDpi);
        }

        private void DrawGrid(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            using (var gridPen = new Pen(Color.LightGray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
            {
                for (int x = 0; x < designPanel.Width; x += gridSize)
                    g.DrawLine(gridPen, x, 0, x, designPanel.Height);
                for (int y = 0; y < designPanel.Height; y += gridSize)
                    g.DrawLine(gridPen, 0, y, designPanel.Width, y);
            }
            foreach (var line in lines)
            {
                using (var pen = new Pen(Color.Black, line.Thickness))
                    g.DrawLine(pen, line.Start, line.End);
            }

            // --- Vẽ khung tạm theo khổ giấy ---
            int paperWidth = 0, paperHeight = 0;
            string info = "";
            switch (cmbPaperSize.SelectedItem?.ToString())
            {
                case "A4": paperWidth = 827; paperHeight = 1169; info = "A4: 210x297mm, 96dpi"; break;
                case "A5": paperWidth = 583; paperHeight = 827; info = "A5: 148x210mm, 96dpi"; break;
                case "Letter": paperWidth = 850; paperHeight = 1100; info = "Letter: 216x279mm, 96dpi"; break;
                case "Tùy chỉnh":
                    // mm to pixel: pixel = mm * dpi / 25.4
                    paperWidth = (int)(customPaperWidth * dpi / 25.4f);
                    paperHeight = (int)(customPaperHeight * dpi / 25.4f);
                    info = $"Tùy chỉnh: {customPaperWidth}x{customPaperHeight}mm, {dpi}dpi";
                    break;
            }
            if (paperWidth > 0 && paperHeight > 0)
            {
                int x = 0;
                int y = 0;
                using (var borderPen = new Pen(Color.Blue, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    g.DrawRectangle(borderPen, x, y, paperWidth, paperHeight);
                }
                // Hiển thị thông số ở góc trên trái panel
                g.DrawString(info, new Font("Arial", 9, FontStyle.Bold), Brushes.Blue, x + 5, y + 5);
            }
        }

        private void DrawTopRuler(object sender, PaintEventArgs e)
        {
            for (int x = 0; x < designPanel.Width; x += gridSize)
            {
                e.Graphics.DrawLine(Pens.Black, x, 0, x, 10);
                e.Graphics.DrawString(x.ToString(), new Font("Arial", 6), Brushes.Black, x + 1, 10);
            }
        }

        private void DrawLeftRuler(object sender, PaintEventArgs e)
        {
            for (int y = 0; y < designPanel.Height; y += gridSize)
            {
                e.Graphics.DrawLine(Pens.Black, panelRulerLeft.Width - 10, y, panelRulerLeft.Width, y);
                e.Graphics.DrawString(y.ToString(), new Font("Arial", 6), Brushes.Black, 0, y + 1);
            }
        }

        private void DesignPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (TryGetLineAt(e.Location, out var line))
                {
                    var menuLine = new ContextMenuStrip();
                    menuLine.Items.Add("Xóa đoạn thẳng", null, (s, args) => { lines.Remove(line); designPanel.Invalidate(); });
                    menuLine.Show(designPanel, e.Location);
                }
                else
                {
                    var menu = new ContextMenuStrip();
                    menu.Items.Add("Thêm Text", null, (s, args) => AddDraggableText(e.Location));
                    menu.Items.Add("Thêm Hình ảnh", null, (s, args) => AddDraggableImage(e.Location));
                    menu.Items.Add("Thêm Barcode", null, (s, args) => AddBarcode(e.Location));
                    menu.Items.Add("Thêm QR Code", null, (s, args) => AddQrCode(e.Location));
                    menu.Show(designPanel, e.Location);
                }
            }
        }

        private bool TryGetLineAt(Point pt, out LineObject found)
        {
            const double tol = 3.0;
            foreach (var line in lines)
            {
                if (DistancePointToLine(line.Start, line.End, pt) <= tol)
                {
                    found = line;
                    return true;
                }
            }
            found = null;
            return false;
        }

        private double DistancePointToLine(Point a, Point b, Point p)
        {
            double dx = b.X - a.X, dy = b.Y - a.Y;
            if (dx == 0 && dy == 0)
                return Math.Sqrt(Math.Pow(p.X - a.X, 2) + Math.Pow(p.Y - a.Y, 2));
            double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0, Math.Min(1, t));
            double projX = a.X + t * dx, projY = a.Y + t * dy;
            return Math.Sqrt(Math.Pow(p.X - projX, 2) + Math.Pow(p.Y - projY, 2));
        }

        private void DesignPanel_DrawLine_MouseDown(object sender, MouseEventArgs e)
        {
            if (isDrawingLine && e.Button == MouseButtons.Left)
                lineStart = e.Location;
        }

        private void DesignPanel_DrawLine_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawingLine && e.Button == MouseButtons.Left)
            {
                lines.Add(new LineObject { Start = lineStart, End = e.Location, Thickness = currentLineThickness });
                designPanel.Invalidate();
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog { Filter = "JSON files (*.json)|*.json", DefaultExt = "json" })
                if (sfd.ShowDialog() == DialogResult.OK)
                    SaveLayout(sfd.FileName);
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" })
                if (ofd.ShowDialog() == DialogResult.OK)
                    LoadLayout(ofd.FileName);
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            var pd = new PrintDocument();
            switch (cmbPaperSize.SelectedItem.ToString())
            {
                case "A4": pd.DefaultPageSettings.PaperSize = new PaperSize("A4", 827, 1169); break;
                case "A5": pd.DefaultPageSettings.PaperSize = new PaperSize("A5", 583, 827); break;
                case "Letter": pd.DefaultPageSettings.PaperSize = new PaperSize("Letter", 850, 1100); break;
            }
            pd.PrintPage += Pd_PrintPage;
            using (var preview = new PrintPreviewDialog { Document = pd, Width = 800, Height = 600 })
                preview.ShowDialog();
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            // Draw lines
            foreach (var line in lines)
                g.DrawLine(new Pen(Color.Black, line.Thickness), line.Start, line.End);
            // Draw controls content without borders
            foreach (Control ctrl in designPanel.Controls)
            {
                if (ctrl is Label lbl)
                {
                    using (var brush = new SolidBrush(lbl.ForeColor))
                        g.DrawString(lbl.Text, lbl.Font, brush, new PointF(lbl.Left, lbl.Top));
                }
                else if (ctrl is PictureBox pic && pic.Image != null)
                {
                    g.DrawImage(pic.Image, new Rectangle(ctrl.Left, ctrl.Top, ctrl.Width, ctrl.Height));
                }
            }
        }

        private void AddDraggableText(Point location, string text = "{ProductName}")
        {
            var lbl = new Label { Text = text, Location = location, AutoSize = true, BackColor = Color.LightYellow, BorderStyle = BorderStyle.FixedSingle };
            AddCommonHandlers(lbl);
            designPanel.Controls.Add(lbl);
        }

        private void AddDraggableImage(Point location, string imagePath = "")
        {
            var pic = new PictureBox { Location = location, Size = new Size(80, 80), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };
            using (var ofd = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" })
                if (string.IsNullOrEmpty(imagePath) && ofd.ShowDialog() == DialogResult.OK)
                    pic.Image = Image.FromFile(ofd.FileName);
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                pic.Image = Image.FromFile(imagePath);
            AddCommonHandlers(pic);
            designPanel.Controls.Add(pic);
        }

        private void AddBarcode(Point location, string text = "1234567890")
        {
            var writer = new BarcodeWriter { Format = BarcodeFormat.CODE_128, Options = new ZXing.Common.EncodingOptions { Width = 200, Height = 80, Margin = 2 } };
            var pic = new PictureBox
            {
                Image = writer.Write(text),
                Location = location,
                Size = new Size(200, 80),
                BorderStyle = BorderStyle.None,
                Tag = new ElementData { Type = "Barcode", Text = text }
            };
            AddCommonHandlers(pic);
            designPanel.Controls.Add(pic);
        }

        private void AddQrCode(Point location, string text = "https://example.com")
        {
            var writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE, Options = new ZXing.Common.EncodingOptions { Width = 150, Height = 150, Margin = 1 } };
            var pic = new PictureBox
            {
                Image = writer.Write(text),
                Location = location,
                Size = new Size(150, 150),
                BorderStyle = BorderStyle.None,
                Tag = new ElementData { Type = "QRCode", Text = text }
            };
            AddCommonHandlers(pic);
            designPanel.Controls.Add(pic);
        }

        private void AddCommonHandlers(Control ctrl)
        {
            ctrl.MouseDown += (s, e) =>
            {
                activeCtrl = ctrl;
                if (e.Button == MouseButtons.Left)
                {
                    var handle = new Rectangle(ctrl.Width - 8, ctrl.Height - 8, 8, 8);
                    if (handle.Contains(e.Location))
                    {
                        isResizing = true;
                        resizeStart = ctrl.PointToScreen(e.Location);
                        resizeStartSize = ctrl.Size;
                    }
                    else
                    {
                        isDragging = true;
                        dragOffset = e.Location;
                    }
                }
            };
            ctrl.MouseMove += (s, e) =>
            {
                var handle = new Rectangle(ctrl.Width - 8, ctrl.Height - 8, 8, 8);
                ctrl.Cursor = handle.Contains(e.Location) ? Cursors.SizeNWSE : Cursors.SizeAll;
                if (isResizing && activeCtrl == ctrl)
                {
                    var curr = ctrl.PointToScreen(e.Location);
                    ctrl.Size = new Size(resizeStartSize.Width + (curr.X - resizeStart.X), resizeStartSize.Height + (curr.Y - resizeStart.Y));
                    if (ctrl is PictureBox pic && pic.Tag is ElementData data)
                    {
                        var writer = new BarcodeWriter { Options = new ZXing.Common.EncodingOptions { Width = pic.Width, Height = pic.Height, Margin = data.Type == "Barcode" ? 2 : 1 } };
                        writer.Format = data.Type == "Barcode" ? BarcodeFormat.CODE_128 : BarcodeFormat.QR_CODE;
                        pic.Image = writer.Write(data.Text);
                    }
                    designPanel.Invalidate();
                }
                else if (isDragging && activeCtrl == ctrl)
                {
                    ctrl.Left += e.X - dragOffset.X;
                    ctrl.Top += e.Y - dragOffset.Y;
                }
            };
            ctrl.MouseUp += (s, e) => { isResizing = false; isDragging = false; activeCtrl = null; };
            var menu = new ContextMenuStrip();
            menu.Items.Add("Xoá", null, (s, e) => designPanel.Controls.Remove(ctrl));
            menu.Items.Add("Chỉnh sửa nội dung", null, (s, e) => EditElement(ctrl));
            ctrl.ContextMenuStrip = menu;
        }

        private void EditElement(Control ctrl)
        {
            var data = ctrl.Tag as ElementData;
            if (data != null)
            {
                using (var dlg = new Form { Width = 300, Height = 150, Text = "Chỉnh sửa nội dung" })
                {
                    var txt = new TextBox { Text = data.Text, Dock = DockStyle.Top };
                    var btn = new Button { Text = "OK", Dock = DockStyle.Bottom };
                    btn.Click += (s, e) =>
                    {
                        data.Text = txt.Text;
                        if (ctrl is PictureBox p)
                        {
                            var writer = new BarcodeWriter
                            {
                                Format = data.Type == "Barcode" ? BarcodeFormat.CODE_128 : BarcodeFormat.QR_CODE,
                                Options = new ZXing.Common.EncodingOptions { Width = p.Width, Height = p.Height, Margin = data.Type == "Barcode" ? 2 : 1 }
                            };
                            p.Image = writer.Write(data.Text);
                        }
                        dlg.Close();
                    };
                    dlg.Controls.Add(txt);
                    dlg.Controls.Add(btn);
                    dlg.ShowDialog();
                }
            }
            else if (ctrl is Label lbl)
            {
                using (var dlg = new Form { Width = 300, Height = 200, Text = "Chỉnh sửa text" })
                {
                    var txt = new TextBox { Text = lbl.Text, Dock = DockStyle.Top };
                    var pnlFont = new Panel { Height = 40, Dock = DockStyle.Top };
                    var lblFontSize = new Label { Text = "Cỡ chữ:", AutoSize = true, Location = new Point(5, 10) };
                    var txtFontSize = new TextBox { Text = lbl.Font.Size.ToString(), Location = new Point(60, 7), Width = 50 };
                    pnlFont.Controls.Add(lblFontSize);
                    pnlFont.Controls.Add(txtFontSize);

                    var btn = new Button { Text = "OK", Dock = DockStyle.Bottom };
                    btn.Click += (s, e) => {
                        lbl.Text = txt.Text;
                        if (float.TryParse(txtFontSize.Text, out float newSize) && newSize > 0)
                        {
                            lbl.Font = new Font(lbl.Font.FontFamily, newSize, lbl.Font.Style);
                        }
                        dlg.Close();
                    };
                    dlg.Controls.Add(txt);
                    dlg.Controls.Add(pnlFont);
                    dlg.Controls.Add(btn);
                    dlg.ShowDialog();
                }
            }
        }

        private void SaveLayout(string filePath)
        {
            var items = new List<LayoutItem>();
            foreach (Control ctrl in designPanel.Controls)
            {
                if (ctrl is Label lbl)
                    items.Add(new LayoutItem
                    {
                        Type = "Label",
                        Text = lbl.Text,
                        X = lbl.Left,
                        Y = lbl.Top,
                        Width = lbl.Width,
                        Height = lbl.Height,
                        FontSize = lbl.Font.Size // Lưu cỡ chữ
                    });
                else if (ctrl is PictureBox pic)
                {
                    var data = pic.Tag as ElementData;
                    if (data != null)
                        items.Add(new LayoutItem
                        {
                            Type = data.Type,
                            Text = data.Text,
                            X = pic.Left,
                            Y = pic.Top,
                            Width = pic.Width,
                            Height = pic.Height
                        });
                }
            }
            var layout = new LayoutData
            {
                Items = items,
                Lines = lines,
                PaperSize = cmbPaperSize.SelectedItem?.ToString(),
                PaperWidth = customPaperWidth,
                PaperHeight = customPaperHeight,
                Dpi = dpi
            };
            File.WriteAllText(filePath, JsonConvert.SerializeObject(layout, Formatting.Indented));
            MessageBox.Show("Đã lưu bố cục thành công!", "Lưu", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void LoadLayout(string filePath)
        {
            if (!File.Exists(filePath)) return;
            designPanel.Controls.Clear();
            lines.Clear();
            var layout = JsonConvert.DeserializeObject<LayoutData>(File.ReadAllText(filePath));
            // Áp dụng lại thông tin khổ giấy nếu có
            if (!string.IsNullOrEmpty(layout.PaperSize))
                cmbPaperSize.SelectedItem = layout.PaperSize;
            if (layout.PaperWidth.HasValue)
                customPaperWidth = layout.PaperWidth.Value;
            if (layout.PaperHeight.HasValue)
                customPaperHeight = layout.PaperHeight.Value;
            if (layout.Dpi.HasValue)
                dpi = layout.Dpi.Value;
            foreach (var item in layout.Items)
            {
                var loc = new Point(item.X, item.Y);
                switch (item.Type)
                {
                    case "Label":
                        AddDraggableText(loc, item.Text);
                        var lbl = designPanel.Controls[designPanel.Controls.Count - 1] as Label;
                        if (lbl != null && item.FontSize.HasValue)
                            lbl.Font = new Font(lbl.Font.FontFamily, item.FontSize.Value, lbl.Font.Style);
                        break;
                    case "Barcode":
                        AddBarcode(loc, item.Text);
                        break;
                    case "QRCode":
                        AddQrCode(loc, item.Text);
                        break;
                }
                var ctrl = designPanel.Controls[designPanel.Controls.Count - 1];
                ctrl.Size = new Size(item.Width, item.Height);

                if (ctrl is PictureBox pic && pic.Tag is ElementData data)
                {
                    var writer = new BarcodeWriter
                    {
                        Format = data.Type == "Barcode" ? BarcodeFormat.CODE_128 : BarcodeFormat.QR_CODE,
                        Options = new ZXing.Common.EncodingOptions
                        {
                            Width = pic.Width,
                            Height = pic.Height,
                            Margin = data.Type == "Barcode" ? 2 : 1
                        }
                    };
                    pic.Image = writer.Write(data.Text);
                }
            }
            if (layout.Lines != null)
                lines = new List<LineObject>(layout.Lines);
            designPanel.Invalidate();
        }

        public class LineObject { public Point Start { get; set; } public Point End { get; set; } public int Thickness { get; set; } }
        public class ElementData { public string Type { get; set; } public string Text { get; set; } }
        public class LayoutItem
        {
            public string Type { get; set; }
            public string Text { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public float? FontSize { get; set; } // Thêm dòng này
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
    }
}
