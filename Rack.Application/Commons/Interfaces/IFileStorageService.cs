namespace Rack.Application.Commons.Interfaces;

public record FileListItem(string Key, long Size, DateTimeOffset? LastModified);
public record PresignedUrlResult(string Url, string? FileKey = null); // FileKey cho upload

public interface IFileStorageService
{
    Task<IEnumerable<FileListItem>> ListFilesAsync(string? prefix = null, CancellationToken cancellationToken = default);

    Task<PresignedUrlResult> GeneratePresignedUploadUrlAsync(string fileName, string contentType, long fileSize, CancellationToken cancellationToken = default);

    Task<PresignedUrlResult> GeneratePresignedDownloadUrlAsync(string fileKey, CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(string fileKey, CancellationToken cancellationToken = default);
}