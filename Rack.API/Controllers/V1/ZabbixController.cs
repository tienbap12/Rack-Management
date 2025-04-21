using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rack.Application.Commons.DTOs;
using Rack.Application.Commons.DTOs.Zabbix;
using Rack.Application.Commons.Interfaces;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.API.Controllers.V1
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZabbixController : ControllerBase
    {
        private readonly IZabbixService _zabbixService;
        private readonly ILogger<ZabbixController> _logger;

        public ZabbixController(
            IZabbixService zabbixService,
            ILogger<ZabbixController> logger)
        {
            _zabbixService = zabbixService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách host được giám sát
        /// </summary>
        [HttpGet("hosts")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetMonitoredHosts(
            CancellationToken cancellationToken)
        {
            try
            {
                var hosts = await _zabbixService.GetMonitoredHostsAsync(cancellationToken);
                return new SuccessApiResponseDto<IEnumerable<ZabbixHostSummary>>(
                    "Lấy danh sách host thành công",
                    HttpStatusCodeEnum.OK,
                    hosts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách host");
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy danh sách host",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto("HostListError", ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Lấy chi tiết host theo ID
        /// </summary>
        [HttpGet("hosts/{hostId}")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetHostDetail(
            [Required] string hostId,
            CancellationToken cancellationToken)
        {
            try
            {
                var hostDetail = await _zabbixService.GetHostDetailsAsync(hostId, cancellationToken);

                return hostDetail == null
                    ? new FailureApiResponseDto(
                        "Không tìm thấy host",
                        HttpStatusCodeEnum.NotFound,
                        new ErrorDto("HostNotFound", $"Host ID {hostId} không tồn tại", ErrorType.Failure))
                    : new SuccessApiResponseDto<ZabbixHostDetail>(
                        "Lấy chi tiết host thành công",
                        HttpStatusCodeEnum.OK,
                        hostDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết host {HostId}", hostId);
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy chi tiết host",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto("HostDetailError", ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Lấy danh sách vấn đề theo host
        /// </summary>
        [HttpGet("hosts/{hostId}/problems")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetProblemsByHost(
            [Required] string hostId,
            CancellationToken cancellationToken)
        {
            try
            {
                var problems = await _zabbixService.GetProblemsByHostAsync(hostId, cancellationToken);
                return new SuccessApiResponseDto<IEnumerable<ZabbixProblemInfo>>(
                    "Lấy danh sách vấn đề thành công",
                    HttpStatusCodeEnum.OK,
                    problems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy vấn đề cho host {HostId}", hostId);
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy vấn đề",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto("HostProblemError", ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Lấy các vấn đề gần đây theo mức độ nghiêm trọng
        /// </summary>
        [HttpGet("problems/recent")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetRecentProblems(
            [FromQuery] int severityThreshold = 0,
            [FromQuery] int limit = 100,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (limit < 1 || limit > 1000)
                {
                    return new FailureApiResponseDto(
                        "Giới hạn không hợp lệ",
                        HttpStatusCodeEnum.BadRequest,
                        new ErrorDto("InvalidLimit", "Limit phải từ 1 đến 1000", ErrorType.Failure));
                }

                var problems = await _zabbixService.GetRecentProblemsAsync(
                    severityThreshold,
                    limit,
                    cancellationToken);

                return new SuccessApiResponseDto<IEnumerable<ZabbixProblemInfo>>(
                    "Lấy vấn đề gần đây thành công",
                    HttpStatusCodeEnum.OK,
                    problems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy vấn đề gần đây");
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy vấn đề",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto("RecentProblemError", ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Lấy thông tin tài nguyên của host
        /// </summary>
        [HttpGet("hosts/{hostId}/resources")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetHostResourceItems(
            [Required] string hostId,
            [FromQuery(Name = "keys")][Required] IEnumerable<string> itemKeys,
            CancellationToken cancellationToken)
        {
            try
            {
                if (!itemKeys.Any())
                {
                    return new FailureApiResponseDto(
                        "Thiếu tham số bắt buộc",
                        HttpStatusCodeEnum.BadRequest,
                        new ErrorDto("MissingKeys", "Danh sách item keys không được rỗng", ErrorType.Failure));
                }

                var resources = await _zabbixService.GetHostResourceItemsAsync(
                    hostId,
                    itemKeys,
                    cancellationToken);

                return new SuccessApiResponseDto<Dictionary<string, string>>(
                    "Lấy thông tin tài nguyên thành công",
                    HttpStatusCodeEnum.OK,
                    resources);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tài nguyên cho host {HostId}", hostId);
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy tài nguyên",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto("ResourceError", ex.Message, ErrorType.InternalServerError));
            }
        }
    }
}