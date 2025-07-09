using System;
using System.Threading;
using System.Windows.Forms;

namespace toolJtechn
{
    internal static class Program
    {
        // Mutex to allow only one instance of the application
        private static Mutex mutex = new Mutex(true, "GetDataDap");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Chương trình đang chạy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            // Release the mutex
            mutex.ReleaseMutex();
        }
    }
}