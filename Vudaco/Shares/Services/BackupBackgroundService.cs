using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vudaco.Shares.Services
{
     public class BackupBackgroundService : BackgroundService
    {
        private readonly ILogger<BackupBackgroundService> _logger;
        private readonly DatabaseBackupService _databaseBackup;
        private readonly CloudflareR2Service _cloudflareR2;

        public BackupBackgroundService(
            ILogger<BackupBackgroundService> logger,
            DatabaseBackupService databaseBackup,
            CloudflareR2Service cloudflareR2)
        {
            _logger = logger;
            _databaseBackup = databaseBackup;
            _cloudflareR2 = cloudflareR2;
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken)
        {
            _logger.LogInformation("Backup Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation( "Starting database backup...");

                    var backupFile = await _databaseBackup.BackupAsync();

                    _logger.LogInformation("Database backup archive created: {BackupFile}", backupFile);

                    var objectKey = System.IO.Path.GetFileName(backupFile);
                    await _cloudflareR2.UploadFileAsync(backupFile, objectKey);

                    _logger.LogInformation("Database backup uploaded to Cloudflare R2: {ObjectKey}", objectKey);

                    var deletedCount = await _cloudflareR2.DeleteOldBackupsAsync(3);

                    _logger.LogInformation(
                        "Old Cloudflare R2 backups removed: {DeletedCount}; keeping the 3 newest backups.",
                        deletedCount);

                    await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database backup or Cloudflare R2 upload failed: {ErrorMessage}", ex.Message);

                    // Nếu lỗi thì thử lại sau 5 phút
                    await Task.Delay( TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation( "Backup Background Service stopped.");
        }
    }
}