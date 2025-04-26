using Rack.Application.Commons.DTOs.Files;

namespace Rack.Application.Commons.Interfaces;

public record FileListItem(string Key, long Size, DateTimeOffset? LastModified);
public record PresignedUrlResult(string Url, string? FileKey = null); // FileKey cho upload
public record BucketResponse(string Name);
public record PrefixResponse(string Prefix);

public interface IMinIOService
{
    /// <summary>
    /// Tạo URL upload cho một file vào bucket cụ thể.
    /// </summary>
    Task<PresignedUrlResultDto> GeneratePresignedUploadUrlAsync(string bucketName, string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo URL download cho một file từ bucket cụ thể.
    /// </summary>
    Task<PresignedUrlResultDto> GeneratePresignedDownloadUrlAsync(string bucketName, string fileKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa một file khỏi bucket cụ thể.
    /// </summary>
    Task<bool> DeleteFileAsync(string bucketName, string fileKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload file trực tiếp vào bucket cụ thể.
    /// </summary>
    Task<string> UploadFileAsync(string bucketName, Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách tên các bucket hiện có.
    /// </summary>
    Task<IEnumerable<BucketResponse>> ListBucketsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Liệt kê các file trong một bucket cụ thể.
    /// </summary>
    Task<IEnumerable<FileListItemDto>> ListFilesAsync(string bucketName, string? prefix = null, CancellationToken cancellationToken = default);

    Task<IEnumerable<PrefixResponse>> ListPrefixesAsync(string bucketName, string? currentPrefix = null, CancellationToken cancellationToken = default);
}