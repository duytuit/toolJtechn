using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Shares.Services
{
   public class DatabaseBackupService
    {
        private readonly IConfiguration _configuration;

        public DatabaseBackupService( IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> BackupAsync()
        {
            var connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            var backupFolder = _configuration["Backup:RemotePath"];
            var localFolder = _configuration["Backup:LocalPath"];

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Thiếu ConnectionStrings:DefaultConnection.");
            }

            if (string.IsNullOrWhiteSpace(backupFolder))
            {
                throw new InvalidOperationException("Thiếu cấu hình Backup:RemotePath.");
            }

            if (string.IsNullOrWhiteSpace(localFolder))
            {
                throw new InvalidOperationException("Thiếu cấu hình Backup:LocalPath.");
            }

            // Lấy thông tin database
            var builder = new SqlConnectionStringBuilder(connectionString);

            var databaseName = builder.InitialCatalog;

            // Tên file backup
            var fileName = $"{databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak";

            var backupPath = Path.Combine(backupFolder, fileName);

            Directory.CreateDirectory(backupFolder);

            var sql = $@"
                    BACKUP DATABASE [{databaseName}]
                    TO DISK = N'{backupPath.Replace("'", "''")}'
                    WITH INIT,STATS = 10;";

            await using var connection = new SqlConnection(connectionString);

            await connection.OpenAsync();

            await using var command = new SqlCommand(sql, connection);

            // Không giới hạn timeout
            command.CommandTimeout = 0;

            await command.ExecuteNonQueryAsync();

            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy file backup sau khi thực hiện backup.",
                    backupPath);
            }

            var archivePath = Path.ChangeExtension(backupPath, ".rar");
            await CompressToRarAsync(backupPath, archivePath);

            File.Delete(backupPath);

            Directory.CreateDirectory(localFolder);
            var localArchivePath = Path.Combine(localFolder, Path.GetFileName(archivePath));
            File.Copy(archivePath, localArchivePath, overwrite: true);

            DeleteOldArchives(backupFolder);
            DeleteOldArchives(localFolder);
            return localArchivePath;

        }

        private async Task CompressToRarAsync(string sourcePath, string archivePath)
        {
            var rarExecutable = _configuration["Backup:RarExecutable"];

            if (string.IsNullOrWhiteSpace(rarExecutable))
            {
                rarExecutable = FindRarExecutable();
            }

            if (string.IsNullOrWhiteSpace(rarExecutable) || !File.Exists(rarExecutable))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy WinRAR/rar.exe. Cấu hình Backup:RarExecutable trỏ tới file thực thi RAR.",
                    rarExecutable);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = rarExecutable,
                Arguments = $"a -ep1 -y \"{archivePath}\" \"{sourcePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            using var process = Process.Start(startInfo);

            if (process == null)
            {
                throw new InvalidOperationException("Không thể khởi động WinRAR/rar.exe.");
            }

            await process.WaitForExitAsync();

            if (process.ExitCode != 0 || !File.Exists(archivePath))
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new InvalidOperationException(
                    $"Nén file backup thành RAR thất bại (ExitCode: {process.ExitCode}). {error}");
            }
        }

        private static string FindRarExecutable()
        {
            var candidates = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WinRAR", "WinRAR.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "WinRAR", "WinRAR.exe"),
                "rar.exe"
            };

            return candidates.FirstOrDefault(File.Exists);
        }

        private static void DeleteOldArchives(string backupFolder)
        {
            var oldBackups = new DirectoryInfo(backupFolder)
                .GetFiles("*.rar")
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .Skip(3);

            foreach (var oldBackup in oldBackups)
            {
                oldBackup.Delete();
            }
        }
    }
}