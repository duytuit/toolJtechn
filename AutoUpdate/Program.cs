using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AutoUpdate
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var loadingForm = new LoadingForm();
            Thread updateThread = new Thread(() =>
            {
                string message = null;
                MessageBoxIcon icon = MessageBoxIcon.Information;

                try
                {
                    bool success = DoUpdate(args, out message, out icon);
                    if (!success && string.IsNullOrWhiteSpace(message))
                    {
                        message = "Cập nhật thất bại.";
                        icon = MessageBoxIcon.Error;
                    }
                }
                finally
                {
                    if (loadingForm.IsHandleCreated)
                    {
                        loadingForm.BeginInvoke(new Action(() =>
                        {
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                MessageBox.Show(loadingForm, message, "Cập nhật", MessageBoxButtons.OK, icon);
                            }

                            loadingForm.Close();
                        }));
                    }
                }
            });

            loadingForm.Shown += (s, e) => updateThread.Start();
            Application.Run(loadingForm);
        }

        private static bool DoUpdate(string[] args, out string message, out MessageBoxIcon icon)
        {
            message = null;
            icon = MessageBoxIcon.Information;

            try
            {
                string targetExecutable = GetArgumentValue(args, "--target-exe");
                if (string.IsNullOrWhiteSpace(targetExecutable) ||
                    targetExecutable.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                    targetExecutable.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
                    targetExecutable.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
                {
                    message = "Thiếu hoặc sai tên file ứng dụng cần cập nhật (--target-exe).";
                    icon = MessageBoxIcon.Warning;
                    return false;
                }

            WaitForProcessExit(args);

                string pathFile = Path.Combine(Application.StartupPath, "PathUpdate.txt");
                string sourceFolder = File.Exists(pathFile)
                    ? File.ReadAllText(pathFile).Trim()
                    : null;
                string targetFolder = Application.StartupPath;

                if (string.IsNullOrWhiteSpace(sourceFolder))
                {
                    message = "Không tìm thấy đường dẫn cập nhật trong PathUpdate.txt.";
                    icon = MessageBoxIcon.Warning;
                    return false;
                }

                if (!Directory.Exists(sourceFolder))
                {
                    message = "Thư mục cập nhật không tồn tại: " + sourceFolder;
                    icon = MessageBoxIcon.Warning;
                    return false;
                }

                foreach (var file in Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories))
                {
                    string relativePath = file.Substring(sourceFolder.Length).TrimStart('\\', '/');
                    string destFile = Path.Combine(targetFolder, relativePath);
                    string destDir = Path.GetDirectoryName(destFile);

                    if (!string.IsNullOrEmpty(destDir))
                        Directory.CreateDirectory(destDir);

                    string fileName = Path.GetFileName(file);
                    string updaterFileName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                    if (fileName.Equals(updaterFileName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    CopyWithRetry(file, destFile);
                }

                string appExe = Path.Combine(targetFolder, targetExecutable);
                if (File.Exists(appExe))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = appExe,
                        WorkingDirectory = targetFolder,
                        UseShellExecute = true
                    });
                    message = "Cập nhật thành công.";
                    icon = MessageBoxIcon.Information;
                    return true;
                }

                message = "Không tìm thấy " + targetExecutable + " sau khi cập nhật.";
                icon = MessageBoxIcon.Error;
                return false;
            }
            catch (Exception ex)
            {
                message = "Lỗi cập nhật: " + ex.Message;
                icon = MessageBoxIcon.Error;
                return false;
            }
        }

        private static string GetArgumentValue(string[] args, string argumentName)
        {
            if (args == null)
                return null;

            for (int index = 0; index < args.Length - 1; index++)
            {
                if (args[index].Equals(argumentName, StringComparison.OrdinalIgnoreCase))
                    return args[index + 1];
            }

            return null;
        }

        private static void WaitForProcessExit(string[] args)
        {
            if (args == null || args.Length < 2 || !args[0].Equals("--wait-for-pid", StringComparison.OrdinalIgnoreCase))
                return;

            int processId;
            if (!int.TryParse(args[1], out processId) || processId == Process.GetCurrentProcess().Id)
                return;

            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    if (!process.WaitForExit(30000))
                        throw new IOException("Ứng dụng đang chạy và chưa thể đóng để cập nhật.");
                }
            }
            catch (ArgumentException)
            {
                // The old application has already exited.
            }
        }

        private static void CopyWithRetry(string sourceFile, string destinationFile)
        {
            const int maxAttempts = 20;

            for (int attempt = 1; ; attempt++)
            {
                try
                {
                    File.Copy(sourceFile, destinationFile, true);
                    File.SetLastWriteTimeUtc(destinationFile, File.GetLastWriteTimeUtc(sourceFile));
                    return;
                }
                catch (IOException) when (attempt < maxAttempts)
                {
                    Thread.Sleep(500);
                }
                catch (UnauthorizedAccessException) when (attempt < maxAttempts)
                {
                    Thread.Sleep(500);
                }
            }
        }
    }

    internal sealed class LoadingForm : Form
    {
        private readonly Label _titleLabel;
        private readonly Label _statusLabel;
        private readonly System.Windows.Forms.Timer _timer;
        private readonly string[] _spinnerFrames = { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
        private int _spinnerIndex;

        public LoadingForm()
        {
            this.Text = "Cập nhật phần mềm";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(420, 150);
            this.BackColor = Color.FromArgb(245, 247, 250);
            this.ControlBox = false;
            this.TopMost = true;
            this.ShowInTaskbar = false;

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(24)
            };

            _titleLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 90),
                Text = "Đang cập nhật"
            };

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 13F, FontStyle.Regular),
                ForeColor = Color.FromArgb(75, 85, 99),
                BackColor = Color.Transparent,
                Text = "⠋ Đang tải phiên bản mới..."
            };

            panel.Controls.Add(_statusLabel);
            panel.Controls.Add(_titleLabel);
            this.Controls.Add(panel);

            _timer = new System.Windows.Forms.Timer
            {
                Interval = 120
            };
            _timer.Tick += (s, e) =>
            {
                _spinnerIndex = (_spinnerIndex + 1) % _spinnerFrames.Length;
                _statusLabel.Text = _spinnerFrames[_spinnerIndex] + " Đang tải phiên bản mới...";
            };
            _timer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            base.OnClosed(e);
        }
    }
}
