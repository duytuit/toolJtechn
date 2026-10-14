using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using JtecFactory.Helpers;
using JtecFactory.Navigation;
using JtecFactory.Services;

namespace JtecFactory.Forms
{
    public sealed partial class FrmLogin : Form
    {
        private readonly EmployeeService employeeService = new EmployeeService();

        public FrmLogin()
        {
            InitializeComponent();
            usernameTextBox.Text = Properties.Settings.Default.LastUsername;
            checkUpdate();
            loginButton.Click += OpenDashboard;
            usernameTextBox.KeyDown += UsernameTextBoxKeyDown;
            Resize += CenterLoginCard;
            CenterLoginCard(this, EventArgs.Empty);
           
        }

        private void CenterLoginCard(object sender, EventArgs e)
        {
            card.Left = (ClientSize.Width - card.Width) / 2;
            card.Top = (ClientSize.Height - card.Height) / 2;
        }

        private void UsernameTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            OpenDashboard(this, EventArgs.Empty);
        }

        private void OpenDashboard(object sender, EventArgs e)
        {
            string username = usernameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(this, "Vui lòng nhập tài khoản.",
                    "Không thể đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                usernameTextBox.Focus();
                return;
            }

            try
            {
                var employee = employeeService.GetByUsername(username);
                if (employee == null)
                {
                    MessageBox.Show(this, "Không tìm thấy tài khoản đang hoạt động. Kiểm tra lại tên tài khoản hoặc liên hệ quản trị viên.",
                        "Không thể đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    usernameTextBox.Focus();
                    usernameTextBox.SelectAll();
                    return;
                }

                Properties.Settings.Default.LastUsername = employee.Username;
                Properties.Settings.Default.Save();

                bool administratorMode = string.Equals(employee.Username, "admin", StringComparison.OrdinalIgnoreCase);
                Hide();
                bool loggedOut;
                using (FrmMain main = new FrmMain(employee.Id, administratorMode))
                {
                    main.ShowDialog();
                    loggedOut = main.LogoutRequested;
                }

                if (loggedOut)
                {
                    Show();
                    Activate();
                    usernameTextBox.Focus();
                    usernameTextBox.SelectAll();
                }
                else
                {
                    Close();
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Lỗi đăng nhập",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public void checkUpdate()
        {
            try
            {
                #region Update chuong trinh moi (Neu co)
                //Đọc đường dẫn cập nhật từ PathUpdate.txt cạnh file chạy chương trình
                string pathFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PathUpdate.txt");
                string pathsource = File.Exists(pathFile)
                    ? File.ReadAllText(pathFile).Trim()
                    : null;
                if (string.IsNullOrWhiteSpace(pathsource))
                    return;

                string[] getIp = pathsource.Split('\\');
                if (getIp.Length <= 2 || !PingToAddress(getIp[2]))
                    return;

                string sourceExe = Path.Combine(pathsource, "JtecFactory.exe");
                string localExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "JtecFactory.exe");
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
                            FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoUpdate.exe"),
                            Arguments = "--wait-for-pid " + currentProcess.Id + " --target-exe JtecFactory.exe",
                            UseShellExecute = true
                        });
                        Application.Exit();
                        currentProcess.Kill();
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

    }
}