
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Upload;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Util.Store;
using Microsoft.Extensions.Logging;
namespace Vudaco.Shares.Services
{
     public class TestGoogleDriveService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TestGoogleDriveService> _logger;

        public TestGoogleDriveService(
            IConfiguration configuration,
            ILogger<TestGoogleDriveService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private async Task<DriveService> CreateDriveServiceAsync()
        {
            var credentialsFile = _configuration["GoogleDrive:CredentialsFile"];
            var tokenStorePath = _configuration["GoogleDrive:TokenStorePath"] ?? "GoogleDriveTokens";
            var user = _configuration["GoogleDrive:User"] ?? "backup-service";

            if (string.IsNullOrWhiteSpace(credentialsFile))
            {
                throw new InvalidOperationException("Thiếu cấu hình GoogleDrive:CredentialsFile.");
            }

            var credentialsPath = ResolvePath(credentialsFile);
            var tokenPath = ResolvePath(tokenStorePath);

            _logger.LogInformation(
                "Preparing Google Drive OAuth. Credentials: {CredentialsPath}; token store: {TokenPath}",
                credentialsPath,
                tokenPath);

            if (!System.IO.File.Exists(credentialsPath))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy file OAuth Client Secrets của Google.",
                    credentialsPath);
            }

            GoogleClientSecrets clientSecrets;
            await using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                clientSecrets = GoogleClientSecrets.FromStream(stream);
            }

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets.Secrets,
                new[] { DriveService.Scope.Drive },
                user,
                CancellationToken.None,
                new FileDataStore(tokenPath, true));

            _logger.LogInformation("Google Drive OAuth authorization completed for user {User}.", user);

            return new DriveService(
                new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Vudaco Backup"
                });
        }

        public async Task<string> UploadAsync(
            string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy file backup.",
                    filePath);
            }

            var folderId =_configuration["GoogleDrive:FolderId"];

            if (string.IsNullOrWhiteSpace(folderId))
            {
                throw new InvalidOperationException("Thiếu cấu hình GoogleDrive:FolderId.");
            }

            _logger.LogInformation("Uploading backup archive {FilePath} to Google Drive folder {FolderId}.", filePath, folderId);

            using var service = await CreateDriveServiceAsync();

            var fileMetadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = Path.GetFileName(filePath),
                Parents = new[]
                {
                    folderId
                }
            };

            await using var stream =
                new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read);

            var request =
                service.Files.Create(
                    fileMetadata,
                    stream,
                    "application/octet-stream");

            request.Fields = "id,name,size";

            var result =
                await request.UploadAsync();

            if (result.Status != UploadStatus.Completed)
            {
                throw new Exception(
                    $"Upload Google Drive thất bại: {result.Exception}");
            }

            _logger.LogInformation("Google Drive upload completed: {FileId} ({FileName}).", request.ResponseBody.Id, Path.GetFileName(filePath));

            return request.ResponseBody.Id;
        }

        public async Task DeleteOldBackupsAsync(int keepCount = 3)
        {
            var folderId = _configuration["GoogleDrive:FolderId"];

            if (string.IsNullOrWhiteSpace(folderId))
            {
                throw new InvalidOperationException("Thiếu cấu hình GoogleDrive:FolderId.");
            }

            using var service = await CreateDriveServiceAsync();

            var request = service.Files.List();
            request.Q = $"'{folderId}' in parents and trashed = false and name contains '.rar'";
            request.Fields = "files(id,name,createdTime)";
            request.OrderBy = "createdTime desc";

            var files = await request.ExecuteAsync();
            _logger.LogInformation("Google Drive contains {BackupCount} backup archives before retention cleanup.", files.Files?.Count ?? 0);
            var oldBackups = (files.Files ?? Enumerable.Empty<Google.Apis.Drive.v3.Data.File>())
                .Skip(keepCount)
                .ToList();

            foreach (var oldBackup in oldBackups)
            {
                await service.Files.Delete(oldBackup.Id).ExecuteAsync();
                _logger.LogInformation("Deleted old Google Drive backup {FileName} ({FileId}).", oldBackup.Name, oldBackup.Id);
            }
        }

        private static string ResolvePath(string path)
        {
            return Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppContext.BaseDirectory, path);
        }
    }
}