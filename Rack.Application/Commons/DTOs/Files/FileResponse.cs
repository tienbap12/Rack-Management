using Rack.Application.Commons.Interfaces;
using System.Text.Json.Serialization;

namespace Rack.Application.Commons.DTOs.Files
{
    /// <summary>
    /// Thông tin chi tiết về một file trong danh sách.
    /// </summary>
    public record FileListItemDto
    {
        /// <summary>
        /// Khóa (đường dẫn) của file trong storage.
        /// </summary>
        public string Key { get; init; }

        /// <summary>
        /// Kích thước file (byte).
        /// </summary>
        public long Size { get; init; }

        /// <summary>
        /// Thời điểm sửa đổi cuối cùng (UTC).
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DateTimeOffset? LastModified { get; init; }

        // Constructor để map từ Domain record nếu cần
        public FileListItemDto(FileListItem domainItem)
        {
            Key = domainItem.Key;
            Size = domainItem.Size;
            LastModified = domainItem.LastModified;
        }
    }

    /// <summary>
    /// Kết quả chứa URL được ký sẵn (và tùy chọn FileKey cho upload).
    /// </summary>
    public record PresignedUrlResultDto
    {
        /// <summary>
        /// URL được ký sẵn.
        /// </summary>
        public string Url { get; init; }

        /// <summary>
        /// Khóa (đường dẫn) của file sẽ được upload (chỉ dùng cho upload).
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FileKey { get; init; }

        // Constructor để map từ Domain record nếu cần
        public PresignedUrlResultDto(PresignedUrlResult domainResult)
        {
            Url = domainResult.Url;
            FileKey = domainResult.FileKey;
        }

        // Constructor chỉ cho URL (dùng cho download)
        public PresignedUrlResultDto(string url)
        {
            Url = url;
            FileKey = null;
        }
    }
}