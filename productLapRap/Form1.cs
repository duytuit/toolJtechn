using Newtonsoft.Json;
using productLapRap.common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace productLapRap
{
    public partial class Form1 : Form
    {
        private int currentIndex = 0;
        private List<Button> buttons = new List<Button>();
        private List<ProductContent> _productContents = new List<ProductContent>();

        // Pan & Zoom
        private Image _image;
        private float zoomFactor = 1.0f;
        private Point _pan = new Point(0, 0);

        private Point mouseDownPosition;
        private bool isDragging = false;

        public Form1()
        {
            InitializeComponent();

            // Setup panel6 cho vẽ ảnh
            panel6.Paint += Panel6_Paint;

            panel6.MouseDown += Panel6_MouseDown;
            panel6.MouseMove += Panel6_MouseMove;
            panel6.MouseUp += Panel6_MouseUp;
            panel6.MouseWheel += Panel6_MouseWheel;

            panel6.Focus();
            panel6.DoubleBuffered(true); // tránh nhấp nháy
        }
        private void Form1_Load(object sender, EventArgs e) { }
        #region Load dữ liệu và Button

        private void LoadData(List<ProductContent> productContents)
        {
            panelListQuyTrinh.Controls.Clear();
            buttons.Clear();
            _productContents.Clear();
            _productContents = productContents;

            for (int i = 0; i < productContents.Count; i++)
            {
                var item = productContents[i];
                Button btn = new Button
                {
                    Text = $"{item.location} - {item.part_code}",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Height = 30,
                    Dock = DockStyle.Top,
                    Tag = i
                };
                btn.Click += Btn_Click;
                panelListQuyTrinh.Controls.Add(btn);
                panelListQuyTrinh.Controls.SetChildIndex(btn, 0);
                buttons.Add(btn);
            }

            if (buttons.Count > 0)
            {
                currentIndex = 0;
                HighlightButton(currentIndex);
                ShowNoteAndImage(currentIndex);
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is int index)
            {
                currentIndex = index;
                HighlightButton(currentIndex);
                ShowNoteAndImage(currentIndex);
            }
        }

        private void HighlightButton(int index)
        {
            foreach (var btn in buttons)
                btn.BackColor = SystemColors.Control;
            buttons[index].BackColor = Color.Wheat;
        }

        private void ShowNoteAndImage(int index)
        {
            if (index < 0 || index >= _productContents.Count) return;
            var item = _productContents[index];

            richTextBoxDienGiai.Text = item.note;

            if (!string.IsNullOrEmpty(item.image))
            {
                try
                {
                    string basePath = @"\\192.168.207.6\jtecdata\JTEC_PD_PROGAM\CMSWeb\jtecweb\public";
                    string fullPath = System.IO.Path.Combine(basePath, item.image);

                    _image?.Dispose();
                    _image = Image.FromFile(fullPath);

                    zoomFactor = 1.0f;
                    _pan = new Point(0, 0);

                    panel6.Invalidate(); // vẽ lại panel
                }
                catch
                {
                    _image = null;
                }
            }
            else
            {
                _image = null;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_productContents.Count == 0) return;
            currentIndex++;
            if (currentIndex >= buttons.Count) currentIndex = 0;
            HighlightButton(currentIndex);
            ShowNoteAndImage(currentIndex);
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (_productContents.Count == 0) return;
            currentIndex--;
            if (currentIndex < 0) currentIndex = buttons.Count - 1;
            HighlightButton(currentIndex);
            ShowNoteAndImage(currentIndex);
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;

            lbMaSanPham.Text = "";
            try
            {
                string qrCode = textBox2.Text.Trim();
                string[] parts = qrCode.Split(',');
                string result = parts[2];
                textBox2.Text = "";
                using (var _db = new MySqlHelper())
                {
                    DataRow rs = _db.GetSingleRecord("product_lap_raps", result, "code");
                    if (rs != null)
                    {
                        lbMaSanPham.Text = rs["code"].ToString();
                        var content = rs["content"].ToString();
                        var items = JsonConvert.DeserializeObject<List<ProductContent>>(content);
                        LoadData(items);
                    }
                    else
                    {
                        panelListQuyTrinh.Controls.Clear();
                        buttons.Clear();
                        _productContents.Clear();
                        MessageBox.Show("Không tìm thấy dữ liệu!");
                    }
                }

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        #endregion

        #region Pan + Zoom trong panel6

        private void Panel6_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Gray);
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            if (_image != null)
            {
                e.Graphics.TranslateTransform(_pan.X, _pan.Y);
                e.Graphics.ScaleTransform(zoomFactor, zoomFactor);
                e.Graphics.DrawImage(_image, 0, 0);
            }
        }

        private void Panel6_MouseDown(object sender, MouseEventArgs e)
        {
            if (_image == null) return;
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                mouseDownPosition = e.Location;
                panel6.Cursor = Cursors.SizeAll;
            }
        }

        private void Panel6_MouseMove(object sender, MouseEventArgs e)
        {
            if (_image == null) return;
            if (isDragging)
            {
                _pan.X += e.X - mouseDownPosition.X;
                _pan.Y += e.Y - mouseDownPosition.Y;
                mouseDownPosition = e.Location;
                panel6.Invalidate();
            }
        }

        private void Panel6_MouseUp(object sender, MouseEventArgs e)
        {
            if (_image == null) return;
            isDragging = false;
            panel6.Cursor = Cursors.Default;
        }

        private void Panel6_MouseWheel(object sender, MouseEventArgs e)
        {
            if (_image == null) return;

            float oldZoom = zoomFactor;
            zoomFactor *= e.Delta > 0 ? 1.1f : 1 / 1.1f;

            _pan.X = (int)(e.X - (e.X - _pan.X) * (zoomFactor / oldZoom));
            _pan.Y = (int)(e.Y - (e.Y - _pan.Y) * (zoomFactor / oldZoom));


            panel6.Invalidate();
        }

        #endregion
    }

    public class ProductContent
    {
        public string image { get; set; }
        public string location { get; set; }
        public string part_code { get; set; }
        public string note { get; set; }
    }

    // Extension để bật DoubleBuffered cho Panel
    public static class ControlExtensions
    {
        public static void DoubleBuffered(this Control control, bool enable)
        {
            var property = typeof(Control).GetProperty("DoubleBuffered",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            property.SetValue(control, enable, null);
        }
    }
}
