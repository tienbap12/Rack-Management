using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.ApiEndpoints;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Rack.Application.Commons.DTOs.Files;
using Rack.Application.Commons.Interfaces; // Chứa các record domain: FileListItem, PresignedUrlResult
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq; // Dùng cho ForEachAsync
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Services
{
    /// <summary>
    /// Triển khai dịch vụ lưu trữ file sử dụng MinIO theo chuẩn DTO.
    /// </summary>
    public sealed class MinioService : IMinIOService
    {
        private readonly IMinioClient _minioClient;
        private readonly ILogger<MinioService> _logger;
        private readonly IConfiguration _config;

        public MinioService(IConfiguration config, ILogger<MinioService> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var endpoint = _config["MinIO:Endpoint"];
            var accessKey = _config["MinIO:AccessKey"];
            var secretKey = _config["MinIO:SecretKey"];
            var useSsl = _config.GetValue<bool?>("MinIO:UseSSL") ?? false;

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(accessKey) || string.IsNullOrEmpty(secretKey))
            {
                _logger.LogError("Cấu hình MinIO chưa đầy đủ. Kiểm tra Endpoint, AccessKey, SecretKey.");
                throw new InvalidOperationException("Cấu hình MinIO chưa đầy đủ.");
            }

            try
            {
                _minioClient = new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(useSsl)
                    .Build();
                _logger.LogInformation("MinIO Client khởi tạo thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Khởi tạo MinIO client thất bại.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BucketResponse>> ListBucketsAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Đang lấy danh sách bucket từ MinIO...");
            try
            {
                var result = await _minioClient.ListBucketsAsync(cancellationToken).ConfigureAwait(false);
                var bucketResponses = result.Buckets.Select(b => new BucketResponse(b.Name));
                _logger.LogInformation("Lấy thành công {Count} bucket.", bucketResponses.Count());
                return bucketResponses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bucket từ MinIO.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<FileListItemDto>> ListFilesAsync(string bucketName, string? prefix = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentNullException(nameof(bucketName));

            _logger.LogInformation("Đang liệt kê object MinIO với prefix '{Prefix}' trong bucket {BucketName}", prefix ?? "gốc", bucketName);
            var fileList = new List<FileListItem>(); // Sử dụng domain model để build dữ liệu
            try
            {
                var args = new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithPrefix(prefix)
                    .WithRecursive(true); // Liệt kê đệ quy các file trong các thư mục con nếu có

                await _minioClient.ListObjectsAsync(args, cancellationToken)
                    .ForEachAsync(item =>
                    {
                        if (!item.IsDir) // Bỏ qua mục đại diện thư mục
                        {
                            fileList.Add(new FileListItem(
                                item.Key,
                                (long)item.Size,
                                item.LastModifiedDateTime
                            ));
                        }
                    }, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Tìm thấy {Count} file với prefix '{Prefix}' trong bucket {BucketName}", fileList.Count, prefix ?? "gốc", bucketName);

                // Sắp xếp theo ngày giảm dần và mapping từ domain sang DTO
                return fileList
                    .OrderByDescending(f => f.LastModified ?? DateTimeOffset.MinValue)
                    .Select(file => new FileListItemDto(file));
            }
            catch (BucketNotFoundException ex)
            {
                _logger.LogWarning(ex, "Bucket không tồn tại khi liệt kê file: {BucketName}", bucketName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi liệt kê object MinIO với prefix '{Prefix}' trong bucket {BucketName}", prefix ?? "gốc", bucketName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PresignedUrlResultDto> GeneratePresignedUploadUrlAsync(string bucketName, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentNullException(nameof(bucketName));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            _logger.LogInformation("Đang tạo URL upload cho: {FileName} ({ContentType}, {FileSize} bytes) vào bucket {BucketName}", fileName, contentType, fileSize, bucketName);

            try
            {
                // Tạo file key duy nhất với folder 'uploads'
                var fileKey = $"uploads/s3.vnso-{Guid.NewGuid()}-{Path.GetFileName(fileName)}";
                int expiry = _config.GetValue<int?>("MinIO:PresignedUrlExpirySeconds") ?? 600; // Mặc định 10 phút

                var args = new PresignedPutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileKey)
                    .WithExpiry(expiry);

                string presignedUrl = await _minioClient.PresignedPutObjectAsync(args).ConfigureAwait(false);

                _logger.LogInformation("Tạo thành công URL upload cho key {FileKey} trong bucket {BucketName}", fileKey, bucketName);

                // Mapping domain result sang DTO
                var domainResult = new PresignedUrlResult(presignedUrl, fileKey);
                return new PresignedUrlResultDto(domainResult);
            }
            catch (BucketNotFoundException ex)
            {
                _logger.LogError(ex, "Bucket không tồn tại khi tạo URL upload: {BucketName}", bucketName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo URL upload cho: {FileName} vào bucket {BucketName}", fileName, bucketName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<PresignedUrlResultDto> GeneratePresignedDownloadUrlAsync(string bucketName, string fileKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentNullException(nameof(bucketName));
            if (string.IsNullOrWhiteSpace(fileKey))
                throw new ArgumentNullException(nameof(fileKey));

            _logger.LogInformation("Đang tạo URL download cho: {FileKey} trong bucket {BucketName}", fileKey, bucketName);

            try
            {
                int expiry = _config.GetValue<int?>("MinIO:PresignedUrlExpirySeconds") ?? 600;

                var args = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileKey)
                    .WithExpiry(expiry);

                string presignedUrl = await _minioClient.PresignedGetObjectAsync(args).ConfigureAwait(false);

                _logger.LogInformation("Tạo thành công URL download cho {FileKey} trong bucket {BucketName}", fileKey, bucketName);

                var domainResult = new PresignedUrlResult(presignedUrl);
                return new PresignedUrlResultDto(domainResult);
            }
            catch (BucketNotFoundException ex)
            {
                _logger.LogWarning(ex, "Bucket không tồn tại khi tạo URL download: {BucketName}", bucketName);
                throw;
            }
            catch (ObjectNotFoundException ex)
            {
                _logger.LogWarning(ex, "Object không tồn tại khi tạo URL download: {FileKey} trong bucket {BucketName}", fileKey, bucketName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo URL download cho: {FileKey} trong bucket {BucketName}", fileKey, bucketName);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteFileAsync(string bucketName, string fileKey, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentNullException(nameof(bucketName));
            if (string.IsNullOrWhiteSpace(fileKey))
                throw new ArgumentNullException(nameof(fileKey));

            _logger.LogInformation("Đang xóa object MinIO: {FileKey} từ bucket {BucketName}", fileKey, bucketName);

            try
            {
                var args = new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileKey);

                await _minioClient.RemoveObjectAsync(args, cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Xóa thành công object MinIO: {FileKey} từ bucket {BucketName}", fileKey, bucketName);
                return true;
            }
            catch (BucketNotFoundException)
            {
                _logger.LogWarning("Bucket không tồn tại khi xóa file: {BucketName}", bucketName);
                return false;
            }
            catch (ObjectNotFoundException)
            {
                _logger.LogWarning("Object không tồn tại khi cố gắng xóa: {FileKey} từ bucket {BucketName}. Coi như đã xóa.", fileKey, bucketName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa object MinIO: {FileKey} từ bucket {BucketName}", fileKey, bucketName);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> UploadFileAsync(string bucketName, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentNullException(nameof(bucketName));
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("Stream file không được null hoặc rỗng.", nameof(fileStream));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            // Tạo file key duy nhất với folder 'uploads'
            var fileKey = $"uploads/s3.vnso-{Guid.NewGuid()}-{Path.GetFileName(fileName)}";

            _logger.LogInformation("Đang upload file trực tiếp. Bucket: {BucketName}, Key: {FileKey}, Loại: {ContentType}, Kích thước: {Size}", bucketName, fileKey, contentType, fileStream.Length);

            try
            {
                if (fileStream.CanSeek)
                {
                    fileStream.Seek(0, SeekOrigin.Begin);
                }

                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileKey)
                    .WithStreamData(fileStream)
                    .WithObjectSize(fileStream.Length)
                    .WithContentType(contentType);

                var result = await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("Upload file trực tiếp thành công. Bucket: {BucketName}, Key: {FileKey}, ETag: {ETag}", bucketName, fileKey, result.Etag);
                return fileKey;
            }
            catch (BucketNotFoundException ex)
            {
                _logger.LogError(ex, "Bucket không tồn tại khi upload trực tiếp: {BucketName}", bucketName);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload file trực tiếp. Bucket: {BucketName}, Key dự định: {FileKey}", bucketName, fileKey);
                throw;
            }
        }

        public async Task<IEnumerable<PrefixResponse>> ListPrefixesAsync(string bucketName, string? currentPrefix = null, CancellationToken cancellationToken = default)
        {
            // --- Input Validation ---
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentNullException(nameof(bucketName));

            _logger.LogInformation("Liệt kê prefixes con của '{Prefix}' trong bucket {BucketName}", currentPrefix ?? "gốc", bucketName);

            try
            {
                // --- Cấu hình truy vấn MinIO ---
                var listArgs = new ListObjectsArgs()
                    .WithBucket(bucketName)
                    .WithPrefix(currentPrefix)
                    .WithRecursive(false); // Chỉ lấy các mục ở cấp hiện tại

                // --- Thu thập bất đồng bộ các mục từ Observable ---
                var items = await GetItemsFromObservableAsync(listArgs, cancellationToken).ConfigureAwait(false);

                // --- Xử lý kết quả để trích xuất Prefixes ---
                var prefixes = items
                    .Where(item => item.IsDir && // Chỉ lấy các mục là "thư mục" (prefix)
                                   !string.IsNullOrEmpty(item.Key) && // Key không rỗng
                                   item.Key != currentPrefix) // Không lấy lại prefix cha
                    .Select(item => new PrefixResponse(item.Key)) // Tạo DTO Response
                    .OrderBy(p => p.Prefix) // Sắp xếp theo alphabet
                    .ToList(); // Chuyển thành List

                _logger.LogInformation("Tìm thấy {Count} prefixes con của '{Prefix}' trong bucket {BucketName}", prefixes.Count, currentPrefix ?? "gốc", bucketName);
                return prefixes;
            }
            // --- Xử lý các lỗi cụ thể từ MinIO ---
            catch (BucketNotFoundException ex)
            {
                _logger.LogWarning(ex, "Bucket không tồn tại khi liệt kê prefixes: {BucketName}", bucketName);
                // Có thể trả về danh sách rỗng hoặc ném lại lỗi tùy yêu cầu
                // return Enumerable.Empty<PrefixResponse>();
                throw; // Ném lại để Controller xử lý thành NotFound hoặc lỗi khác
            }
            catch (MinioException ex) // Bắt các lỗi MinIO khác
            {
                _logger.LogError(ex, "Lỗi MinIO khi liệt kê prefixes con của '{Prefix}' trong bucket {BucketName}", currentPrefix ?? "gốc", bucketName);
                throw; // Ném lại lỗi
            }
            // --- Xử lý lỗi chung ---
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi không xác định khi liệt kê prefixes con của '{Prefix}' trong bucket {BucketName}", currentPrefix ?? "gốc", bucketName);
                throw; // Ném lại lỗi
            }
        }

        /// <summary>
        /// Phương thức helper để chuyển đổi IObservable<Item> thành Task<List<Item>>.
        /// </summary>
        private async Task<List<Minio.DataModel.Item>> GetItemsFromObservableAsync(ListObjectsArgs args, CancellationToken cancellationToken)
        {
            var items = new List<Minio.DataModel.Item>();
            var tcs = new TaskCompletionSource<bool>(); // Dùng TaskCompletionSource để đợi Observable

            // Lấy Observable từ client
            var listObjectsObservable = _minioClient.ListObjectsAsync(args, cancellationToken);

            // Đăng ký vào Observable
            var subscription = listObjectsObservable.Subscribe(
                item => items.Add(item), // OnNext: Thêm item vào danh sách
                ex => tcs.TrySetException(ex), // OnError: Báo lỗi cho Task
                () => tcs.TrySetResult(true) // OnCompleted: Báo thành công cho Task
            );

            // Sử dụng using để đảm bảo Dispose subscription
            using (subscription)
            {
                // Tạo một task để theo dõi cancellation
                var cancellationTask = Task.Delay(Timeout.Infinite, cancellationToken);
                // Chờ task của Observable hoặc task cancellation hoàn thành
                var completedTask = await Task.WhenAny(tcs.Task, cancellationTask).ConfigureAwait(false);

                // Nếu task cancellation hoàn thành trước (bị hủy), ném lỗi hủy bỏ
                if (completedTask == cancellationTask)
                {
                    // Task bị hủy bởi CancellationToken
                    _logger.LogWarning("ListObjectsAsync operation was cancelled for bucket {Bucket} and prefix {Prefix}.", args.WithBucket, args.WithPrefix);
                    throw new OperationCanceledException(cancellationToken);
                }

                // Nếu task của Observable hoàn thành (dù thành công hay lỗi),
                // await lại nó để ném exception nếu có lỗi xảy ra trong Observable
                await tcs.Task.ConfigureAwait(false);
            }

            return items; // Trả về danh sách các items đã thu thập
        }
    }
}