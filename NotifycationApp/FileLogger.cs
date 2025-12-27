using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifycationApp
{
    public static class FileLogger
    {
        private static readonly object _lock = new object();

        private static readonly string _logDir =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        public static void Log(string message)
        {
            try
            {
                if (!Directory.Exists(_logDir))
                    Directory.CreateDirectory(_logDir);

                string filePath = Path.Combine(
                    _logDir,
                    $"log-{DateTime.Now:yyyy-MM-dd}.txt"
                );

                string logLine =
                    $"{DateTime.Now:HH:mm:ss} {message}{Environment.NewLine}";

                lock (_lock)
                {
                    File.AppendAllText(filePath, logLine, Encoding.UTF8);
                }
            }
            catch
            {
                // ❗ không throw để tránh crash app
            }
        }
        public static void CleanupOldLogs(int days = 7)
        {
            if (!Directory.Exists(_logDir)) return;

            foreach (var file in Directory.GetFiles(_logDir, "log-*.txt"))
            {
                if (File.GetCreationTime(file) < DateTime.Now.AddDays(-days))
                    File.Delete(file);
            }
        }
    }

}
