using System.ComponentModel.DataAnnotations;

namespace Rack.Application.Commons.DTOs.Files
{
    /// <summary>
    /// Dữ liệu đầu vào để yêu cầu tạo URL upload được ký sẵn.
    /// </summary>
    public record GenerateUploadUrlRequestDto // Đổi tên từ GenerateUploadUrlRequest để rõ ràng là DTO
    {
        [Required(ErrorMessage = "Tên file là bắt buộc.")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Độ dài tên file phải từ 1 đến 255 ký tự.")]
        public string FileName { get; init; }

        [Required(ErrorMessage = "Loại nội dung (Content type) là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Loại nội dung quá dài.")]
        public string ContentType { get; init; }

        [Required(ErrorMessage = "Kích thước file là bắt buộc.")]
        [Range(1, 10737418240, ErrorMessage = "Kích thước file phải từ 1 byte đến 10 GB.")] // Giới hạn ví dụ 10GB
        public long FileSize { get; init; }
    }
}