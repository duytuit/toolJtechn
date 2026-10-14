using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JtecFactory
{
    static class Program
    {
        // Mutex to allow only one instance of the application
        static Mutex mutex = new Mutex(true, "LogAppJtecHn");
        static string Machine = System.Environment.MachineName;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Bắt sự kiện ngoại lệ không kiểm soát trong UI Thread
            // Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Chương trình đang chạy!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new JtecFactory.Forms.FrmLogin());
            // Release the mutex
            mutex.ReleaseMutex();
        }

    }
}
