using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Vudaco.Shares.Services
{
    public class CloudflareR2Service
    {
        private readonly IAmazonS3 _s3;
        private readonly string _bucketName;

        public CloudflareR2Service(IConfiguration configuration)
        {
            var accountId =
                configuration["CloudflareR2:AccountId"];

            var accessKey =
                configuration["CloudflareR2:AccessKeyId"];

            var secretKey =
                configuration["CloudflareR2:SecretAccessKey"];

            _bucketName =
                configuration["CloudflareR2:BucketName"];

            var credentials =
                new Amazon.Runtime.BasicAWSCredentials(
                    accessKey,
                    secretKey
                );

            var config = new AmazonS3Config
            {
                ServiceURL =
                    $"https://{accountId}.r2.cloudflarestorage.com"
            };

            _s3 = new AmazonS3Client(
                credentials,
                config
            );
        }

        public async Task UploadFileAsync(
            string filePath,
            string objectKey)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Không tìm thấy file",
                    filePath
                );
            }

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                FilePath = filePath,

                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true
            };

            await _s3.PutObjectAsync(request);
        }

        public async Task<int> DeleteOldBackupsAsync(int keepCount = 3)
        {
            var backups = new System.Collections.Generic.List<S3Object>();
            string continuationToken = null;

            do
            {
                var response = await _s3.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    ContinuationToken = continuationToken
                });

                backups.AddRange(response.S3Objects.Where(file =>
                    file.Key.EndsWith(".rar", StringComparison.OrdinalIgnoreCase)));

                continuationToken = response.IsTruncated == true
                    ? response.NextContinuationToken
                    : null;
            }
            while (!string.IsNullOrEmpty(continuationToken));

            var oldBackups = backups
                .OrderByDescending(file => file.LastModified)
                .Skip(keepCount)
                .ToList();

            foreach (var oldBackup in oldBackups)
            {
                await _s3.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = oldBackup.Key
                });
            }

            return oldBackups.Count;
        }
    }
}