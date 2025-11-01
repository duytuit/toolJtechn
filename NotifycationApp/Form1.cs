using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using UITimer = System.Windows.Forms.Timer;

namespace NotifycationApp
{
    public partial class Form1 : Form
    {
        private UITimer shakeTimer;
        private int shakeStep = 0;
        private PictureBox pic; // <--- thêm dòng này
        private PictureBox circle; // giữ tham chiếu để lắc
        private int baseY = 0;     // lưu vị trí gốc Y để dao động quanh nó
        private WebSocketClient _client;
        private CancellationTokenSource _cts;
        private List<notify> _notify = new List<notify>();
        public Form1()
        {
            InitializeComponent();
            richTextBox1.DetectUrls = true;
            richTextBox1.LinkClicked += RichTextBox1_LinkClicked;
            string code = Environment.GetEnvironmentVariable("MY_APP_CODENV", EnvironmentVariableTarget.User);
            txtCodeNV.Text = code;
            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width, workingArea.Bottom - Size.Height);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.WindowState = FormWindowState.Minimized;
            // Ảnh nền
            pic = new PictureBox();
            pic.Image = Image.FromFile("Screenshot_2025-10-31_074635-removebg-preview.png");
            pic.SizeMode = PictureBoxSizeMode.Zoom;
            pic.SetBounds(300, 18, 200, 120);
            this.Controls.Add(pic);

            DrawCircleAbovePicture(pic, 0, 405, 18);

            // Tạo timer dung
            shakeTimer = new UITimer();
            shakeTimer.Interval = 50; // tốc độ dung (ms)
            shakeTimer.Tick += ShakeTimer_Tick;
            shakeTimer.Start();
            connectSocket();
            //richTextBox1.AppendText("Mở website tại: http://192.168.207.6:8088/admin \n");
            getData();

        }
        private async void connectSocket()
        {

            _cts = new CancellationTokenSource();
            _client = new WebSocketClient("wss://192.168.207.6:5007/ws");

            //_client.OnLog += (msg) => Invoke((Action)(() => richTextBox1.AppendText(msg + "\n")));
            _client.OnMessageReceived += (msg) => Invoke((Action)(() => {
                try
                {
                    var data = JsonConvert.DeserializeObject<JObject>(msg); // parse chuỗi JSON
                    if (IsJsonObject(data["messageText"]))
                    {
                        // Chuyển messageText từ string JSON sang JObject
                        var msgObj = JsonConvert.DeserializeObject<JObject>(data["messageText"].ToString());

                        // ✅ Trích xuất dữ liệu từ msgObj
                        int job_id = int.Parse(msgObj["job_id"]?.ToString());
                        string job_name = msgObj["job_name"]?.ToString();
                        string app = msgObj["app"]?.ToString();
                        string code = msgObj["code"]?.ToString();
                        string link = msgObj["link"]?.ToString();
                        int status = int.Parse( msgObj["status"]?.ToString());
                        if (txtCodeNV.Text == code)
                        {
                            // ⚠️ code lỗi của bạn: `"Code "` có khoảng trắng => phải sửa lại `"Code"`
                            notify check_notify = _notify.FirstOrDefault(x =>
                                x.JobId == job_id &&
                                x.App == app &&
                                x.Code == code
                            );

                            if (check_notify == null)
                            {

                                var newNotify = new notify
                                {
                                    JobId = job_id,
                                    JobName = job_name,
                                    App = app,
                                    Code = code,
                                    Link = msgObj["link"]?.ToString(),
                                    Status = msgObj["status"]?.Value<int>() ?? 0 // add
                                };
                                var newNotify_object = new
                                {
                                    id = 0,
                                    job_id = job_id,
                                    job_name = job_name,
                                    app = app,
                                    code = code,
                                    link = msgObj["link"]?.ToString(),
                                    status = msgObj["status"]?.Value<int>() ?? 0 // add
                                };
                                _notify.Add(newNotify);

                                string _title = $"Công việc - {job_name}:\n";
                                AppendColoredText(richTextBox1, _title, Color.Yellow, Color.Black, bold: true);
                                string _link = $"{msgObj["link"]?.ToString()}\n";
                                AppendColoredText(richTextBox1,_link);

                                // Tạo hình tròn nằm trên ảnh
                                DrawCircleAbovePicture(pic, _notify.Count, 405, 18);
                                // Ghi vào DB
                                using (var _db = new clsKetNoi())
                                {
                                    _db.UpsertFromObject("Notifycation", newNotify_object);
                                }
                            }
                            else
                            {
                                if (status == 1) // xóa
                                {
                                    richTextBox1.Text = "";
                                    _notify.Remove(check_notify);
                                    foreach (var item in _notify)
                                    {
                                        string _title = $"Công việc - {item.JobName}:\n";
                                        AppendColoredText(richTextBox1, _title, Color.Yellow, Color.Black, bold: true);
                                        string _link = $"{item.Link}\n";
                                        AppendColoredText(richTextBox1, _link);
                                    }
                                    DrawCircleAbovePicture(pic, _notify.Count, 405, 18);
                                    using (var _db = new clsKetNoi())
                                    {
                                        var whereEquals = new Dictionary<string, object>
                                        {
                                            ["job_id"] = job_id,
                                            ["app"] = app
                                        };
                                        _db.DeleteWhere("Notifycation", whereEquals);
                                    }

                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    richTextBox1.AppendText($"[Error parsing message] {ex.Message}\n");
                }

            }));
           // _client.OnConnected += () => Invoke((Action)(() => labelStatus.Text = "🟢 Connected"));
           // _client.OnDisconnected += () => Invoke((Action)(() => labelStatus.Text = "🔴 Disconnected"));

            await _client.StartAsync(_cts.Token);
            var obj = new
            {
                Event = 15,
                Chanel = "dencanhbao_cd_dap",
                MessageText= ""
            };
            string jsonData = JsonConvert.SerializeObject(obj);
            await _client.SendMessageAsync(jsonData, _cts.Token);
        }
        private void DrawCircleAbovePicture(PictureBox basePicture, int number, int x, int y)
        {
            int diameter = 50;

            // 🧹 Xóa hình tròn cũ (nếu có)
            if (circle != null)
            {
                if (circle.Image != null)
                {
                    circle.Image.Dispose(); // giải phóng ảnh cũ
                    circle.Image = null;
                }
                if (circle.Parent != null)
                    circle.Parent.Controls.Remove(circle); // gỡ khỏi giao diện

                circle.Dispose();
                circle = null;
            }

            // 🟢 Tạo mới hình tròn
            circle = new PictureBox
            {
                Width = diameter,
                Height = diameter,
                Left = x,
                Top = y,
                BackColor = Color.Transparent
            };

            // Cắt hình dạng PictureBox thành hình tròn
            using (GraphicsPath gp = new GraphicsPath())
            {
                gp.AddEllipse(0, 0, diameter - 1, diameter - 1);
                circle.Region = new Region(gp);
            }

            // 🎨 Vẽ hình tròn và số bên trong
            Bitmap bmp = new Bitmap(diameter, diameter);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);

                using (Brush brush = new SolidBrush(Color.Red))
                    g.FillEllipse(brush, 0, 0, diameter - 1, diameter - 1);

                using (Pen pen = new Pen(Color.White, 5))
                    g.DrawEllipse(pen, 0, 0, diameter - 1, diameter - 1);

                using (Font font = new Font("Arial", 14, FontStyle.Bold))
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    string text = number.ToString();
                    SizeF textSize = g.MeasureString(text, font);
                    float textX = (diameter - textSize.Width) / 2;
                    float textY = (diameter - textSize.Height) / 2;
                    g.DrawString(text, font, textBrush, textX, textY);
                }
            }

            // Gán lại hình
            circle.Image = bmp;

            // 🧩 Gắn vào giao diện
            basePicture.Parent.Controls.Add(circle);
            circle.BringToFront();

            // ✅ Lưu vị trí gốc để dùng cho hiệu ứng rung
            baseY = circle.Top;
        }

        private void ShakeTimer_Tick(object sender, EventArgs e)
        {
            if (_notify.Count >0)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                if (circle == null) return;

                // ⚡ Dung theo trục Y (lên xuống)
                int amplitude = 4; // biên độ dung (px)
                double speed = 0.5; // tốc độ (càng lớn càng nhanh)
                int offset = (int)(Math.Sin(shakeStep * speed) * amplitude);

                circle.Top = baseY + offset;
                shakeStep++;
            }
            
        }
        private void getData()
        {
            _notify.Clear();
            richTextBox1.Text = "";
            DrawCircleAbovePicture(pic, _notify.Count, 405, 18);
            if (txtCodeNV.Text != null)
            {
                string code = txtCodeNV.Text;
                using (var _db = new clsKetNoi())
                {
                    string sql = $@"select * from Notifycation where code = N'{code}'";
                    DataTable table = _db.LoadTable(sql);
                    if (table.Rows.Count > 0)
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            var notify = new notify
                            {
                                JobId = Convert.ToInt32(row["job_id"]),
                                JobName = row["job_name"].ToString(),
                                App = row["app"].ToString(),
                                Code = row["code"].ToString(),
                                Link = row["link"].ToString(),
                                Status = Convert.ToInt32(row["status"])
                            };

                            _notify.Add(notify);
                            string _title = $"Công việc - {row["job_name"].ToString()}:\n";
                            AppendColoredText(richTextBox1, _title, Color.Yellow, Color.Black, bold: true);
                            string _link = $"{ row["link"].ToString()}\n";
                            AppendColoredText(richTextBox1, _link);
                        }
                        DrawCircleAbovePicture(pic, _notify.Count, 405, 18);
                    }
                }
            }
        }
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Environment.SetEnvironmentVariable("MY_APP_CODENV", txtCodeNV.Text, EnvironmentVariableTarget.User);
            getData();
        }
        private void RichTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.LinkText);
            }
            catch
            {
                MessageBox.Show("Không thể mở liên kết này.");
            }
        }
        public static bool IsJsonObject(JToken token)
        {
            if (token == null)
                return false;

            if (token.Type == JTokenType.Object)
                return true;

            if (token.Type == JTokenType.String)
            {
                string s = token.ToString().Trim();
                return s.StartsWith("{") && s.EndsWith("}");
            }

            return false;
        }
        public void AppendColoredText(
            RichTextBox box,
            string text,
            Color? backColor = null,
            Color? foreColor = null,
            bool bold = false,
            bool italic = false,
            bool underline = false,
            float? fontSize = null
        )
        {
            int start = box.TextLength;
            box.AppendText(text);
            box.Select(start, text.Length);

            // Đặt màu nền
            if (backColor.HasValue)
                box.SelectionBackColor = backColor.Value;

            // Đặt màu chữ
            if (foreColor.HasValue)
                box.SelectionColor = foreColor.Value;

            // Xác định kiểu chữ
            FontStyle style = FontStyle.Regular;
            if (bold) style |= FontStyle.Bold;
            if (italic) style |= FontStyle.Italic;
            if (underline) style |= FontStyle.Underline;

            // Dùng font hiện tại, chỉ thay size nếu có
            float size = fontSize ?? box.Font.Size;

            box.SelectionFont = new Font(box.Font.FontFamily, size, style);

            // Bỏ chọn
            box.Select(box.TextLength, 0);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            getData();
        }
        public void checkUpdate()
        {
            try
            {
                #region Update chuong trinh moi (Neu co)
                //Đọc file PathUpdate.txt để lấy đường dẫn
                StreamReader strReader = new StreamReader(File.OpenRead(Directory.GetCurrentDirectory() + @"\PathUpdate.txt"));
                string pathsource = strReader.ReadLine();
                strReader.Close();
                string[] getIp = pathsource.Split('\\');
                if (PingToAddress(getIp[2]))
                {
                    string datemodifypathsource = "";
                    string datemodifypathdes = "";
                    //ktra datemodify chuong trinh tren server
                    DateTime modifysource = File.GetLastWriteTime(pathsource);
                    datemodifypathsource = modifysource.ToString();
                    //ktra chuong trinh tai may client
                    //Lay tên chuong trinh hien tai
                    string nameprogram = "";
                    DirectoryInfo diDirectoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
                    FileInfo[] fileInfo = diDirectoryInfo.GetFiles("*.exe");
                    //Duyet cac file
                    for (int i = 0; i < fileInfo.Length; i++)
                    {
                        if (fileInfo[i].Name.ToString() != "AutoUpdate.exe")
                        {
                            nameprogram = fileInfo[i].Name.ToString();
                            break;
                        }
                    }
                    DateTime modifydes = File.GetLastWriteTime(Directory.GetCurrentDirectory() + @"\" + nameprogram);
                    datemodifypathdes = modifydes.ToString();
                    if (modifysource > modifydes)//Neu datemodify khác nhau thì chay chuong trình copy file lên và thoát chuong trình
                    {
                        DialogResult dialogResult = MessageBox.Show("Đã có phiên bản mới, bạn có muốn cập nhật?", "Cập nhật", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (dialogResult == DialogResult.Yes)
                        {
                            ////chay chuong trình copy
                            System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + @"\" + "AutoUpdate.exe");
                            Application.Exit();
                            foreach (Process clsProcess in Process.GetProcesses())
                            {
                                if (clsProcess.ProcessName.Equals("asyncDataSoltec"))
                                {
                                    clsProcess.Kill();
                                    break;
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            catch
            {

            }
        }
        public static bool PingToAddress(string IP)
        {
            try
            {
                System.Net.NetworkInformation.Ping PingSender = new System.Net.NetworkInformation.Ping();
                int TimeOut = 120;
                string PingData = "aaaa";
                byte[] Buffer = System.Text.Encoding.ASCII.GetBytes(PingData);
                PingReply PingReply = PingSender.Send(IP, TimeOut, Buffer);
                if (PingReply.Status == IPStatus.Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch { return false; }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkUpdate();
        }
    }
    public class notify
    {
        public int JobId { get; set; }
        public string JobName { get; set; }
        public string App { get; set; }
        public string Code { get; set; }
        public string Link { get; set; }
        public int Status { get; set; }

        public override string ToString()
        {
            return $"[{JobId}] {JobName} - {Code} ({Status}) => {Link}";
        }
    }
}
