using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Threading.Tasks;
using ThreadingTimer = System.Threading.Timer;
using Timeout = System.Threading.Timeout;
using Newtonsoft.Json;

namespace hotfixapp
{
    public partial class Form1 : Form
    {
        private Dictionary<string, DateTime> fileWriteTimes = new Dictionary<string, DateTime>();
        private System.Windows.Forms.Timer pollingTimer;
        private ThreadingTimer debounceTimer;
        private bool isRestarting = false;
        private DateTime lastBuildTime = DateTime.MinValue;
        private string configFilePath = Path.Combine(Application.StartupPath, "config.json");

        public Form1()
        {
            InitializeComponent();
            LoadConfig();
            //abc
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string folder = txtProjectPath.Text.Trim();
            if (!Directory.Exists(folder))
            {
                Log("❌ Invalid folder.");
                return;
            }

            SaveConfig();
            StartPolling(folder);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            pollingTimer?.Stop();
            pollingTimer = null;
            Log("🛑 Polling stopped.");
        }

        private void StartPolling(string folder)
        {
            pollingTimer?.Stop();
            pollingTimer = new System.Windows.Forms.Timer
            {
                Interval = 1000
            };

            pollingTimer.Tick += (s, e) =>
            {
                try
                {
                    var files = Directory.GetFiles(folder, "*.cs", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var lastWrite = File.GetLastWriteTime(file);
                        if (!fileWriteTimes.TryGetValue(file, out var prevWrite) || lastWrite > prevWrite)
                        {
                            fileWriteTimes[file] = lastWrite;
                            if (lastWrite > lastBuildTime)
                            {
                                Log($"📝 File changed: {file}");
                                ScheduleRestart();
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("❗ Polling error: " + ex.Message);
                }
            };

            pollingTimer.Start();
            Log("📡 Polling started.");
        }

        private void ScheduleRestart()
        {
            Log("⏳ Scheduling restart...");
            debounceTimer?.Dispose();
            debounceTimer = new ThreadingTimer(_ =>
            {
                if (isRestarting) return;
                isRestarting = true;

                Task.Run(() =>
                {
                    RestartApp();
                    isRestarting = false;
                });

            }, null, 500, Timeout.Infinite);
        }

        private void RestartApp()
        {
            string projectFile = txtProjectFile.Text.Trim();
            string exePath = txtExePath.Text.Trim();
            string processName = txtProcessName.Text.Trim();
            string msbuildPath = txtMsbuildPath.Text.Trim();

            try
            {
                foreach (var p in Process.GetProcessesByName(processName))
                {
                    SafeLog("❌ Killing existing process...");
                    p.Kill();
                    p.WaitForExit();
                }

                SafeLog("🔨 Building project...");
                var build = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = msbuildPath,
                        Arguments = $"\"{projectFile}\" /p:Configuration=Debug /t:Build",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                build.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) SafeLog(e.Data); };

                build.Start();
                build.BeginOutputReadLine();
                build.WaitForExit();

                if (build.ExitCode == 0 && File.Exists(exePath))
                {
                    SafeLog("🚀 Restarting app...");
                    lastBuildTime = DateTime.Now;
                    Process.Start(exePath);
                }
                else
                {
                    SafeLog("⚠️ Build failed or EXE not found.");
                }
            }
            catch (Exception ex)
            {
                SafeLog("❗ Restart error: " + ex.Message);
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (File.Exists(configFilePath))
                {
                    var json = File.ReadAllText(configFilePath);
                    var cfg = JsonConvert.DeserializeObject<WatchConfig>(json);
                    if (cfg != null)
                    {
                        txtProjectPath.Text = cfg.ProjectPath;
                        txtProjectFile.Text = cfg.ProjectFile;
                        txtExePath.Text = cfg.ExePath;
                        txtProcessName.Text = cfg.ProcessName;
                        txtMsbuildPath.Text = cfg.MsbuildPath;
                        Log("⚙️ Config loaded.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("❗ Load config error: " + ex.Message);
            }
        }

        private void SaveConfig()
        {
            try
            {
                var cfg = new WatchConfig
                {
                    ProjectPath = txtProjectPath.Text.Trim(),
                    ProjectFile = txtProjectFile.Text.Trim(),
                    ExePath = txtExePath.Text.Trim(),
                    ProcessName = txtProcessName.Text.Trim(),
                    MsbuildPath = txtMsbuildPath.Text.Trim()
                };
                var json = JsonConvert.SerializeObject(cfg);
                File.WriteAllText(configFilePath, json);
                Log("💾 Config saved.");
            }
            catch (Exception ex)
            {
                Log("❗ Save config error: " + ex.Message);
            }
        }

        private void Log(string message)
        {
            txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        private void SafeLog(string message)
        {
            if (txtLog.InvokeRequired)
                txtLog.Invoke(new Action(() => Log(message)));
            else
                Log(message);
        }

        private class WatchConfig
        {
            public string ProjectPath { get; set; }
            public string ProjectFile { get; set; }
            public string ExePath { get; set; }
            public string ProcessName { get; set; }
            public string MsbuildPath { get; set; }
        }
    }
}
