using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args; // Namespace chứa các Args
using Minio.Exceptions;
using Rack.Application.Commons.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq; // Cần cho ListObjectsAsync().ToList()
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Services;

public sealed class MinioService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinioService> _logger;
    private readonly string _bucketName;

    public MinioService(IConfiguration config, ILogger<MinioService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var endpoint = config["MinIO:Endpoint"];
        var accessKey = config["MinIO:AccessKey"];
        var secretKey = config["MinIO:SecretKey"];
        _bucketName = config["MinIO:BucketName"] ?? throw new InvalidOperationException("MinIO:BucketName configuration missing.");
        var useSsl = config.GetValue<bool?>("MinIO:UseSSL") ?? false; // Mặc định không dùng SSL nếu không rõ

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
        {
            _logger.LogError("MinIO configuration is incomplete. Check Endpoint, AccessKey, SecretKey, BucketName.");
            throw new InvalidOperationException("MinIO configuration is incomplete.");
        }

        try
        {
            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(useSsl)
                .Build();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MinIO client.");
            throw;
        }
    }

    // Helper đảm bảo bucket tồn tại
    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName), cancellationToken);
            if (!found)
            {
                _logger.LogWarning($"Bucket '{_bucketName}' not found. Attempting to create.");
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName), cancellationToken);
                _logger.LogInformation($"Bucket '{_bucketName}' created successfully.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error ensuring bucket '{_bucketName}' exists.");
            throw; // Ném lại lỗi để báo hiệu thất bại
        }
    }

    public async Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileKey)) return false;
        _logger.LogInformation("Attempting to delete MinIO object: {FileKey} from bucket {BucketName}", fileKey, _bucketName);
        try
        {
            var args = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileKey); // Object chính là fileKey

            await _minioClient.RemoveObjectAsync(args, cancellationToken);
            _logger.LogInformation("Successfully deleted MinIO object: {FileKey}", fileKey);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete MinIO object: {FileKey}", fileKey);
            // Có thể trả về false hoặc ném lỗi tùy theo yêu cầu xử lý lỗi
            return false;
            // throw;
        }
    }

    public async Task<PresignedUrlResult> GeneratePresignedDownloadUrlAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileKey)) throw new ArgumentNullException(nameof(fileKey));
        _logger.LogInformation("Generating presigned download URL for: {FileKey} in bucket {BucketName}", fileKey, _bucketName);

        try
        {
            // Thời gian hết hạn URL (ví dụ: 10 phút)
            int expiry = 600; // seconds

            var args = new PresignedGetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileKey)
                .WithExpiry(expiry);

            string presignedUrl = await _minioClient.PresignedGetObjectAsync(args).ConfigureAwait(false); // ConfigureAwait(false) tốt cho thư viện

            _logger.LogInformation("Generated presigned download URL for {FileKey}", fileKey);
            return new PresignedUrlResult(presignedUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate presigned download URL for: {FileKey}", fileKey);
            throw; // Ném lỗi để lớp Application/Api xử lý
        }
    }

    public async Task<PresignedUrlResult> GeneratePresignedUploadUrlAsync(string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
        _logger.LogInformation("Generating presigned upload URL for: {FileName} ({ContentType}, {FileSize} bytes) in bucket {BucketName}", fileName, contentType, fileSize, _bucketName);

        // TODO: Thêm logic kiểm tra fileSize nếu cần (ví dụ: giới hạn kích thước)

        try
        {
            // Tạo key duy nhất (ví dụ) - Cần logic tốt hơn cho production
            var fileKey = $"uploads/{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid()}-{Path.GetFileName(fileName)}"; // Đảm bảo fileKey hợp lệ

            int expiry = 600; // URL hợp lệ trong 10 phút

            var args = new PresignedPutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(fileKey)
                .WithExpiry(expiry);
            // .WithContentType(contentType); // Có thể thêm content type nếu cần

            string presignedUrl = await _minioClient.PresignedPutObjectAsync(args).ConfigureAwait(false);

            _logger.LogInformation("Generated presigned upload URL for key {FileKey}", fileKey);
            // Trả về cả URL và Key để client biết lưu file vào đâu
            return new PresignedUrlResult(presignedUrl, fileKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate presigned upload URL for: {FileName}", fileName);
            throw;
        }
    }

    public async Task<IEnumerable<FileListItem>> ListFilesAsync(string? prefix = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Listing MinIO objects with prefix '{Prefix}' in bucket {BucketName}", prefix ?? "root", _bucketName);
        var fileList = new List<FileListItem>();
        try
        {
            var args = new ListObjectsArgs()
                .WithBucket(_bucketName)
                .WithPrefix(prefix) // Lọc theo prefix (thư mục)
                .WithRecursive(true); // Lấy tất cả các file trong prefix và thư mục con

            // ListObjectsAsync trả về IObservable, cần dùng System.Reactive.Linq
            await _minioClient.ListObjectsAsync(args, cancellationToken)
                .ForEachAsync(item =>
                {
                    if (!item.IsDir) // Chỉ lấy file, bỏ qua thư mục
                    {
                        fileList.Add(new FileListItem(
                            item.Key,
                            (long)(item.Size), // Chuyển đổi ulong? sang long
                            item.LastModifiedDateTime // Lấy DateTimeOffset?
                        ));
                    }
                }, cancellationToken);

            _logger.LogInformation("Found {Count} files with prefix '{Prefix}'", fileList.Count, prefix ?? "root");
            // Sắp xếp theo ngày sửa đổi giảm dần (mới nhất trước)
            return fileList.OrderByDescending(f => f.LastModified ?? DateTimeOffset.MinValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list MinIO objects with prefix '{Prefix}'", prefix ?? "root");
            throw;
        }
    }
}