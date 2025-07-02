using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace asyncDataSoltec
{
    static class Program
    {
        // Mutex to allow only one instance of the application
        static Mutex mutex = new Mutex(true, "GetDataDap");
        static string Machine = System.Environment.MachineName;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Bắt sự kiện ngoại lệ không kiểm soát trong UI Thread
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
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
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            DataAccess ac = new DataAccess();
            DateTime now = DateTime.Now;
            string InsertData = string.Format("INSERT INTO  [STOCKMANAGEMENT].[dbo].[Log_Exception_Winform] ([app],[machine],[exception],[created_at]) VALUES ('{0}','{1}','{2}','{3}');", "GetDataDap", Machine, ex.StackTrace, now);
            ac.RunQuery(InsertData);
        }

    }
   
}
