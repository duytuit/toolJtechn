using Newtonsoft.Json;
using productLapRap.common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
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

        SerialPort sp;
        string[] oldPorts = new string[0];
        DateTime lastDataTime = DateTime.MinValue;

        // Dictionary lưu trạng thái nhấp nháy của từng đèn
        private Dictionary<int, CancellationTokenSource> ledBlinkTokens = new Dictionary<int, CancellationTokenSource>();

        // Chế độ hiện tại: true = chỉ 1 đèn, false = nhiều đèn
        private bool singleLedMode = true;

        // Biến ghi nhớ đèn đang nhấp nháy (chỉ dùng cho chế độ singleLedMode)
        private int currentLed;

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
        private void Form1_Load(object sender, EventArgs e) { 

        }
        // Liệt kê cổng COM
       // private void LoadCOMPorts()
       // {
       //     string[] ports = SerialPort.GetPortNames();
       //     // Nếu có thay đổi mới update ComboBox
       //     if (!ports.SequenceEqual(oldPorts))
       //     {
       //         oldPorts = ports;
       //         cbListPort.Items.Clear();
       //         cbListPort.Items.AddRange(ports);
       //         if (ports.Length > 0)
       //             cbListPort.SelectedIndex = 0;
       //     }
       // }
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
            if (!string.IsNullOrEmpty(item.location))
            {
                string digits = new string(item.location.Where(char.IsDigit).ToArray());
                int number = int.Parse(digits);
                if (singleLedMode)
                    SwitchBlinkSingle(number-1, 500);
                else
                    SwitchBlinkMulti(number-1, 500);

            }
        }
        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = sp.ReadLine().Trim(); // đọc từng dòng
                                                    // Gọi về UI thread
                this.Invoke((MethodInvoker)delegate
                {
                    richTextBoxArduino.AppendText(data + Environment.NewLine);
                    // Đảm bảo cuộn xuống cuối
                    richTextBoxArduino.SelectionStart = richTextBoxArduino.Text.Length;
                    richTextBoxArduino.ScrollToCaret();
                });
               
                lastDataTime = DateTime.Now; // Cập nhật thời gian nhận dữ liệu
                if (data == "NEXT")
                {
                    // Gọi event btnNext_Click trên UI thread
                    if (this.IsHandleCreated)
                    {
                        this.BeginInvoke((MethodInvoker)delegate {
                            btnNext.PerformClick();
                        });
                    }
                }
                else if (data == "PREV")
                {
                    if (this.IsHandleCreated)
                    {
                        this.BeginInvoke((MethodInvoker)delegate {
                            btnPrev.PerformClick();
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                HandleDisconnect("Mất kết nối khi đọc: " + ex.Message);
            }
        }
        private string FindArduinoPort()
        {
            using (var searcher = new ManagementObjectSearcher(
                "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'"))
            {
                foreach (var device in searcher.Get())
                {
                    string name = device["Name"]?.ToString() ?? "";

                    // Kiểm tra có chữ "Arduino"
                    if (name.IndexOf("Arduino", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // Ví dụ: "Arduino Mega 2560 (COM3)"
                        int start = name.IndexOf("(COM") + 1;
                        int end = name.IndexOf(")", start);
                        if (start > 0 && end > start)
                        {
                            string comPort = name.Substring(start, end - start); // "COM3"

                            // Cập nhật TextBox hiển thị thông tin
                            txtModule.Text = name;  // VD: "Arduino Mega 2560 (COM3)"

                            return comPort;
                        }
                    }
                }
            }
            return null;
        }

        private void SafeWrite(string text)
        {
            if (sp != null && sp.IsOpen)
            {
                try
                {
                    sp.WriteLine(text);
                }
                catch (Exception ex)
                {
                    //HandleDisconnect("Mất kết nối khi gửi: " + ex.Message);
                }
            }
        }
        private void HandleDisconnect(string reason)
        {
            if (sp != null)
            {
                try { sp.Close(); } catch { }
                sp = null;
                this.Invoke(new Action(() =>
                {
                    btnConnect.Text = "Kết nối module";
                    btnConnect.BackColor = Color.White;
                    MessageBox.Show("⚠️ Arduino ngắt kết nối: " + reason + Environment.NewLine);
                }));
            }
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                if (_productContents.Count == 0) return;
                currentIndex++;
                if (currentIndex >= buttons.Count) currentIndex = 0;
                HighlightButton(currentIndex);
                ShowNoteAndImage(currentIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi trong btnNext_Click: " + ex.Message);
            }
           
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            try
            {
                if (_productContents.Count == 0) return;
                currentIndex--;
                if (currentIndex < 0) currentIndex = buttons.Count - 1;
                HighlightButton(currentIndex);
                ShowNoteAndImage(currentIndex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi trong btnNext_Click: " + ex.Message);
            }
         
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
                mouseDownPosition = e.Location; panel6.Invalidate();
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
            zoomFactor *= e.Delta > 0 ? 1.2f : 1 / 1.2f;
            _pan.X = (int)(e.X - (e.X - _pan.X) * (zoomFactor / oldZoom)); 
            _pan.Y = (int)(e.Y - (e.Y - _pan.Y) * (zoomFactor / oldZoom));
            panel6.Invalidate(); 
        }

        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
           // LoadCOMPorts();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (sp == null || !sp.IsOpen)
            {
                try
                {
                    string arduinoPort = FindArduinoPort();
                    if (!string.IsNullOrEmpty(arduinoPort))
                    {
                        sp = new SerialPort(arduinoPort, 9600);
                        sp.DataReceived += Sp_DataReceived;
                        sp.Open();

                        btnConnect.Text = "Ngắt kết nối";
                        btnConnect.BackColor = Color.LawnGreen;
                    }
                    else
                    {
                        txtModule.Text = "";
                        btnConnect.BackColor = Color.White;
                        btnConnect.Text = "Kết nối module";
                        MessageBox.Show("❌ Không tìm thấy Arduino!");
                    }
                }
                catch (Exception ex)
                {
                    txtModule.Text = "";
                    btnConnect.BackColor = Color.White;
                    btnConnect.Text = "Kết nối module";
                    MessageBox.Show("Lỗi kết nối: " + ex.Message);
                }
            }
            else
            {
                try
                {
                    sp.Close();

                }
                catch (Exception ex)
                {
                    txtModule.Text = "";
                    MessageBox.Show("Lỗi khi ngắt kết nối: " + ex.Message);
                }
                finally
                {
                    sp = null;
                    txtModule.Text = "";
                    btnConnect.BackColor = Color.White;
                    btnConnect.Text = "Kết nối module";
                }
            }
        }
        // ==========================
        // Chế độ 1: chỉ 1 đèn nhấp nháy
        // ==========================
        private async void SwitchBlinkSingle(int ledNumber, int intervalMs)
        {
            // Nếu nhấn cùng đèn → tắt đèn
            if (currentLed == ledNumber)
            {
                StopAllBlink();
                return;
            }

            // Tắt tất cả đèn đang nhấp nháy
            StopAllBlink();

            // Bật đèn mới
            var cts = new CancellationTokenSource();
            ledBlinkTokens[ledNumber] = cts;
            currentLed = ledNumber;

            try
            {
                await BlinkLedContinuousAsync(ledNumber, intervalMs, cts.Token);
            }
            catch (OperationCanceledException)
            {
                SafeWrite($"OFF{ledNumber}");
                ledBlinkTokens.Remove(ledNumber);
                currentLed = 0;
            }
        }

        // ==========================
        // Chế độ 2: nhiều đèn nhấp nháy độc lập
        // ==========================
        private async void SwitchBlinkMulti(int ledNumber, int intervalMs)
        {
            // Nếu đèn này đang nhấp nháy → tắt nó
            if (ledBlinkTokens.ContainsKey(ledNumber))
            {
                StopBlink(ledNumber);
                return;
            }

            // Tạo CancellationTokenSource mới cho đèn này
            var cts = new CancellationTokenSource();
            ledBlinkTokens[ledNumber] = cts;

            try
            {
                await BlinkLedContinuousAsync(ledNumber, intervalMs, cts.Token);
            }
            catch (OperationCanceledException)
            {
                SafeWrite($"OFF{ledNumber}");
                ledBlinkTokens.Remove(ledNumber);
            }
        }

        // ==========================
        // Hàm nhấp nháy liên tục cho 1 đèn
        // ==========================
        private async Task BlinkLedContinuousAsync(int ledNumber, int intervalMs, CancellationToken token)
        {
            if (sp == null || !sp.IsOpen)
                return;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                SafeWrite($"ON{ledNumber}");
                await Task.Delay(intervalMs, token);

                SafeWrite($"OFF{ledNumber}");
                await Task.Delay(intervalMs, token);
            }
        }

        // ==========================
        // Hàm tắt một đèn hoặc tất cả đèn
        // ==========================
        private void StopBlink(int ledNumber)
        {
            if (ledBlinkTokens.ContainsKey(ledNumber))
            {
                ledBlinkTokens[ledNumber].Cancel();
                SafeWrite($"OFF{ledNumber}");
                ledBlinkTokens.Remove(ledNumber);
            }
        }

        private void StopAllBlink()
        {
            // Copy key trước khi duyệt
            foreach (var key in ledBlinkTokens.Keys.ToArray())
            {
                ledBlinkTokens[key].Cancel();
                SafeWrite($"OFF{key}");
                ledBlinkTokens.Remove(key);
            }

            currentLed = 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                StopAllBlink();
            }
            catch (Exception ex)
            {
               
            }
        }
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
