using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rack.Application.Commons.DTOs;
using Rack.Application.Commons.DTOs.Files;
using Rack.Application.Commons.Interfaces;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.API.Controllers
{
    [ApiController]
    [Route("api/v1/files")]
    public class FileController : ControllerBase
    {
        private readonly IMinIOService _minioService;
        private readonly ILogger<FileController> _logger;

        public FileController(
            IMinIOService minioService,
            ILogger<FileController> logger)
        {
            _minioService = minioService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo URL upload file được ký sẵn
        /// </summary>
        [HttpPost("buckets/{bucketName}/generate-upload-url")]
        public async Task<ActionResult<ApiResponseBaseDto>> GeneratePresignedUploadUrl(
            [FromBody] GenerateUploadUrlRequestDto request,
            [FromRoute][Required] string bucketName)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var error = new ErrorDto(
                        HttpStatusCodeEnum.BadRequest,
                        "Dữ liệu đầu vào không hợp lệ",
                        ErrorType.Failure);
                    return new FailureApiResponseDto(
                        "Lỗi validate",
                        HttpStatusCodeEnum.BadRequest,
                        error);
                }

                var result = await _minioService.GeneratePresignedUploadUrlAsync(
                    bucketName,
                    request.FileName,
                    request.ContentType,
                    request.FileSize);

                return new SuccessApiResponseDto<PresignedUrlResultDto>(
                    "Tạo URL upload thành công",
                    HttpStatusCodeEnum.OK,
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo URL upload");
                var error = new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError);
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi tạo URL upload",
                    HttpStatusCodeEnum.InternalServerError,
                    error);
            }
        }

        /// <summary>
        /// Tạo URL download file được ký sẵn
        /// </summary>
        [HttpGet("buckets/{bucketName}/url/{fileKey}")]
        public async Task<ActionResult<ApiResponseBaseDto>> GeneratePresignedDownloadUrl(
            [FromRoute][Required] string bucketName,
            [FromRoute][Required] string fileKey)
        {
            try
            {
                // Decode fileKey khi nhận từ frontend
                string decodedFileKey = WebUtility.UrlDecode(fileKey);
                _logger.LogInformation("Decoded file key: {DecodedFileKey} from {EncodedFileKey}", decodedFileKey, fileKey);

                var result = await _minioService.GeneratePresignedDownloadUrlAsync(bucketName, decodedFileKey);
                return new SuccessApiResponseDto<PresignedUrlResultDto>(
                    "Tạo URL download thành công",
                    HttpStatusCodeEnum.OK,
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo URL download");
                var error = new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError);
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi tạo URL download",
                    HttpStatusCodeEnum.InternalServerError,
                    error);
            }
        }

        /// <summary>
        /// Xóa file
        /// </summary>
        [HttpDelete("buckets/{bucketName}/{fileKey}")]
        public async Task<ActionResult<ApiResponseBaseDto>> DeleteFile(
            [FromRoute][Required] string bucketName,
            [FromRoute][Required] string fileKey)
        {
            try
            {
                // Decode fileKey khi nhận từ frontend
                string decodedFileKey = WebUtility.UrlDecode(fileKey);
                _logger.LogInformation("Decoded file key for delete: {DecodedFileKey} from {EncodedFileKey}", decodedFileKey, fileKey);

                var success = await _minioService.DeleteFileAsync(bucketName, decodedFileKey);

                return success
                    ? new SuccessApiResponseDto("Xóa file thành công", HttpStatusCodeEnum.OK)
                    : new FailureApiResponseDto(
                        "Xóa file thất bại",
                        HttpStatusCodeEnum.BadRequest,
                        new ErrorDto(HttpStatusCodeEnum.NotFound, "Không tìm thấy file hoặc bucket", ErrorType.Failure));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa file");
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi xóa file",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Upload file trực tiếp
        /// </summary>
        [HttpPost("buckets/{bucketName}/upload-direct")]
        public async Task<ActionResult<ApiResponseBaseDto>> UploadFile(
            [FromRoute][Required] string bucketName,
            [FromForm][Required] IFormFile file)
        {
            try
            {
                using var stream = file.OpenReadStream();
                var fileKey = await _minioService.UploadFileAsync(
                    bucketName,
                    stream,
                    file.FileName,
                    file.ContentType);

                return new SuccessApiResponseDto<string>(
                    "Upload file thành công",
                    HttpStatusCodeEnum.OK,
                    fileKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload file");
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi upload file",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Danh sách bucket
        /// </summary>
        [HttpGet("buckets")]
        public async Task<ActionResult<ApiResponseBaseDto>> ListBuckets()
        {
            try
            {
                var buckets = await _minioService.ListBucketsAsync();
                return new SuccessApiResponseDto<IEnumerable<BucketResponse>>(
                    "Lấy danh sách bucket thành công",
                    HttpStatusCodeEnum.OK,
                    buckets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách bucket");
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy danh sách bucket",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Danh sách file trong bucket
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult<ApiResponseBaseDto>> ListFiles(
            [FromQuery][Required] string bucketName,
            [FromQuery] string? prefix = null)
        {
            try
            {
                var files = await _minioService.ListFilesAsync(bucketName, prefix);
                return new SuccessApiResponseDto<IEnumerable<FileListItemDto>>(
                    "Lấy danh sách file thành công",
                    HttpStatusCodeEnum.OK,
                    files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách file");
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy danh sách file",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        [HttpGet("prefixes")] // Route mới
        public async Task<ActionResult<ApiResponseBaseDto>> ListPrefixes(
       [FromQuery][Required] string bucketName,
       [FromQuery] string? prefix = null, // Prefix hiện tại để lấy thư mục con của nó
       CancellationToken cancellationToken = default)
        {
            try
            {
                var prefixes = await _minioService.ListPrefixesAsync(bucketName, prefix, cancellationToken);
                // Trả về SuccessApiResponseDto chứa List<string>
                return new SuccessApiResponseDto<IEnumerable<PrefixResponse>>(
                    "Lấy danh sách prefix thành công",
                    HttpStatusCodeEnum.OK,
                    prefixes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách prefix");
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy danh sách prefix",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }
    }
}