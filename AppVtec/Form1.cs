using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace AppVtec
{
    public partial class Form1 : Form
    {
        //test
        private const string PdfTable = "upload_data";
        private const string ScanCodeColumn = "code";
        private const string PdfUrlColumn = "url";
        private const string PdfBasePathEnvironmentVariable = "APPVTEC_PDF_BASE_PATH";

        public Form1()
        {
            checkUpdate();
            InitializeComponent();

            var savedBasePath = Environment.GetEnvironmentVariable(
                PdfBasePathEnvironmentVariable, EnvironmentVariableTarget.User);
            basePathTextBox.Text = string.IsNullOrWhiteSpace(savedBasePath)
                ? ConfigurationManager.AppSettings["PathDefault"] ?? string.Empty
                : savedBasePath;
            SetDatabaseStatus("Đang kiểm tra kết nối MySQL...", Color.DimGray);
        }

        private async void Form1_Shown(object sender, EventArgs e)
        {
            await UpdateDatabaseStatusAsync();
            scanTextBox.Focus();
        }

        private async Task UpdateDatabaseStatusAsync()
        {
            try
            {
                using (var connection = new MySqlConnection(GetMySqlConnectionString()))
                {
                    await connection.OpenAsync();
                }

                SetDatabaseStatus("Kết nối MySQL thành công.", Color.ForestGreen);
            }
            catch (ConfigurationErrorsException ex)
            {
                SetDatabaseStatus("Cấu hình MySQL không hợp lệ: " + ex.Message, Color.Firebrick);
            }
            catch (InvalidOperationException ex)
            {
                SetDatabaseStatus("Chưa cấu hình MySQL: " + ex.Message, Color.DarkOrange);
            }
            catch (MySqlException ex)
            {
                SetDatabaseStatus("Kết nối MySQL thất bại: " + ex.Message, Color.Firebrick);
            }
        }

        private async void ScanTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            string scanCode;
            if (!TryExtractScanCode(scanTextBox.Text, out scanCode))
            {
                messageLabel.Text = "Mã QR không đúng định dạng hoặc thiếu mã ở trường thứ ba.";
                scanTextBox.Clear();
                scanTextBox.Focus();
                return;
            }

            scanTextBox.Enabled = false;
            messageLabel.Text = "Đang tra cứu mã " + scanCode + "...";
            try
            {
                var pdfPath = await FindPdfPathAsync(scanCode);
                if (pdfPath == null)
                {
                    messageLabel.Text = "Không tìm thấy mã scan trong cơ sở dữ liệu.";
                    return;
                }

                pdfPathTextBox.Text = pdfPath;
                OpenPdf();
            }
            catch (ConfigurationErrorsException ex)
            {
                messageLabel.Text = "Cấu hình MySQL không hợp lệ: " + ex.Message;
            }
            catch (InvalidOperationException ex)
            {
                messageLabel.Text = ex.Message;
            }
            catch (MySqlException ex)
            {
                SetDatabaseStatus("Lỗi truy vấn MySQL: " + ex.Message, Color.Firebrick);
                messageLabel.Text = "Không thể tra cứu mã scan.";
            }
            finally
            {
                scanTextBox.Enabled = true;
                scanTextBox.Clear();
                scanTextBox.Focus();
            }
        }

        private static bool TryExtractScanCode(string scannedQr, out string scanCode)
        {
            scanCode = null;
            if (string.IsNullOrWhiteSpace(scannedQr))
                return false;

            var fields = scannedQr.Split(',');
            if (fields.Length < 3)
                return false;

            scanCode = fields[2].Trim();
            return !string.IsNullOrWhiteSpace(scanCode);
        }

        private async Task<string> FindPdfPathAsync(string scanCode)
        {
            var sql = "SELECT `" + PdfUrlColumn + "` FROM `" + PdfTable + "` WHERE `" +
                ScanCodeColumn + "` = @scanCode LIMIT 2";

            using (var connection = new MySqlConnection(GetMySqlConnectionString()))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@scanCode", scanCode);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                            return null;

                        if (reader.IsDBNull(0))
                            throw new InvalidOperationException("Bản ghi tìm thấy nhưng chưa có đường dẫn PDF.");

                        var pdfPath = Convert.ToString(reader.GetValue(0));
                        if (await reader.ReadAsync())
                            throw new InvalidOperationException("Mã scan khớp nhiều bản ghi. Vui lòng kiểm tra dữ liệu DB.");

                        return pdfPath;
                    }
                }
            }
        }

        private void OpenPdfButton_Click(object sender, EventArgs e)
        {
            OpenPdf();
        }

        private void SaveBasePathButton_Click(object sender, EventArgs e)
        {
            var path = basePathTextBox.Text.Trim();
            var value = string.IsNullOrWhiteSpace(path) ? null : path;

            try
            {
                Environment.SetEnvironmentVariable(
                    PdfBasePathEnvironmentVariable, value, EnvironmentVariableTarget.User);
                Environment.SetEnvironmentVariable(
                    PdfBasePathEnvironmentVariable, value, EnvironmentVariableTarget.Process);
                messageLabel.Text = value == null
                    ? "Đã xóa đường dẫn PDF đã lưu."
                    : "Đã lưu đường dẫn PDF vào biến môi trường của tài khoản Windows.";
            }
            catch (ArgumentException ex)
            {
                messageLabel.Text = "Đường dẫn không thể lưu: " + ex.Message;
            }
            catch (System.Security.SecurityException ex)
            {
                messageLabel.Text = "Không có quyền lưu biến môi trường: " + ex.Message;
            }
        }

        private void OpenPdf()
        {
            var pdfLocation = pdfPathTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(pdfLocation))
            {
                messageLabel.Text = "Chưa có đường dẫn PDF để mở.";
                return;
            }

            string pdfUri;
            var basePath = basePathTextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(basePath))
            {
                if (!Directory.Exists(basePath))
                {
                    messageLabel.Text = "Thư mục PDF cấu hình không tồn tại.";
                    return;
                }

                var fileName = GetPdfFileName(pdfLocation);
                if (string.IsNullOrWhiteSpace(fileName) ||
                    !string.Equals(Path.GetExtension(fileName), ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    messageLabel.Text = "Không lấy được tên file PDF từ trường url.";
                    return;
                }

                var combinedPath = Path.Combine(basePath, fileName);
                if (!File.Exists(combinedPath))
                {
                    messageLabel.Text = "Không tìm thấy file trong thư mục PDF cấu hình.";
                    return;
                }

                pdfUri = new Uri(Path.GetFullPath(combinedPath)).AbsoluteUri;
            }
            else
            {
                Uri absoluteUri;
                if (Uri.TryCreate(pdfLocation, UriKind.Absolute, out absoluteUri) &&
                    (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
                {
                    if (!string.Equals(Path.GetExtension(absoluteUri.AbsolutePath), ".pdf",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        messageLabel.Text = "URL được tra cứu không trỏ đến file PDF.";
                        return;
                    }

                    pdfUri = absoluteUri.AbsoluteUri;
                }
                else
                {
                    if (!File.Exists(pdfLocation))
                    {
                        messageLabel.Text = "Không tìm thấy file PDF theo đường dẫn đã tra cứu.";
                        return;
                    }

                    if (!string.Equals(Path.GetExtension(pdfLocation), ".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        messageLabel.Text = "Đường dẫn được tra cứu không trỏ đến file PDF.";
                        return;
                    }

                    pdfUri = new Uri(Path.GetFullPath(pdfLocation)).AbsoluteUri;
                }
            }

            try
            {
                var viewerForm = new PdfViewerForm(pdfUri, GetSelectedScreenCount());
                viewerForm.Show(this);
                messageLabel.Text = "Đã mở PDF trong form mới.";
            }
            catch (InvalidOperationException ex)
            {
                messageLabel.Text = "Không thể mở form PDF: " + ex.Message;
            }
        }

        private int GetSelectedScreenCount()
        {
            if (threeScreensRadioButton.Checked)
                return 3;

            return twoScreensRadioButton.Checked ? 2 : 1;
        }

        private void SetDatabaseStatus(string text, Color color)
        {
            databaseStatusLabel.Text = "Trạng thái DB: " + text;
            databaseStatusLabel.ForeColor = color;
        }

        private string GetMySqlConnectionString()
        {
            var settings = ConfigurationManager.ConnectionStrings["VtecMySql"];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ConfigurationErrorsException("Không tìm thấy connection string VtecMySql trong App.config.");

            var password = ConfigurationManager.AppSettings["VtecMySqlPasswordProtected"];
            if (string.IsNullOrWhiteSpace(password))
                throw new ConfigurationErrorsException("Thiếu mật khẩu VtecMySqlPasswordProtected trong App.config.");

            var builder = new MySqlConnectionStringBuilder(settings.ConnectionString)
            {
                Password = password,
                ConnectionTimeout = 10
            };
            return builder.ConnectionString;
        }
          public void checkUpdate()
        {
            try
            {
                #region Update chuong trinh moi (Neu co)
                //Đọc đường dẫn cập nhật từ PathUpdate.txt cạnh file chạy chương trình
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string pathFile = Path.Combine(appDirectory, "PathUpdate.txt");
                string pathsource = File.Exists(pathFile)
                    ? File.ReadAllText(pathFile).Trim()
                    : null;
                if (string.IsNullOrWhiteSpace(pathsource))
                    return;

                const string targetExecutable = "AppVtec.exe";
                string sourceExe = Directory.Exists(pathsource)
                    ? Path.Combine(pathsource, targetExecutable)
                    : pathsource;
                string localExe = Path.Combine(appDirectory, "AppVtec.exe");
                if (!Path.GetFileName(sourceExe).Equals(targetExecutable, StringComparison.OrdinalIgnoreCase))
                    return;

                if (!File.Exists(sourceExe) || !File.Exists(localExe))
                    return;

                DateTime sourceLastWriteTime = File.GetLastWriteTimeUtc(sourceExe);
                DateTime localLastWriteTime = File.GetLastWriteTimeUtc(localExe);
                if (sourceLastWriteTime > localLastWriteTime)
                {
                    DialogResult dialogResult = MessageBox.Show("Đã có phiên bản mới, bạn có muốn cập nhật?", "Cập nhật", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                    {
                        Process currentProcess = Process.GetCurrentProcess();
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = Path.Combine(appDirectory, "AutoUpdate.exe"),
                            Arguments = "--wait-for-pid " + currentProcess.Id + " --target-exe " + targetExecutable,
                            UseShellExecute = true
                        });
                        Application.Exit();
                        currentProcess.Kill();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kiểm tra cập nhật: " + ex.Message, "Cập nhật", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private static string GetPdfFileName(string pdfLocation)
        {
            Uri absoluteUri;
            if (Uri.TryCreate(pdfLocation, UriKind.Absolute, out absoluteUri) &&
                (absoluteUri.Scheme == Uri.UriSchemeHttp || absoluteUri.Scheme == Uri.UriSchemeHttps))
            {
                return Path.GetFileName(Uri.UnescapeDataString(absoluteUri.AbsolutePath));
            }

            return Path.GetFileName(pdfLocation);
        }
    }
}