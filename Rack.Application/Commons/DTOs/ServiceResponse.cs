using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;
using System.Text.Json.Serialization;

namespace Rack.Application.Commons.DTOs
{
    /// <summary>
    /// Chi tiết lỗi trả về cho client.
    /// </summary>
    public class ErrorDto
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public ErrorType Type { get; set; }

        public ErrorDto(string code, string message, ErrorType type)
        {
            Code = code; Message = message; Type = type;
        }

        // Constructor để map từ Error domain nếu cần
        public ErrorDto(Error domainError) : this(domainError.Code, domainError.Message, domainError.Type) { }
    }

    /// <summary>
    /// Cấu trúc cơ sở cho phản hồi API.
    /// </summary>
    public abstract class ApiResponseBaseDto // Đặt tên rõ ràng hơn là DTO
    {
        /// <summary>
        /// Thông điệp mô tả kết quả.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Mã trạng thái HTTP tùy chỉnh.
        /// </summary>
        public HttpStatusCodeEnum StatusCode { get; set; }

        protected ApiResponseBaseDto(string message, HttpStatusCodeEnum statusCode)
        {
            Message = message; StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Phản hồi API thành công không chứa dữ liệu cụ thể (hoặc dữ liệu được bao gồm trong lớp kế thừa).
    /// </summary>
    public class SuccessApiResponseDto : ApiResponseBaseDto
    {
        public SuccessApiResponseDto(string message, HttpStatusCodeEnum statusCode)
             : base(message, statusCode) { }
    }

    /// <summary>
    /// Phản hồi API thành công chứa dữ liệu.
    /// </summary>
    /// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
    public class SuccessApiResponseDto<T> : ApiResponseBaseDto
    {
        /// <summary>
        /// Dữ liệu trả về.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        public SuccessApiResponseDto(string message, HttpStatusCodeEnum statusCode, T? data)
            : base(message, statusCode)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Phản hồi API thất bại.
    /// </summary>
    public class FailureApiResponseDto : ApiResponseBaseDto
    {
        /// <summary>
        /// Chi tiết lỗi.
        /// </summary>
        public ErrorDto Error { get; set; }

        public FailureApiResponseDto(string message, HttpStatusCodeEnum statusCode, ErrorDto error)
            : base(message, statusCode)
        {
            Error = error;
        }

        // Constructor để map từ Response domain nếu cần
        public FailureApiResponseDto(Response domainResponse)
            : base(domainResponse.Message, domainResponse.StatusCode)
        {
            if (domainResponse.IsSuccess) throw new ArgumentException("Cannot create FailureResponseDto from a success response.");
            Error = new ErrorDto(domainResponse.Error);
        }
    }
}