using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace AppVtec
{
    internal sealed class PdfViewerForm : Form
    {
        private const uint SetWindowPosNoZOrder = 0x0004;
        private const uint SetWindowPosNoActivate = 0x0010;
        private const uint SetWindowPosFrameChanged = 0x0020;

        private readonly string pdfUri;
        private readonly Rectangle targetBounds;
        private readonly WebView2 webView = new WebView2();
        private readonly Label statusLabel = new Label();

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(
            IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int width, int height, uint flags);

        public PdfViewerForm(string pdfUri, int screenCount)
        {
            if (string.IsNullOrWhiteSpace(pdfUri))
                throw new ArgumentException("Đường dẫn PDF không được để trống.", nameof(pdfUri));
            if (screenCount < 1 || screenCount > 3)
                throw new ArgumentOutOfRangeException(nameof(screenCount));

            this.pdfUri = pdfUri;
            targetBounds = GetCombinedScreenBounds(screenCount);
            Text = "Xem PDF";
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.Manual;
            Bounds = targetBounds;

            statusLabel.Dock = DockStyle.Top;
            statusLabel.Height = 30;
            statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            statusLabel.Padding = new Padding(10, 0, 0, 0);
            statusLabel.Text = "Đang khởi tạo trình xem PDF...";

            webView.Dock = DockStyle.Fill;
            webView.NavigationCompleted += WebView_NavigationCompleted;

            Controls.Add(webView);
            Controls.Add(statusLabel);
            Shown += PdfViewerForm_Shown;
        }

        private async void PdfViewerForm_Shown(object sender, EventArgs e)
        {
            if (!SetWindowPos(
                Handle,
                IntPtr.Zero,
                targetBounds.Left,
                targetBounds.Top,
                targetBounds.Width,
                targetBounds.Height,
                SetWindowPosNoZOrder | SetWindowPosNoActivate | SetWindowPosFrameChanged))
            {
                var error = new Win32Exception(Marshal.GetLastWin32Error());
                ShowViewerError("Không thể phủ vùng màn hình đã chọn: " + error.Message);
                return;
            }

            try
            {
                await InitializeViewerAsync();
                var viewerUri = new UriBuilder(pdfUri)
                {
                    Fragment = "toolbar=0&view=FitH"
                }.Uri;
                webView.CoreWebView2.Navigate(viewerUri.AbsoluteUri);
                statusLabel.Text = "Đang tải PDF...";
            }
            catch (WebView2RuntimeNotFoundException ex)
            {
                ShowViewerError("Thiếu Microsoft Edge WebView2 Runtime: " + ex.Message);
            }
            catch (BadImageFormatException ex)
            {
                ShowViewerError("Kiến trúc WebView2 không khớp với ứng dụng. Hãy build lại cấu hình x64: " + ex.Message);
            }
            catch (COMException ex)
            {
                ShowViewerError("Không khởi tạo được trình xem PDF: " + ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                ShowViewerError("Không khởi tạo được trình xem PDF: " + ex.Message);
            }
        }

        private async Task InitializeViewerAsync()
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.Settings.HiddenPdfToolbarItems =
                CoreWebView2PdfToolbarItems.Save |
                CoreWebView2PdfToolbarItems.Print |
                CoreWebView2PdfToolbarItems.SaveAs |
                CoreWebView2PdfToolbarItems.ZoomIn |
                CoreWebView2PdfToolbarItems.ZoomOut |
                CoreWebView2PdfToolbarItems.Rotate |
                CoreWebView2PdfToolbarItems.FitPage |
                CoreWebView2PdfToolbarItems.PageLayout |
                CoreWebView2PdfToolbarItems.Bookmarks |
                CoreWebView2PdfToolbarItems.PageSelector |
                CoreWebView2PdfToolbarItems.Search |
                CoreWebView2PdfToolbarItems.FullScreen |
                CoreWebView2PdfToolbarItems.MoreSettings;
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                statusLabel.Visible = false;
                return;
            }

            ShowViewerError("Không thể tải PDF trong form.");
        }

        private void ShowViewerError(string message)
        {
            statusLabel.ForeColor = Color.Firebrick;
            statusLabel.Text = message;
        }

        private static Rectangle GetCombinedScreenBounds(int screenCount)
        {
            var screens = Screen.AllScreens;
            if (screens.Length < screenCount)
                throw new InvalidOperationException(
                    "Không đủ màn hình cho chế độ đã chọn. Hiện tìm thấy " +
                    screens.Length + " màn hình.");

            var externalScreens = screens
                .Where(screen => !screen.Primary)
                .OrderBy(screen => screen.Bounds.Left)
                .ThenBy(screen => screen.Bounds.Top)
                .ToArray();
            var targetScreens = (externalScreens.Length >= screenCount
                    ? externalScreens
                    : screens.OrderBy(screen => screen.Bounds.Left)
                        .ThenBy(screen => screen.Bounds.Top)
                        .ToArray())
                .Take(screenCount)
                .ToArray();

            var bounds = targetScreens[0].Bounds;
            for (int i = 1; i < targetScreens.Length; i++)
                bounds = Rectangle.Union(bounds, targetScreens[i].Bounds);

            return bounds;
        }
    }
}
