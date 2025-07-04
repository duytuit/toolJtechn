using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TaskService
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;
        private ImapClient client;
        private static readonly HttpClient __client = new HttpClient(); // dùng 1 lần cho cả app
        private readonly string imapServer = "jtechanoi.com.vn";
        private readonly int imapPort = 993;
        private readonly string username = "cam-002@jtechanoi.com.vn";
        private readonly string password = "l891jc%5U";

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                client = new ImapClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(imapServer, imapPort, SecureSocketOptions.SslOnConnect);
                client.Authenticate(username, password);

                WriteLog("Đã kết nối IMAP.");

                timer = new Timer(5 * 60 * 1000); // 5 phút
                timer.Elapsed += Timer_Elapsed;
                timer.AutoReset = true;
                timer.Start();

                WriteLog("Service started.");
            }
            catch (Exception ex)
            {
                WriteLog("Lỗi khởi tạo: " + ex.Message);
            }
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var now = DateTime.Now.TimeOfDay;
            var start = new TimeSpan(7, 0, 0);  // 07:00
            var end = new TimeSpan(21, 0, 0);   // 21:00

            if (now >= start && now <= end)
            {
                try
                {
                    await ReadEmail();
                }
                catch (Exception ex)
                {
                    WriteLog("Lỗi xử lý: " + ex.Message);
                }
            }
            else
            {
                WriteLog($"Ngoài giờ làm việc: {now}. Không xử lý.");
            }
        }

        private async Task ReadEmail()
        {
            if (client?.IsConnected != true)
            {
                WriteLog("Client mất kết nối, đang reconnect...");
                client?.Connect(imapServer, imapPort, SecureSocketOptions.SslOnConnect);
                client?.Authenticate(username, password);
            }

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);
            var today = DateTime.Today;

            for (int i = inbox.Count - 1; i >= Math.Max(inbox.Count - 50, 0); i--)
            {
                var message = inbox.GetMessage(i);
                if (message.Date.Date != today) continue;

                if (!message.From.ToString().ToLower().Contains("qlsx")) continue;

               // var subject = message.Subject?.Trim();
                //if (!string.IsNullOrEmpty(subject) &&
                //    (subject.StartsWith("Fwd:", StringComparison.OrdinalIgnoreCase) ||
                //     subject.StartsWith("FW:", StringComparison.OrdinalIgnoreCase) ||
                //     subject.StartsWith("RE:", StringComparison.OrdinalIgnoreCase))) continue;
                // Đọc đính kèm
                List<string> path_files = new List<string>();
                string content = null;
                foreach (var attachment in message.Attachments)
                {
                    var rawFileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;
                    var directoryPath = @"D:\jtecdata\JTEC_PD_PROGAM\CMSWeb\jtecweb\public\public\assets\files";
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }
                    if (attachment is MimePart part)
                    {
                        using (var stream = new MemoryStream())
                        {
                            part.Content.DecodeTo(stream);
                            var savePath = Path.Combine(directoryPath, rawFileName);
                            File.WriteAllBytes(savePath, stream.ToArray());
                            path_files.Add(rawFileName);
                            WriteLog($"Đã lưu file đính kèm: {rawFileName}");
                        }
                    }
                    //if (!string.IsNullOrEmpty(message.TextBody))
                    //{
                     //content = message.TextBody;
                    //}
                   
                    //if (!string.IsNullOrEmpty(message.HtmlBody))
                    //{}
                }
                content = message.HtmlBody;
                string attachValue = path_files.Count > 0 ? JsonConvert.SerializeObject(path_files) : null;

                var form = new MultipartFormDataContent();
                // Giả sử bạn đang gửi 2 dữ liệu: taskName và nội dung HTML lớn
                form.Add(new StringContent(content?? ""), "Content");
                form.Add(new StringContent(message.From.ToString()), "Code");
                form.Add(new StringContent(message.Subject), "Title");
                form.Add(new StringContent(attachValue ?? ""), "Attach");
                form.Add(new StringContent(message.Date.ToString("yyyy-MM-dd HH:mm:ss")), "Created_client");

                try
                {
                    var response = await __client.PostAsync("http://192.168.207.6:8080/Required/task/create", form);
                    string result = await response.Content.ReadAsStringAsync();
                    if (!response.IsSuccessStatusCode)
                    {
                        WriteLog($"Phản hồi từ server: {result}{JsonConvert.SerializeObject(form)}");
                    }
                }
                catch (Exception ex)
                {
                    WriteLog("Lỗi khi gửi API: " + ex.ToString() + JsonConvert.SerializeObject(form));
                }
                WriteLog($"Email từ {message.From} - Chủ đề: {message.Subject}");
            }
        }

        protected override void OnStop()
        {
            try
            {
                timer?.Stop();
                timer?.Dispose();

                if (client?.IsConnected == true)
                {
                    client.Disconnect(true);
                    WriteLog("IMAP client disconnected.");
                }

                client?.Dispose();
                WriteLog("Service stopped.");
            }
            catch (Exception ex)
            {
                WriteLog("Lỗi khi dừng service: " + ex.Message);
            }
        }

        private void WriteLog(string message)
        {
            string logPath = AppDomain.CurrentDomain.BaseDirectory + @"\Logs";
            Directory.CreateDirectory(logPath);
            File.AppendAllText(Path.Combine(logPath, $"log-{DateTime.Today:yyyy-MM-dd}.txt"),
                $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }
    }
}
