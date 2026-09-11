using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
namespace Vudaco.Shares.Services
{
     public class TestBackgroundService : BackgroundService
    {
        private readonly ILogger<TestBackgroundService> _logger;
        private readonly TestGoogleDriveService _googleDrive;

        public TestBackgroundService(
            ILogger<TestBackgroundService> logger,
            TestGoogleDriveService googleDrive)
        {
            _logger = logger;
            _googleDrive = googleDrive;
        }

        protected override async Task ExecuteAsync( CancellationToken stoppingToken)
        {
            _logger.LogInformation("Test Background Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation( "Starting database backup...");

                    var driveFileId = await _googleDrive.UploadAsync("C:\\Users\\admin\\Downloads\\vudaco_20260911_115109.rar");

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Database backup or Google Drive upload failed: {ErrorMessage}", ex.Message);

                    // Nếu lỗi thì thử lại sau 5 phút
                    await Task.Delay( TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation( "Backup Background Service stopped.");
        }
    }
}