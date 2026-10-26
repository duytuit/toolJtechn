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
        private ContextMenuStrip contextMenu;
        private bool _isLoadingData;
        private readonly SemaphoreSlim _socketMessageLock = new SemaphoreSlim(1, 1);
        private volatile string _employeeCode;
        public Form1()
        {
            InitializeComponent();
            richTextBox1.DetectUrls = true;
            richTextBox1.LinkClicked += RichTextBox1_LinkClicked;
            string code = Environment.GetEnvironmentVariable("MY_APP_CODENV", EnvironmentVariableTarget.User);
            txtCodeNV.Text = code;
            _employeeCode = code;
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
            CreateNotifyIcon();
            FileLogger.CleanupOldLogs();
        }
        private async void connectSocket()
        {

            _cts = new CancellationTokenSource();
            _client = new WebSocketClient("wss://192.168.207.6:5007/ws");

            //_client.OnLog += (msg) => Invoke((Action)(() => richTextBox1.AppendText(msg + "\n")));
            _client.OnMessageReceived += HandleSocketMessage;
            _client.OnConnected += () => _ = SendSocketSubscriptionAsync(_client, _cts.Token);
            // _client.OnConnected += () => Invoke((Action)(() => labelStatus.Text = "🟢 Connected"));
            // _client.OnDisconnected += () => Invoke((Action)(() => labelStatus.Text = "🔴 Disconnected"));
            _client.OnLog += (log) =>
            {
                FileLogger.Log(log);
            };
            await _client.StartAsync(_cts.Token);
        }

        private async Task SendSocketSubscriptionAsync(WebSocketClient client, CancellationToken token)
        {
            var obj = new
            {
                Event = 15,
                Chanel = "dencanhbao_cd_dap",
                MessageText = ""
            };
            try
            {
                string jsonData = JsonConvert.SerializeObject(obj);
                await client.SendMessageAsync(jsonData, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested) { }
            catch (Exception ex)
            {
                FileLogger.Log("Failed to subscribe to notification channel: " + ex);
            }
        }

        private async void HandleSocketMessage(string message)
        {
            await _socketMessageLock.WaitAsync();
            try
            {
                var data = JsonConvert.DeserializeObject<JObject>(message);
                JToken messageToken = data?["messageText"];
                if (!IsJsonObject(messageToken))
                    return;

                var messageObject = messageToken.Type == JTokenType.Object
                    ? (JObject)messageToken
                    : JsonConvert.DeserializeObject<JObject>(messageToken.ToString());
                if (messageObject == null ||
                    !int.TryParse(messageObject["job_id"]?.ToString(), out int jobId) ||
                    !int.TryParse(messageObject["status"]?.ToString(), out int status))
                {
                    throw new FormatException("Gói tin thiếu job_id hoặc status hợp lệ.");
                }

                var notification = new notify
                {
                    JobId = jobId,
                    JobName = messageObject["job_name"]?.ToString(),
                    App = messageObject["app"]?.ToString(),
                    Code = messageObject["code"]?.ToString(),
                    Link = messageObject["link"]?.ToString(),
                    Status = status
                };

                bool isClearAll = status == 3;
                if (!isClearAll && notification.Code != _employeeCode)
                    return;

                await Task.Run(() =>
                {
                    using (var db = new clsKetNoi())
                    {
                        if (status == 1)
                        {
                            db.DeleteWhere("Notifycation", new Dictionary<string, object>
                            {
                                ["job_id"] = notification.JobId,
                                ["app"] = notification.App,
                                ["code"] = notification.Code
                            });
                        }
                        else if (isClearAll)
                        {
                            db.DeleteWhere("Notifycation", new Dictionary<string, object>
                            {
                                ["job_id"] = notification.JobId,
                                ["app"] = notification.App
                            });
                        }
                        else
                        {
                            const string sql = @"
IF EXISTS (
    SELECT 1 FROM Notifycation
    WHERE job_id = @job_id AND app = @app AND code = @code
)
    UPDATE Notifycation
    SET job_name = @job_name, link = @link, status = @status
    WHERE job_id = @job_id AND app = @app AND code = @code
ELSE
    INSERT INTO Notifycation (job_id, job_name, app, code, link, status)
    VALUES (@job_id, @job_name, @app, @code, @link, @status)";

                            db.UpdateTable(
                                sql,
                                new[] { "@job_id", "@job_name", "@app", "@code", "@link", "@status" },
                                new object[]
                                {
                                    notification.JobId,
                                    (object)notification.JobName ?? DBNull.Value,
                                    (object)notification.App ?? DBNull.Value,
                                    (object)notification.Code ?? DBNull.Value,
                                    (object)notification.Link ?? DBNull.Value,
                                    notification.Status
                                },
                                6);
                        }
                    }
                });

                if (IsDisposed || !IsHandleCreated)
                    return;

                BeginInvoke((Action)(() =>
                {
                    if (status == 1)
                    {
                        _notify.RemoveAll(x =>
                            x.JobId == notification.JobId &&
                            x.App == notification.App &&
                            x.Code == notification.Code);
                    }
                    else if (isClearAll)
                    {
                        _notify.RemoveAll(x =>
                            x.JobId == notification.JobId &&
                            x.App == notification.App);
                    }
                    else
                    {
                        var existing = _notify.FirstOrDefault(x =>
                            x.JobId == notification.JobId &&
                            x.App == notification.App &&
                            x.Code == notification.Code);

                        if (existing == null)
                            _notify.Add(notification);
                        else
                        {
                            existing.JobName = notification.JobName;
                            existing.Link = notification.Link;
                            existing.Status = notification.Status;
                        }
                    }

                    RenderNotifications();
                }));
            }
            catch (Exception ex)
            {
                FileLogger.Log($"Socket message was not applied to database: {ex}");
                if (!IsDisposed && IsHandleCreated)
                {
                    BeginInvoke((Action)(() =>
                        richTextBox1.AppendText($"[Lỗi xử lý thông báo socket/DB] {ex.Message}\n")));
                }
            }
            finally
            {
                _socketMessageLock.Release();
            }
        }

        private void RenderNotifications()
        {
            richTextBox1.Clear();
            foreach (var item in _notify)
            {
                AppendColoredText(
                    richTextBox1,
                    $"Công việc - {item.JobName}:\n",
                    Color.Yellow,
                    Color.Black,
                    bold: true);
                AppendColoredText(richTextBox1, $"{item.Link}\n");
            }

            DrawCircleAbovePicture(pic, _notify.Count, 405, 18);
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
        private async Task LoadDataAsync()
        {
            if (_isLoadingData)
                return;

            _isLoadingData = true;
            button1.Enabled = false;

            try
            {
                string code = txtCodeNV.Text;
                List<notify> loadedNotifications = await Task.Run(() =>
                {
                    using (var db = new clsKetNoi())
                    {
                        string sql = $@"SELECT job_id, job_name, app, code, link, status 
                                FROM Notifycation
                                WHERE code = @code";

                        DataTable table = db.LoadTable(
                            sql,
                            new[] { "@code" },
                            new object[] { code },
                            1);

                        return table.AsEnumerable()
                            .Select(row => new notify
                            {
                                JobId = Convert.ToInt32(row["job_id"]),
                                JobName = row["job_name"].ToString(),
                                App = row["app"].ToString(),
                                Code = row["code"].ToString(),
                                Link = row["link"].ToString(),
                                Status = Convert.ToInt32(row["status"])
                            })
                            .ToList();
                    }
                });

                richTextBox1.Clear();
                _notify.Clear();
                _notify.AddRange(loadedNotifications);

                if (circle != null)
                {
                    circle.Visible = false;
                }

                const int batchSize = 25;
                for (int start = 0; start < loadedNotifications.Count; start += batchSize)
                {
                    int end = Math.Min(start + batchSize, loadedNotifications.Count);
                    for (int i = start; i < end; i++)
                    {
                        var notification = loadedNotifications[i];
                        if (!_notify.Contains(notification))
                            continue;

                        string title = $"Công việc - {notification.JobName}:\n";
                        AppendColoredText(richTextBox1, title, Color.Yellow, Color.Black, true);
                        AppendColoredText(richTextBox1, notification.Link + "\n");
                    }

                    if (end < loadedNotifications.Count)
                        await Task.Yield();
                }

                DrawCircleAbovePicture(pic, _notify.Count, 405, 18);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi LoadDataAsync: " + ex.Message);
            }
            finally
            {
                _isLoadingData = false;
                button1.Enabled = true;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Environment.SetEnvironmentVariable("MY_APP_CODENV", txtCodeNV.Text, EnvironmentVariableTarget.User);
            _employeeCode = txtCodeNV.Text;
            _ = LoadDataAsync();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //await LoadDataAsync();
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
        private void CreateNotifyIcon()
        {
            // Tạo menu chuột phải
            contextMenu = new ContextMenuStrip();
            var menuOpen = new ToolStripMenuItem("Mở lại");
            var menuClose = new ToolStripMenuItem("Thoát");

            menuOpen.Click += (s, e) =>
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
            };
            menuClose.Click += (s, e) =>
            {
                notifyIcon1.Visible = false;
                Application.Exit();
            };

            contextMenu.Items.Add(menuOpen);
            contextMenu.Items.Add(menuClose);

            notifyIcon1.ContextMenuStrip = contextMenu;
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

            using (var font = new Font(box.Font.FontFamily, size, style))
            {
                box.SelectionFont = font;
            }

            // Bỏ chọn
            box.Select(box.TextLength, 0);
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
            _ = LoadDataAsync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;  // Ngăn không cho form đóng
                this.Hide();      // Ẩn form (chạy ngầm)
                notifyIcon1.Visible = true;
            }
            base.OnFormClosing(e);
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
