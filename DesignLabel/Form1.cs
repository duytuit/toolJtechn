using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace WindowsFormsApp5
{
    public partial class Form1 : Form
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
        private int customPaperWidth = 1000;  // pixel
        private int customPaperHeight = 1000; // pixel
        private float dpi = 96f; // mặc định 96dpi
        private float zoomFactor = 1f;
        private const int canvasScrollPadding = 160;

        private bool isDrawingLine = false;
        private bool isLineDragging = false;
        private Point lineStart;
        private Point lineCurrentEnd;

        public Form1()
        {
            this.Text = "Label Designer";
            this.Size = new Size(1100, 850);
            this.WindowState = FormWindowState.Maximized;
            this.KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
            InitUI();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isDrawingLine || selectedControls.Count == 0 && selectedLines.Count == 0)
                return;

            var distance = e.Shift ? 10 : 1;
            var deltaX = 0;
            var deltaY = 0;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    deltaX = -distance;
                    break;
                case Keys.Right:
                    deltaX = distance;
                    break;
                case Keys.Up:
                    deltaY = -distance;
                    break;
                case Keys.Down:
                    deltaY = distance;
                    break;
                default:
                    return;
            }

            MoveSelectedItems(deltaX, deltaY);
            e.Handled = true;
            e.SuppressKeyPress = true;
            designPanel.Invalidate();
        }

        private void InitUI()
        {
            this.MinimumSize = new Size(900, 650);

            var workspacePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Gainsboro };
            var controlPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 220,
                BackColor = Color.FromArgb(245, 245, 245),
                AutoScroll = true,
                Padding = new Padding(12)
            };

            // Top ruler
            panelRulerTop = new Panel { Height = 20, Dock = DockStyle.Top, BackColor = Color.White };
            panelRulerTop.Paint += DrawTopRuler;
            // Left ruler
            panelRulerLeft = new Panel { Width = 40, Dock = DockStyle.Left, BackColor = Color.White };
            panelRulerLeft.Paint += DrawLeftRuler;

            // Design panel
            designPanel = new BufferedPanel
            {
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true,
                Dock = DockStyle.Fill
            };
            designPanel.Paint += DrawGrid;
            designPanel.MouseDown += DesignPanel_MouseDown;
            designPanel.MouseDown += DesignPanel_Selection_MouseDown;
            designPanel.MouseMove += DesignPanel_Selection_MouseMove;
            designPanel.MouseUp += DesignPanel_Selection_MouseUp;
            designPanel.MouseDown += DesignPanel_DrawLine_MouseDown;
            designPanel.MouseMove += DesignPanel_DrawLine_MouseMove;
            designPanel.MouseUp += DesignPanel_DrawLine_MouseUp;
            designPanel.MouseEnter += (s, e) => designPanel.Focus();
            designPanel.Scroll += (s, e) =>
            {
                panelRulerTop.Invalidate();
                panelRulerLeft.Invalidate();
                designPanel.Invalidate();
            };

            // Control panel on the right
            int ctrlX = 12;
            var btnSave = new Button { Text = "Lưu bố cục", Location = new Point(ctrlX, 20), Size = new Size(180, 32) };
            btnSave.Click += BtnSave_Click;
            var btnLoad = new Button { Text = "Tải bố cục", Location = new Point(ctrlX, 60), Size = new Size(180, 32) };
            btnLoad.Click += BtnLoad_Click;

            var chkDrawLine = new CheckBox { Text = "Vẽ đoạn thẳng", Location = new Point(ctrlX, 108), AutoSize = true };
            chkDrawLine.CheckedChanged += (s, e) => isDrawingLine = chkDrawLine.Checked;

            var numThickness = new NumericUpDown { Location = new Point(ctrlX, 138), Minimum = 1, Maximum = 10, Value = currentLineThickness, Width = 180 };
            numThickness.ValueChanged += (s, e) => currentLineThickness = (int)numThickness.Value;

            var lblPaper = new Label { Text = "Khổ giấy:", Location = new Point(ctrlX, 178), AutoSize = true };
            cmbPaperSize = new ComboBox { Location = new Point(ctrlX, 198), Size = new Size(180, 30), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbPaperSize.Items.AddRange(new object[] { "A4", "A5", "Letter", "Tùy chỉnh" });
            cmbPaperSize.SelectedIndex = 0;
            cmbPaperSize.SelectedIndexChanged += (s, e) => UpdateCanvasScrollSize();

            var btnPrint = new Button { Text = "In tem", Location = new Point(ctrlX, 250), Size = new Size(180, 32) };
            btnPrint.Click += BtnPrint_Click;

            var btnZoomOut = new Button { Text = "-", Location = new Point(ctrlX, 390), Size = new Size(45, 30) };
            btnZoomOut.Click += (s, e) => SetZoom(zoomFactor - 0.1f);
            var lblZoom = new Label { Text = "100%", Location = new Point(ctrlX + 55, 398), AutoSize = true, Name = "lblZoom" };
            var btnZoomIn = new Button { Text = "+", Location = new Point(ctrlX + 105, 390), Size = new Size(45, 30) };
            btnZoomIn.Click += (s, e) => SetZoom(zoomFactor + 0.1f);
            var btnFit = new Button { Text = "Vừa khổ", Location = new Point(ctrlX, 430), Size = new Size(180, 32) };
            btnFit.Click += (s, e) => FitCanvasToWorkspace(workspacePanel);
            var btnExportHtml = new Button { Text = "Xuất HTML", Location = new Point(ctrlX, 470), Size = new Size(180, 32) };
            btnExportHtml.Click += BtnExportHtml_Click;

            // Thêm các ô nhập kích thước khổ giấy
            var lblCustomSize = new Label { Text = "Rộng (mm):", Location = new Point(ctrlX, 290), AutoSize = true };
            var txtCustomWidth = new TextBox { Location = new Point(ctrlX + 90, 288), Width = 90, Text = "100" };
            var lblCustomHeight = new Label { Text = "Cao (mm):", Location = new Point(ctrlX, 320), AutoSize = true };
            var txtCustomHeight = new TextBox { Location = new Point(ctrlX + 90, 318), Width = 90, Text = "100" };
            var lblDpi = new Label { Text = "DPI:", Location = new Point(ctrlX, 350), AutoSize = true };
            var txtDpi = new TextBox { Location = new Point(ctrlX + 90, 348), Width = 90, Text = "96" };

            // Sự kiện cập nhật giá trị khi thay đổi text
            txtCustomWidth.TextChanged += (s, e) => {
                if (int.TryParse(txtCustomWidth.Text, out int val) && val > 0)
                {
                    customPaperWidth = val;
                    UpdateCanvasScrollSize();
                }
            };
            txtCustomHeight.TextChanged += (s, e) => {
                if (int.TryParse(txtCustomHeight.Text, out int val) && val > 0)
                {
                    customPaperHeight = val;
                    UpdateCanvasScrollSize();
                }
            };
            txtDpi.TextChanged += (s, e) => {
                if (float.TryParse(txtDpi.Text, out float val) && val > 0)
                {
                    dpi = val;
                    UpdateCanvasScrollSize();
                }
            };

            controlPanel.Controls.AddRange(new Control[]
            {
                btnSave, btnLoad, chkDrawLine, numThickness, lblPaper, cmbPaperSize,
                btnPrint, btnZoomOut, lblZoom, btnZoomIn, btnFit, btnExportHtml, lblCustomSize, txtCustomWidth,
                lblCustomHeight, txtCustomHeight, lblDpi, txtDpi
            });

            workspacePanel.Controls.Add(designPanel);
            workspacePanel.Controls.Add(panelRulerLeft);
            workspacePanel.Controls.Add(panelRulerTop);
            this.Controls.Add(workspacePanel);
            this.Controls.Add(controlPanel);
            UpdateCanvasScrollSize();
            Shown += (s, e) => FitCanvasToWorkspace(workspacePanel);
        }

        private void FitCanvasToWorkspace(Control workspacePanel)
        {
            var paperSize = GetUnscaledCanvasPaperSize();
            var availableWidth = Math.Max(1, workspacePanel.ClientSize.Width - panelRulerLeft.Width - 30);
            var availableHeight = Math.Max(1, workspacePanel.ClientSize.Height - panelRulerTop.Height - 30);
            var fitZoom = Math.Min(
                availableWidth / (float)Math.Max(1, paperSize.Width),
                availableHeight / (float)Math.Max(1, paperSize.Height));
            SetZoom(Math.Max(0.5f, Math.Min(2f, fitZoom)));
            designPanel.AutoScrollPosition = Point.Empty;
            designPanel.Invalidate();
        }

        private void SetZoom(float requestedZoom)
        {
            var newZoom = Math.Max(0.5f, Math.Min(2f, requestedZoom));
            var scale = newZoom / zoomFactor;
            var currentScroll = designPanel.AutoScrollPosition;
            var contentScroll = new Point(-currentScroll.X, -currentScroll.Y);
            designPanel.AutoScrollPosition = Point.Empty;

            foreach (Control ctrl in designPanel.Controls)
            {
                ctrl.Left = (int)Math.Round(ctrl.Left * scale);
                ctrl.Top = (int)Math.Round(ctrl.Top * scale);
                ctrl.Width = Math.Max(1, (int)Math.Round(ctrl.Width * scale));
                ctrl.Height = Math.Max(1, (int)Math.Round(ctrl.Height * scale));
                if (ctrl is Label label)
                    label.Font = new Font(label.Font.FontFamily, label.Font.Size * scale, label.Font.Style);
            }

            foreach (var line in lines)
            {
                line.Start = ScalePoint(line.Start, scale);
                line.End = ScalePoint(line.End, scale);
                line.Thickness = Math.Max(1, (int)Math.Round(line.Thickness * scale));
            }

            currentLineThickness = Math.Max(1, (int)Math.Round(currentLineThickness * scale));
            zoomFactor = newZoom;
            var zoomLabel = Controls.Find("lblZoom", true);
            if (zoomLabel.Length > 0)
                zoomLabel[0].Text = $"{zoomFactor:P0}";
            UpdateCanvasScrollSize();
            designPanel.AutoScrollPosition = new Point(
                Math.Max(0, (int)Math.Round(contentScroll.X * scale)),
                Math.Max(0, (int)Math.Round(contentScroll.Y * scale)));
            designPanel.Invalidate();
        }

        private Point ScalePoint(Point point, float scale)
        {
            return new Point(
                (int)Math.Round(point.X * scale),
                (int)Math.Round(point.Y * scale));
        }

        private void DrawGrid(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            var scrollOffset = designPanel.AutoScrollPosition;
            var canvasWidth = Math.Max(designPanel.ClientSize.Width, designPanel.AutoScrollMinSize.Width);
            var canvasHeight = Math.Max(designPanel.ClientSize.Height, designPanel.AutoScrollMinSize.Height);
            using (var gridPen = new Pen(Color.LightGray) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot })
            {
                var gridStep = Math.Max(1, (int)(gridSize * zoomFactor));
                for (int x = 0; x < canvasWidth; x += gridStep)
                    g.DrawLine(gridPen, x + scrollOffset.X, scrollOffset.Y, x + scrollOffset.X, canvasHeight + scrollOffset.Y);
                for (int y = 0; y < canvasHeight; y += gridStep)
                    g.DrawLine(gridPen, scrollOffset.X, y + scrollOffset.Y, canvasWidth + scrollOffset.X, y + scrollOffset.Y);
            }
            foreach (var line in lines)
            {
                var start = ToViewportPoint(line.Start);
                var end = ToViewportPoint(line.End);
                using (var pen = new Pen(Color.Black, line.Thickness))
                    g.DrawLine(pen, start, end);
            }

            using (var selectedPen = new Pen(Color.DodgerBlue, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
            {
                foreach (var ctrl in selectedControls)
                {
                    var location = GetControlViewportLocation(ctrl);
                    g.DrawRectangle(selectedPen, location.X - 2, location.Y - 2, ctrl.Width + 3, ctrl.Height + 3);
                }
            }

            using (var selectedLinePen = new Pen(Color.DodgerBlue, 3))
            {
                foreach (var line in selectedLines)
                {
                    var start = ToViewportPoint(line.Start);
                    var end = ToViewportPoint(line.End);
                    g.DrawLine(selectedLinePen, start, end);
                    using (var handleBrush = new SolidBrush(Color.White))
                    using (var handlePen = new Pen(Color.DodgerBlue, 1))
                    {
                        DrawLineEndpointHandle(g, start, handleBrush, handlePen);
                        DrawLineEndpointHandle(g, end, handleBrush, handlePen);
                    }
                }
            }

            if (isSelecting)
            {
                var selectionRectangle = GetSelectionRectangle();
                selectionRectangle = ToViewportRectangle(selectionRectangle);
                using (var selectionBrush = new SolidBrush(Color.FromArgb(35, Color.DodgerBlue)))
                using (var selectionPen = new Pen(Color.DodgerBlue, 1) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    g.FillRectangle(selectionBrush, selectionRectangle);
                    g.DrawRectangle(selectionPen, selectionRectangle);
                }
            }

            if (isDrawingLine && isLineDragging)
            {
                var start = ToViewportPoint(lineStart);
                var end = ToViewportPoint(lineCurrentEnd);
                using (var previewPen = new Pen(Color.FromArgb(180, Color.Red), currentLineThickness))
                {
                    previewPen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
                    previewPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
                    g.DrawLine(previewPen, start, end);
                }
            }

            // --- Vẽ khung tạm theo khổ giấy ---
            int paperWidth = 0, paperHeight = 0;
            string info = "";
            switch (cmbPaperSize.SelectedItem?.ToString())
            {
                case "A4":
                    paperWidth = MillimetersToPixels(210, 96f);
                    paperHeight = MillimetersToPixels(297, 96f);
                    info = "A4: 210x297mm, 96dpi";
                    break;
                case "A5":
                    paperWidth = MillimetersToPixels(148, 96f);
                    paperHeight = MillimetersToPixels(210, 96f);
                    info = "A5: 148x210mm, 96dpi";
                    break;
                case "Letter":
                    paperWidth = MillimetersToPixels(215.9f, 96f);
                    paperHeight = MillimetersToPixels(279.4f, 96f);
                    info = "Letter: 216x279mm, 96dpi";
                    break;
                case "Tùy chỉnh":
                    // mm to pixel: pixel = mm * dpi / 25.4
                    paperWidth = MillimetersToPixels(customPaperWidth, dpi);
                    paperHeight = MillimetersToPixels(customPaperHeight, dpi);
                    info = $"Tùy chỉnh: {customPaperWidth}x{customPaperHeight}mm, {dpi}dpi";
                    break;
            }
            if (paperWidth > 0 && paperHeight > 0)
            {
                paperWidth = (int)(paperWidth * zoomFactor);
                paperHeight = (int)(paperHeight * zoomFactor);
                int x = 0;
                int y = 0;
                using (var borderPen = new Pen(Color.Blue, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                {
                    g.DrawRectangle(borderPen, x + scrollOffset.X, y + scrollOffset.Y, paperWidth, paperHeight);
                }
                // Hiển thị thông số ở góc trên trái panel
                g.DrawString(info, new Font("Arial", 9, FontStyle.Bold), Brushes.Blue, x + scrollOffset.X + 5, y + scrollOffset.Y + 5);
            }
        }

        private Point ToViewportPoint(Point contentPoint)
        {
            var scrollOffset = designPanel.AutoScrollPosition;
            return new Point(contentPoint.X + scrollOffset.X, contentPoint.Y + scrollOffset.Y);
        }

        private Point GetControlViewportLocation(Control control)
        {
            return designPanel.PointToClient(control.PointToScreen(Point.Empty));
        }

        private Point GetControlContentLocation(Control control)
        {
            var viewportLocation = GetControlViewportLocation(control);
            var scrollOffset = designPanel.AutoScrollPosition;
            return new Point(
                viewportLocation.X - scrollOffset.X,
                viewportLocation.Y - scrollOffset.Y);
        }

        private Rectangle ToViewportRectangle(Rectangle contentRectangle)
        {
            var location = ToViewportPoint(contentRectangle.Location);
            contentRectangle.Location = location;
            return contentRectangle;
        }

        private void DrawTopRuler(object sender, PaintEventArgs e)
        {
            var scrollX = -designPanel.AutoScrollPosition.X;
            var step = Math.Max(1, (int)(gridSize * zoomFactor));
            for (int x = -scrollX % step; x < panelRulerTop.Width; x += step)
            {
                e.Graphics.DrawLine(Pens.Black, x, 0, x, 10);
                e.Graphics.DrawString((x + scrollX).ToString(), new Font("Arial", 6), Brushes.Black, x + 1, 10);
            }
        }

        private void DrawLeftRuler(object sender, PaintEventArgs e)
        {
            var scrollY = -designPanel.AutoScrollPosition.Y;
            var step = Math.Max(1, (int)(gridSize * zoomFactor));
            for (int y = -scrollY % step; y < panelRulerLeft.Height; y += step)
            {
                e.Graphics.DrawLine(Pens.Black, panelRulerLeft.Width - 10, y, panelRulerLeft.Width, y);
                e.Graphics.DrawString((y + scrollY).ToString(), new Font("Arial", 6), Brushes.Black, 0, y + 1);
            }
        }

        private void DesignPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var canvasPoint = ToCanvasPoint(e.Location);
                if (TryGetLineAt(canvasPoint, out var line))
                {
                    var menuLine = new ContextMenuStrip();
                    menuLine.Items.Add("Xóa đoạn thẳng", null, (s, args) => { lines.Remove(line); designPanel.Invalidate(); });
                    menuLine.Show(designPanel, e.Location);
                }
                else
                {
                    var menu = new ContextMenuStrip();
                    menu.Items.Add("Thêm Text", null, (s, args) => AddTextAtViewport(e.Location));
                    menu.Items.Add("Thêm Hình ảnh", null, (s, args) => AddDraggableImage(ClampCanvasPoint(canvasPoint)));
                    menu.Items.Add("Thêm Barcode", null, (s, args) => AddBarcodeAtViewport(e.Location));
                    menu.Items.Add("Thêm QR Code", null, (s, args) => AddQrCode(ClampCanvasPoint(canvasPoint)));
                    menu.Show(designPanel, e.Location);
                }
            }
        }

        private void DesignPanel_Selection_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || isDrawingLine)
                return;

            var canvasPoint = ToCanvasPoint(e.Location);

            if (TryGetLineEndpointAt(canvasPoint, out var endpointLine, out var isStart))
            {
                selectedControls.Clear();
                selectedLines.Clear();
                selectedLines.Add(endpointLine);
                activeLine = endpointLine;
                resizingLineStart = isStart;
                isResizingLine = true;
                designPanel.Invalidate();
                return;
            }

            if (TryGetLineAt(canvasPoint, out var line))
            {
                if (!selectedLines.Contains(line))
                {
                    selectedControls.Clear();
                    selectedLines.Clear();
                    selectedLines.Add(line);
                }

                isMovingSelection = true;
                selectionDragLast = canvasPoint;
                designPanel.Invalidate();
                return;
            }

            selectedControls.Clear();
            selectedLines.Clear();
            selectionStart = canvasPoint;
            selectionCurrent = canvasPoint;
            isSelecting = true;
            designPanel.Invalidate();
        }

        private void DesignPanel_Selection_MouseMove(object sender, MouseEventArgs e)
        {
            var canvasPoint = ToCanvasPoint(e.Location);
            if (isResizingLine)
            {
                if (resizingLineStart)
                    activeLine.Start = canvasPoint;
                else
                    activeLine.End = canvasPoint;
                designPanel.Invalidate();
                return;
            }

            if (isMovingSelection)
            {
                var deltaX = canvasPoint.X - selectionDragLast.X;
                var deltaY = canvasPoint.Y - selectionDragLast.Y;
                MoveSelectedItems(deltaX, deltaY);
                selectionDragLast = canvasPoint;
                designPanel.Invalidate();
                return;
            }

            if (!isSelecting)
                return;

            selectionCurrent = canvasPoint;
            designPanel.Invalidate();
        }

        private void DesignPanel_Selection_MouseUp(object sender, MouseEventArgs e)
        {
            if (isResizingLine && e.Button == MouseButtons.Left)
            {
                isResizingLine = false;
                activeLine = null;
                return;
            }

            if (isMovingSelection && e.Button == MouseButtons.Left)
            {
                isMovingSelection = false;
                return;
            }

            if (!isSelecting || e.Button != MouseButtons.Left)
                return;

            selectionCurrent = ToCanvasPoint(e.Location);
            var selectionRectangle = GetSelectionRectangle();
            selectedControls.Clear();
            selectedLines.Clear();

            foreach (Control ctrl in designPanel.Controls)
            {
                if (selectionRectangle.IntersectsWith(ctrl.Bounds))
                    selectedControls.Add(ctrl);
            }

            foreach (var line in lines)
            {
                if (selectionRectangle.IntersectsWith(GetLineBounds(line)))
                    selectedLines.Add(line);
            }

            isSelecting = false;
            designPanel.Invalidate();
        }

        private Rectangle GetSelectionRectangle()
        {
            return Rectangle.FromLTRB(
                Math.Min(selectionStart.X, selectionCurrent.X),
                Math.Min(selectionStart.Y, selectionCurrent.Y),
                Math.Max(selectionStart.X, selectionCurrent.X),
                Math.Max(selectionStart.Y, selectionCurrent.Y));
        }

        private Rectangle GetLineBounds(LineObject line)
        {
            return Rectangle.FromLTRB(
                Math.Min(line.Start.X, line.End.X) - currentLineThickness,
                Math.Min(line.Start.Y, line.End.Y) - currentLineThickness,
                Math.Max(line.Start.X, line.End.X) + currentLineThickness,
                Math.Max(line.Start.Y, line.End.Y) + currentLineThickness);
        }

        private void MoveSelectedItems(int deltaX, int deltaY)
        {
            foreach (var ctrl in selectedControls)
            {
                ctrl.Left += deltaX;
                ctrl.Top += deltaY;
            }

            foreach (var line in selectedLines)
            {
                line.Start = new Point(line.Start.X + deltaX, line.Start.Y + deltaY);
                line.End = new Point(line.End.X + deltaX, line.End.Y + deltaY);
            }
        }

        private void DrawLineEndpointHandle(Graphics graphics, Point endpoint, Brush brush, Pen pen)
        {
            const int handleSize = 8;
            var handle = new Rectangle(endpoint.X - handleSize / 2, endpoint.Y - handleSize / 2, handleSize, handleSize);
            graphics.FillRectangle(brush, handle);
            graphics.DrawRectangle(pen, handle);
        }

        private bool TryGetLineEndpointAt(Point point, out LineObject found, out bool isStart)
        {
            const int endpointTolerance = 8;
            for (var index = lines.Count - 1; index >= 0; index--)
            {
                var line = lines[index];
                if (Distance(point, line.Start) <= endpointTolerance)
                {
                    found = line;
                    isStart = true;
                    return true;
                }

                if (Distance(point, line.End) <= endpointTolerance)
                {
                    found = line;
                    isStart = false;
                    return true;
                }
            }

            found = null;
            isStart = false;
            return false;
        }

        private double Distance(Point first, Point second)
        {
            var deltaX = first.X - second.X;
            var deltaY = first.Y - second.Y;
            return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
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
            {
                isLineDragging = true;
                lineStart = ToCanvasPoint(e.Location);
                lineCurrentEnd = lineStart;
                designPanel.Capture = true;
            }
        }

        private void DesignPanel_DrawLine_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDrawingLine || !isLineDragging)
                return;

            var oldEnd = lineCurrentEnd;
            lineCurrentEnd = ToCanvasPoint(e.Location);

            var dirtyRegion = Rectangle.Inflate(
                Rectangle.FromLTRB(
                    Math.Min(lineStart.X, Math.Min(oldEnd.X, lineCurrentEnd.X)),
                    Math.Min(lineStart.Y, Math.Min(oldEnd.Y, lineCurrentEnd.Y)),
                    Math.Max(lineStart.X, Math.Max(oldEnd.X, lineCurrentEnd.X)),
                    Math.Max(lineStart.Y, Math.Max(oldEnd.Y, lineCurrentEnd.Y))),
                currentLineThickness + 6,
                currentLineThickness + 6);

            designPanel.Invalidate(dirtyRegion);
        }

        private void DesignPanel_DrawLine_MouseUp(object sender, MouseEventArgs e)
        {
            if (isDrawingLine && isLineDragging && e.Button == MouseButtons.Left)
            {
                lineCurrentEnd = ToCanvasPoint(e.Location);

                if (lineStart != lineCurrentEnd)
                {
                    lines.Add(new LineObject
                    {
                        Start = lineStart,
                        End = lineCurrentEnd,
                        Thickness = currentLineThickness
                    });
                }

                isLineDragging = false;
                designPanel.Capture = false;
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

        private void BtnExportHtml_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = "HTML files (*.html)|*.html",
                DefaultExt = "html",
                FileName = "label-template.html"
            })
            {
                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                File.WriteAllText(sfd.FileName, BuildHtmlFromDesigner(), new UTF8Encoding(false));
                MessageBox.Show("Đã xuất mẫu HTML thành công.", "Xuất HTML", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private string BuildHtmlFromDesigner()
        {
            var paper = GetPaperDimensionsInMillimeters();
            var canvasSize = GetUnscaledCanvasPaperSize();
            var html = new StringBuilder();
            html.AppendLine("<!doctype html>");
            html.AppendLine("<html><head><meta charset=\"utf-8\"><title>Label template</title>");
            html.AppendLine("<style>");
            html.AppendLine("@page { size: " + paper.Width.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "mm " + paper.Height.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "mm; margin: 0; }");
            html.AppendLine("html, body { margin: 0; padding: 0; background: #eee; }");
            html.AppendLine(".label-canvas { position: relative; overflow: hidden; background: #fff; width: " + canvasSize.Width + "px; height: " + canvasSize.Height + "px; }");
            html.AppendLine(".label-item { position: absolute; box-sizing: border-box; white-space: pre-wrap; overflow: hidden; }");
            html.AppendLine(".label-line { position: absolute; transform-origin: 0 50%; background: #000; }");
            html.AppendLine("@media print { html, body { background: #fff; } .label-canvas { page-break-after: always; } }");
            html.AppendLine("</style></head><body><div class=\"label-canvas\">");

            foreach (Control ctrl in designPanel.Controls)
            {
                var x = UnscaleValue(ctrl.Left);
                var y = UnscaleValue(ctrl.Top);
                var width = UnscaleValue(ctrl.Width);
                var height = UnscaleValue(ctrl.Height);
                var style = "left:" + x + "px;top:" + y + "px;width:" + width + "px;height:" + height + "px;";

                if (ctrl is Label label)
                {
                    var data = label.Tag as ElementData;
                    var text = data != null && data.ReplaceText
                        ? ToHtmlPlaceholder(label.Text)
                        : label.Text;
                    var labelStyle = style
                        + "font-family:" + HtmlEncode(label.Font.FontFamily.Name) + ";"
                        + "font-size:" + (label.Font.Size / zoomFactor).ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) + "px;"
                        + "font-weight:" + (label.Font.Bold ? "bold" : "normal") + ";"
                        + "font-style:" + (label.Font.Italic ? "italic" : "normal") + ";"
                        + "color:" + ColorToCss(label.ForeColor) + ";"
                        + GetHtmlBorderStyle(label);
                    if (label is RotatableLabel rotatable && rotatable.VerticalText)
                        labelStyle += "writing-mode:vertical-rl;";
                    html.Append("<div class=\"label-item\" style=\"")
                        .Append(labelStyle)
                        .Append("\">")
                        .Append(HtmlEncode(text))
                        .AppendLine("</div>");
                }
                else if (ctrl is PictureBox picture && picture.Image != null)
                {
                    var dataUri = ImageToDataUri(picture.Image);
                    var imageStyle = style + GetHtmlBorderStyle(picture);
                    html.Append("<img class=\"label-item\" style=\"")
                        .Append(imageStyle)
                        .Append("\" src=\"")
                        .Append(dataUri)
                        .AppendLine("\" />");
                }
            }

            foreach (var line in lines)
            {
                var start = new Point(UnscaleValue(line.Start.X), UnscaleValue(line.Start.Y));
                var end = new Point(UnscaleValue(line.End.X), UnscaleValue(line.End.Y));
                var lineWidth = Math.Max(1, (int)Math.Round(Math.Sqrt(
                    Math.Pow(end.X - start.X, 2) + Math.Pow(end.Y - start.Y, 2))));
                var angle = Math.Atan2(end.Y - start.Y, end.X - start.X) * 180 / Math.PI;
                html.Append("<div class=\"label-line\" style=\"left:")
                    .Append(start.X).Append("px;top:").Append(start.Y).Append("px;width:")
                    .Append(lineWidth).Append("px;height:").Append(Math.Max(1, line.Thickness / zoomFactor))
                    .Append("px;transform:rotate(").Append(angle.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture))
                    .AppendLine("deg);\"></div>");
            }

            html.AppendLine("</div></body></html>");
            return html.ToString();
        }

        private static string ToHtmlPlaceholder(string value)
        {
            var name = (value ?? string.Empty).Trim().Trim('{', '}').Trim();
            return string.IsNullOrEmpty(name) ? string.Empty : "{{" + name + "}}";
        }

        private static string HtmlEncode(string value)
        {
            return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
        }

        private static string ColorToCss(Color color)
        {
            return "rgb(" + color.R + "," + color.G + "," + color.B + ")";
        }

        private static string GetHtmlBorderStyle(Control control)
        {
            switch (control is Label label ? label.BorderStyle : (control as PictureBox)?.BorderStyle ?? BorderStyle.None)
            {
                case BorderStyle.FixedSingle:
                    return "border:1px solid " + ColorToCss(control.ForeColor) + ";";
                case BorderStyle.Fixed3D:
                    return "border:2px inset " + ColorToCss(control.ForeColor) + ";";
                default:
                    return string.Empty;
            }
        }

        private static string ImageToDataUri(Image image)
        {
            using (var stream = new MemoryStream())
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return "data:image/png;base64," + Convert.ToBase64String(stream.ToArray());
            }
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            var savedScrollPosition = designPanel.AutoScrollPosition;
            designPanel.AutoScrollPosition = Point.Empty;
            designPanel.PerformLayout();
            designPanel.Update();

            var pd = new PrintDocument();
            var paper = GetPaperDimensionsInMillimeters();
            pd.DefaultPageSettings.PaperSize = new PaperSize(
                cmbPaperSize.SelectedItem?.ToString() ?? "Custom",
                MillimetersToHundredthsOfAnInch(paper.Width),
                MillimetersToHundredthsOfAnInch(paper.Height));
            pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            pd.OriginAtMargins = false;
            pd.PrintPage += Pd_PrintPage;
            try
            {
                using (var preview = new PrintPreviewDialog { Document = pd, Width = 800, Height = 600 })
                    preview.ShowDialog();
            }
            finally
            {
                designPanel.AutoScrollPosition = new Point(-savedScrollPosition.X, -savedScrollPosition.Y);
                designPanel.Invalidate();
            }
        }

        private void Pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            var g = e.Graphics;
            var sourceDpi = GetDesignDpi();
            g.PageUnit = System.Drawing.GraphicsUnit.Pixel;
            g.ScaleTransform(g.DpiX / sourceDpi, g.DpiY / sourceDpi);

            // Draw lines
            foreach (var line in lines)
            {
                using (var pen = new Pen(Color.Black, Math.Max(1f, line.Thickness / zoomFactor)))
                    g.DrawLine(pen, ScalePoint(line.Start, 1f / zoomFactor), ScalePoint(line.End, 1f / zoomFactor));
            }
            // Draw controls content without borders
            foreach (Control ctrl in designPanel.Controls)
            {
                if (ctrl is Label lbl)
                {
                    using (var brush = new SolidBrush(lbl.ForeColor))
                    using (var font = new Font(
                        lbl.Font.FontFamily,
                        lbl.Font.Size * sourceDpi / 72f / zoomFactor,
                        lbl.Font.Style,
                        GraphicsUnit.Pixel))
                    {
                        var left = UnscaleValue(lbl.Left);
                        var top = UnscaleValue(lbl.Top);
                        if (lbl is RotatableLabel rotatable && rotatable.VerticalText)
                        {
                            var text = (lbl.Text ?? string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
                            float y = top + UnscaleValue(lbl.Padding.Top);
                            foreach (var character in text)
                            {
                                g.DrawString(character.ToString(), font, brush, left + UnscaleValue(lbl.Padding.Left), y);
                                y += g.MeasureString(character.ToString(), font).Height;
                            }
                        }
                        else
                        {
                            g.DrawString(lbl.Text, font, brush, new PointF(left, top));
                        }
                    }
                }
                else if (ctrl is PictureBox pic && pic.Image != null)
                {
                    g.DrawImage(pic.Image, new Rectangle(
                        UnscaleValue(pic.Left),
                        UnscaleValue(pic.Top),
                        UnscaleValue(ctrl.Width),
                        UnscaleValue(ctrl.Height)));
                }
            }
        }

        private SizeF GetPaperDimensionsInMillimeters()
        {
            switch (cmbPaperSize.SelectedItem?.ToString())
            {
                case "A4": return new SizeF(210, 297);
                case "A5": return new SizeF(148, 210);
                case "Letter": return new SizeF(215.9f, 279.4f);
                default: return new SizeF(customPaperWidth, customPaperHeight);
            }
        }

        private int MillimetersToHundredthsOfAnInch(float millimeters)
        {
            return Math.Max(1, (int)Math.Round(millimeters / 25.4f * 100f));
        }

        private int MillimetersToPixels(float millimeters, float targetDpi)
        {
            return Math.Max(1, (int)Math.Round(millimeters / 25.4f * targetDpi));
        }

        private float GetDesignDpi()
        {
            return cmbPaperSize.SelectedItem?.ToString() == "Tùy chỉnh" ? dpi : 96f;
        }

        private void AddDraggableText(Point location, string text = "{ProductName}", bool ReplaceText = false, bool bold = false, bool italic = false)
        {
            var lbl = new RotatableLabel
            {
                Text = text,
                AutoSize = true,
                BackColor = Color.LightYellow,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = new ElementData { Type = "Label", Text = text, ReplaceText = ReplaceText, Bold = bold, Italic = italic }
            };
            lbl.Font = new Font(
                lbl.Font.FontFamily,
                8.25f * zoomFactor,
                GetFontStyle(bold, italic));
            lbl.Location = ClampCanvasLocation(location, lbl.Size);
            AddCommonHandlers(lbl);
            designPanel.Controls.Add(lbl);
        }

        private void AddTextAtViewport(Point viewportLocation, string text = "{ProductName}")
        {
            AddDraggableText(Point.Empty, text);
            var label = designPanel.Controls[designPanel.Controls.Count - 1];
            var actualViewportLocation = GetControlViewportLocation(label);
            label.Left += viewportLocation.X - actualViewportLocation.X;
            label.Top += viewportLocation.Y - actualViewportLocation.Y;
            designPanel.Invalidate();
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

        private void AddDraggableImage(Point location, string imagePath = "")
        {
            var pic = new PictureBox { Size = new Size(80, 80), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.StretchImage };
            using (var ofd = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" })
                if (string.IsNullOrEmpty(imagePath) && ofd.ShowDialog() == DialogResult.OK)
                    imagePath = ofd.FileName;
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                pic.Image = Image.FromFile(imagePath);
                pic.Tag = new ElementData { Type = "Image", ImagePath = imagePath };
            }
            pic.Location = ClampCanvasLocation(location, pic.Size);
            AddCommonHandlers(pic);
            designPanel.Controls.Add(pic);
        }

        private void AddBarcode(Point location, string text = "1234567890", bool pureBarcode = true)
        {
            var pic = new PictureBox
            {
                Image = CreateBarcodeImage(text, BarcodeFormat.CODE_128, 200, 80, pureBarcode),
                Size = new Size(200, 80),
                BorderStyle = BorderStyle.None,
                Tag = new ElementData { Type = "Barcode", Text = text, PureBarcode = pureBarcode, ReplaceText = true }
            };
            pic.Location = ClampCanvasLocation(location, pic.Size);
            AddCommonHandlers(pic);
            designPanel.Controls.Add(pic);
        }

        private void AddBarcodeAtViewport(Point viewportLocation, string text = "1234567890", bool pureBarcode = true)
        {
            AddBarcode(Point.Empty, text, pureBarcode);
            var barcode = designPanel.Controls[designPanel.Controls.Count - 1];
            var actualViewportLocation = GetControlViewportLocation(barcode);
            barcode.Left += viewportLocation.X - actualViewportLocation.X;
            barcode.Top += viewportLocation.Y - actualViewportLocation.Y;
            designPanel.Invalidate();
        }

        private void AddQrCode(Point location, string text = "https://example.com")
        {
            var pic = new PictureBox
            {
                Image = CreateBarcodeImage(text, BarcodeFormat.QR_CODE, 150, 150, true),
                Size = new Size(150, 150),
                BorderStyle = BorderStyle.None,
                Tag = new ElementData { Type = "QRCode", Text = text, ReplaceText = true }
            };
            pic.Location = ClampCanvasLocation(location, pic.Size);
            AddCommonHandlers(pic);
            designPanel.Controls.Add(pic);
        }

        private static Bitmap CreateBarcodeImage(string text, BarcodeFormat format, int width, int height, bool pureBarcode)
        {
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = format,
                Options = new EncodingOptions
                {
                    Width = Math.Max(1, width),
                    Height = Math.Max(1, height),
                    Margin = format == BarcodeFormat.QR_CODE ? 0 : 2,
                    PureBarcode = format == BarcodeFormat.QR_CODE || pureBarcode
                },
                Renderer = new BitmapBarcodeRenderer()
            };

            return writer.Write(text);
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
                        resizeStart = Cursor.Position;
                        resizeStartSize = ctrl.Size;
                    }
                    else
                    {
                        isDragging = true;
                        dragStartCanvas = ToCanvasPoint(designPanel.PointToClient(Cursor.Position));
                        if (!selectedControls.Contains(ctrl))
                        {
                            selectedControls.Clear();
                            selectedControls.Add(ctrl);
                            selectedLines.Clear();
                        }
                        designPanel.Invalidate();
                    }
                }
            };
            ctrl.MouseMove += (s, e) =>
            {
                var handle = new Rectangle(ctrl.Width - 8, ctrl.Height - 8, 8, 8);
                ctrl.Cursor = handle.Contains(e.Location) ? Cursors.SizeNWSE : Cursors.SizeAll;
                if (isResizing && activeCtrl == ctrl)
                {
                    var curr = Cursor.Position;
                    var newWidth = Math.Max(1, resizeStartSize.Width + curr.X - resizeStart.X);
                    var newHeight = Math.Max(1, resizeStartSize.Height + curr.Y - resizeStart.Y);
                    ctrl.Size = new Size(newWidth, newHeight);
                    if (ctrl is PictureBox pic && pic.Tag is ElementData data)
                    {
                        pic.Image = CreateBarcodeImage(
                            data.Text,
                            data.Type == "Barcode" ? BarcodeFormat.CODE_128 : BarcodeFormat.QR_CODE,
                            pic.Width,
                            pic.Height,
                            data.Type == "Barcode" && data.PureBarcode);
                    }
                    designPanel.Invalidate();
                }
                else if (isDragging && activeCtrl == ctrl)
                {
                    var currentCanvasPoint = ToCanvasPoint(designPanel.PointToClient(Cursor.Position));
                    var deltaX = currentCanvasPoint.X - dragStartCanvas.X;
                    var deltaY = currentCanvasPoint.Y - dragStartCanvas.Y;
                    MoveSelectedItems(deltaX, deltaY);
                    dragStartCanvas = currentCanvasPoint;
                    designPanel.Invalidate();
                }
            };
            ctrl.MouseUp += (s, e) => { isResizing = false; isDragging = false; activeCtrl = null; };
            var menu = new ContextMenuStrip();
            menu.Items.Add("Xoá", null, (s, e) => designPanel.Controls.Remove(ctrl));
            menu.Items.Add("Sao chép", null, (s, e) => CopyElement(ctrl));
            menu.Items.Add("Chỉnh sửa nội dung", null, (s, e) => EditElement(ctrl));
            ctrl.ContextMenuStrip = menu;
        }

        private void CopyElement(Control source)
        {
            var data = source.Tag as ElementData;
            if (data == null)
                return;

            Control copy;
            var location = ClampCanvasLocation(
                new Point(source.Left + 20, source.Top + 20),
                source.Size);

            if (source is Label sourceLabel)
            {
                var copyLabel = new RotatableLabel
                {
                    Text = sourceLabel.Text,
                    AutoSize = sourceLabel.AutoSize,
                    BackColor = sourceLabel.BackColor,
                    BorderStyle = sourceLabel.BorderStyle,
                    ForeColor = sourceLabel.ForeColor,
                    Padding = sourceLabel.Padding,
                    Tag = new ElementData
                    {
                        Type = data.Type,
                        Text = data.Text,
                        ReplaceText = data.ReplaceText,
                        Bold = data.Bold,
                        Italic = data.Italic,
                        ImagePath = data.ImagePath
                    }
                };
                copyLabel.Font = (Font)sourceLabel.Font.Clone();
                copyLabel.Size = source.Size;
                if (sourceLabel is RotatableLabel sourceRotatable)
                    copyLabel.VerticalText = sourceRotatable.VerticalText;
                copy = copyLabel;
            }
            else if (source is PictureBox sourcePicture && sourcePicture.Image != null)
            {
                var copyPicture = new PictureBox
                {
                    Image = new Bitmap(sourcePicture.Image),
                    Size = sourcePicture.Size,
                    BorderStyle = sourcePicture.BorderStyle,
                    SizeMode = sourcePicture.SizeMode,
                    Tag = new ElementData
                    {
                        Type = data.Type,
                        Text = data.Text,
                        PureBarcode = data.PureBarcode,
                        ReplaceText = data.ReplaceText,
                        ImagePath = data.ImagePath
                    }
                };
                copy = copyPicture;
            }
            else
            {
                return;
            }

            copy.Location = location;
            AddCommonHandlers(copy);
            designPanel.Controls.Add(copy);
            selectedControls.Clear();
            selectedLines.Clear();
            selectedControls.Add(copy);
            designPanel.Invalidate();
        }

        private void EditElement(Control ctrl)
        {
            var data = ctrl.Tag as ElementData;
            if (data != null && data.Type == "Barcode")
            {
                using (var dlg = new Form { Width = 300, Height = 200, Text = "Chỉnh sửa nội dung" })
                {
                    var txt = new TextBox { Text = data.Text, Dock = DockStyle.Top };
                    var chkPure = new CheckBox { Text = "PureBarcode (ẩn text dưới mã vạch)", Checked = data.PureBarcode, Dock = DockStyle.Top };
                    var btn = new Button { Text = "OK", Dock = DockStyle.Bottom };
                    btn.Click += (s, e) =>
                    {
                        data.Text = txt.Text;
                        data.PureBarcode = chkPure.Checked;
                        if (ctrl is PictureBox p)
                        {
                            p.Image = CreateBarcodeImage(data.Text, BarcodeFormat.CODE_128, p.Width, p.Height, data.PureBarcode);
                        }
                        dlg.Close();
                    };
                    dlg.Controls.Add(txt);
                    dlg.Controls.Add(chkPure);
                    dlg.Controls.Add(btn);
                    dlg.ShowDialog();
                }
            }
            if (data != null && data.Type == "QRCode")
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
                            p.Image = CreateBarcodeImage(data.Text, BarcodeFormat.QR_CODE, p.Width, p.Height, true);
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
                using (var dlg = new Form { Width = 300, Height = 260, Text = "Chỉnh sửa text" })
                {
                    var txt = new TextBox { Text = lbl.Text, Dock = DockStyle.Top };
                    var chkVertical = new CheckBox
                    {
                        Text = "Chữ dọc (từ trên xuống)",
                        Checked = lbl is RotatableLabel initialRotatable && initialRotatable.VerticalText,
                        Dock = DockStyle.Top
                    };
                    var labelData = lbl.Tag as ElementData;
                    var chkReplace = new CheckBox
                    {
                        Text = "Là trường cần Replace",
                        Checked = labelData?.ReplaceText ?? false,
                        Dock = DockStyle.Top
                    };
                    var chkBold = new CheckBox
                    {
                        Text = "In đậm",
                        Checked = lbl.Font.Bold,
                        Dock = DockStyle.Top
                    };
                    var chkItalic = new CheckBox
                    {
                        Text = "In nghiêng",
                        Checked = lbl.Font.Italic,
                        Dock = DockStyle.Top
                    };
                    var pnlFont = new Panel { Height = 40, Dock = DockStyle.Top };
                    var lblFontSize = new Label { Text = "Cỡ chữ:", AutoSize = true, Location = new Point(5, 10) };
                    var txtFontSize = new TextBox { Text = (lbl.Font.Size / zoomFactor).ToString(), Location = new Point(60, 7), Width = 50 };
                    pnlFont.Controls.Add(lblFontSize);
                    pnlFont.Controls.Add(txtFontSize);

                    var btn = new Button { Text = "OK", Dock = DockStyle.Bottom };
                    btn.Click += (s, e) => {
                        lbl.Text = txt.Text;
                        if (labelData == null)
                        {
                            labelData = new ElementData { Type = "Label" };
                            lbl.Tag = labelData;
                        }
                        labelData.Text = lbl.Text;
                        labelData.ReplaceText = chkReplace.Checked;
                        labelData.Bold = chkBold.Checked;
                        labelData.Italic = chkItalic.Checked;
                        if (float.TryParse(txtFontSize.Text, out float newSize) && newSize > 0)
                        {
                            lbl.Font = new Font(lbl.Font.FontFamily, newSize * zoomFactor, GetFontStyle(chkBold.Checked, chkItalic.Checked));
                        }
                        else
                        {
                            lbl.Font = new Font(lbl.Font.FontFamily, lbl.Font.Size, GetFontStyle(chkBold.Checked, chkItalic.Checked));
                        }
                        if (lbl is RotatableLabel rotatable)
                            SetLabelOrientation(rotatable, chkVertical.Checked);
                        dlg.Close();
                    };
                    dlg.Controls.Add(txt);
                    dlg.Controls.Add(chkVertical);
                    dlg.Controls.Add(chkReplace);
                    dlg.Controls.Add(chkBold);
                    dlg.Controls.Add(chkItalic);
                    dlg.Controls.Add(pnlFont);
                    dlg.Controls.Add(btn);
                    dlg.ShowDialog();
                }
            }
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

        private void SaveLayout(string filePath)
        {
            var items = new List<LayoutItem>();
            foreach (Control ctrl in designPanel.Controls)
            {
                var contentLocation = GetControlContentLocation(ctrl);
                if (ctrl is Label lbl)
                    items.Add(new LayoutItem
                    {
                        Type = "Label",
                        Text = lbl.Text,
                        X = UnscaleValue(contentLocation.X),
                        Y = UnscaleValue(contentLocation.Y),
                        Width = UnscaleValue(lbl.Width),
                        Height = UnscaleValue(lbl.Height),
                        FontSize = lbl.Font.Size / zoomFactor,
                        VerticalText = lbl is RotatableLabel rotatable && rotatable.VerticalText,
                        ReplaceText = (lbl.Tag as ElementData)?.ReplaceText ?? false,
                        Bold = lbl.Font.Bold,
                        Italic = lbl.Font.Italic
                    });
                else if (ctrl is PictureBox pic)
                {
                    var data = pic.Tag as ElementData;
                    if (data != null)
                        items.Add(new LayoutItem
                        {
                            Type = data.Type,
                            Text = data.Text,
                            X = UnscaleValue(contentLocation.X),
                            Y = UnscaleValue(contentLocation.Y),
                            Width = UnscaleValue(pic.Width),
                            Height = UnscaleValue(pic.Height),
                            ImagePath = data.Type == "Image" ? data.ImagePath : null,
                            PureBarcode = data.Type == "Barcode" ? data.PureBarcode : (bool?)null,
                            ReplaceText = data.Type == "Barcode" || data.Type == "QRCode" ? data.ReplaceText : (bool?)null
                        });
                }
            }
            var layout = new LayoutData
            {
                Items = items,
                Lines = GetUnscaledLines(),
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
            var layout = JsonConvert.DeserializeObject<LayoutData>(File.ReadAllText(filePath));
            if (layout == null)
                return;

            selectedControls.Clear();
            selectedLines.Clear();
            activeCtrl = null;
            activeLine = null;
            isSelecting = false;
            isMovingSelection = false;
            isResizingLine = false;

            // Áp dụng lại thông tin khổ giấy nếu có
            if (!string.IsNullOrEmpty(layout.PaperSize))
                cmbPaperSize.SelectedItem = layout.PaperSize;
            if (layout.PaperWidth.HasValue)
                customPaperWidth = layout.PaperWidth.Value;
            if (layout.PaperHeight.HasValue)
                customPaperHeight = layout.PaperHeight.Value;
            if (layout.Dpi.HasValue)
                dpi = layout.Dpi.Value;

            if (zoomFactor != 1f)
                SetZoom(1f);

            designPanel.Controls.Clear();
            lines.Clear();
            UpdateCanvasScrollSize();
            designPanel.AutoScrollPosition = Point.Empty;
            designPanel.PerformLayout();

            foreach (var item in layout.Items ?? new List<LayoutItem>())
            {
                var loc = new Point(item.X, item.Y);
                Control ctrl;
                switch (item.Type)
                {
                    case "Label":
                        AddDraggableText(loc, item.Text, item.ReplaceText ?? false, item.Bold ?? false, item.Italic ?? false);
                        var lbl = designPanel.Controls[designPanel.Controls.Count - 1] as Label;
                        if (lbl is RotatableLabel rotatable)
                        {
                            if (item.FontSize.HasValue)
                                rotatable.Font = new Font(rotatable.Font.FontFamily, item.FontSize.Value, GetFontStyle(item.Bold ?? false, item.Italic ?? false));
                            SetLabelOrientation(rotatable, item.VerticalText ?? false);
                        }
                        break;
                    case "Barcode":
                        AddBarcode(loc, item.Text, item.PureBarcode ?? true);
                        break;
                    case "QRCode":
                        AddQrCode(loc, item.Text);
                        break;
                    case "Image":
                        AddDraggableImage(loc, item.ImagePath);
                        break;
                    default:
                        continue;
                }

                ctrl = designPanel.Controls[designPanel.Controls.Count - 1];
                ctrl.Location = loc;
                ctrl.Size = new Size(item.Width, item.Height);

                if (ctrl is PictureBox pic && pic.Tag is ElementData data)
                {
                    if (data.Type == "Barcode" || data.Type == "QRCode")
                        data.ReplaceText = item.ReplaceText ?? true;

                    pic.Image = CreateBarcodeImage(
                        data.Text,
                        data.Type == "Barcode" ? BarcodeFormat.CODE_128 : BarcodeFormat.QR_CODE,
                        pic.Width,
                        pic.Height,
                        data.Type == "Barcode" && data.PureBarcode);
                }
            }
            if (layout.Lines != null)
                lines = new List<LineObject>(layout.Lines);
            UpdateCanvasScrollSize();
            designPanel.Invalidate();
        }

        private List<LineObject> GetUnscaledLines()
        {
            var result = new List<LineObject>();
            foreach (var line in lines)
            {
                result.Add(new LineObject
                {
                    Start = ScalePoint(line.Start, 1f / zoomFactor),
                    End = ScalePoint(line.End, 1f / zoomFactor),
                    Thickness = Math.Max(1, (int)Math.Round(line.Thickness / zoomFactor))
                });
            }
            return result;
        }

        private int UnscaleValue(int value)
        {
            return Math.Max(1, (int)Math.Round(value / zoomFactor));
        }

        private void UpdateCanvasScrollSize()
        {
            if (designPanel == null)
                return;

            var paperSize = GetCanvasPaperSize();
            designPanel.AutoScrollMinSize = new Size(
                paperSize.Width + canvasScrollPadding,
                paperSize.Height + canvasScrollPadding);
            designPanel.Invalidate();
        }

        private Size GetCanvasPaperSize()
        {
            var paperSize = GetUnscaledCanvasPaperSize();
            return new Size(
                Math.Max(1, (int)Math.Round(paperSize.Width * zoomFactor)),
                Math.Max(1, (int)Math.Round(paperSize.Height * zoomFactor)));
        }

        private Size GetUnscaledCanvasPaperSize()
        {
            Size paperSize;
            switch (cmbPaperSize?.SelectedItem?.ToString())
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

        private Point ToCanvasPoint(Point clientPoint)
        {
            return new Point(
                clientPoint.X - designPanel.AutoScrollPosition.X,
                clientPoint.Y - designPanel.AutoScrollPosition.Y);
        }

        private Point ClampCanvasPoint(Point point)
        {
            return ClampCanvasLocation(point, Size.Empty);
        }

        private Point ClampCanvasLocation(Point point, Size elementSize)
        {
            var paperSize = GetCanvasPaperSize();
            var maxX = Math.Max(0, paperSize.Width - Math.Max(1, elementSize.Width));
            var maxY = Math.Max(0, paperSize.Height - Math.Max(1, elementSize.Height));
            return new Point(
                Math.Max(0, Math.Min(point.X, maxX)),
                Math.Max(0, Math.Min(point.Y, maxY)));
        }

        private sealed class BufferedPanel : Panel
        {
            public BufferedPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                SetStyle(
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer,
                    true);
            }

            protected override void OnMouseWheel(MouseEventArgs e)
            {
                var currentScroll = AutoScrollPosition;
                var nextScrollY = Math.Max(0, -currentScroll.Y - e.Delta / 3);
                AutoScrollPosition = new Point(-currentScroll.X, nextScrollY);
            }
        }

        private sealed class RotatableLabel : Label
        {
            public bool VerticalText { get; set; }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (!VerticalText)
                {
                    base.OnPaint(e);
                    return;
                }

                e.Graphics.Clear(BackColor);
                var text = (Text ?? string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
                using (var brush = new SolidBrush(ForeColor))
                {
                    var y = Padding.Top;
                    foreach (var character in text)
                    {
                        e.Graphics.DrawString(character.ToString(), Font, brush, Padding.Left, y);
                        y += Font.Height;
                    }
                }

                if (BorderStyle == BorderStyle.FixedSingle)
                    ControlPaint.DrawBorder(e.Graphics, ClientRectangle, ForeColor, ButtonBorderStyle.Solid);
                else if (BorderStyle == BorderStyle.Fixed3D)
                    ControlPaint.DrawBorder3D(e.Graphics, ClientRectangle, Border3DStyle.Sunken);
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
                    {
                        bitmap.SetPixel(x, y, matrix[x, y] ? Color.Black : Color.White);
                    }
                }

                return bitmap;
            }
        }

        public class LineObject { public Point Start { get; set; } public Point End { get; set; } public int Thickness { get; set; } }
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
    }
}
