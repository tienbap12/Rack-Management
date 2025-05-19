using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rack.Application.Commons.DTOs;
using Rack.Application.Commons.DTOs.Zabbix;
using Rack.Application.Commons.Helpers;
using Rack.Application.Commons.Interfaces;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Rack.API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/zabbix")]
    public class ZabbixController : ControllerBase
    {
        private readonly IZabbixService _zabbixService;
        private readonly ILogger<ZabbixController> _logger;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5); // 5-minute cache for Zabbix data

        public ZabbixController(
            IZabbixService zabbixService,
            ILogger<ZabbixController> logger,
            IMemoryCache cache)
        {
            _zabbixService = zabbixService;
            _logger = logger;
            _cache = cache;
        }

        /// <summary>
        /// Lấy danh sách host được giám sát
        /// </summary>
        [HttpGet("hosts")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetAllHostsAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                string cacheKey = "zabbix_all_hosts";
                var hosts = await _cache.GetOrCreateAsync(
                    cacheKey,
                    async (cacheEntry) =>
                    {
                        _logger.LogInformation("Cache miss for zabbix_all_hosts - fetching from service");
                        cacheEntry.SetSize(1); // Set Size to comply with SizeLimit
                        return await _zabbixService.GetMonitoredHostsAsync(cancellationToken);
                    },
                    _cacheExpiration,
                    CacheItemPriority.Normal,
                    cancellationToken);

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
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        [HttpGet("hosts/paged")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetHostsPagedAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? search = null,
            [FromQuery] string? deviceType = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("API: GetHostsPagedAsync - Page: {Page}, PageSize: {PageSize}, Search: {Search}, DeviceType: {DeviceType}",
                    page, pageSize, search, deviceType);

                // QUAN TRỌNG: Luôn tạo cache key duy nhất dựa trên tất cả các tham số
                string cacheKey = $"zabbix_controller_hosts_paged_p{page}_s{pageSize}_search{search ?? "none"}_type{deviceType ?? "all"}";

                _logger.LogInformation("Using cache key: {CacheKey}", cacheKey);

                // Sử dụng tất cả tham số khi lấy từ cache hoặc gọi service
                var hosts = await _cache.GetOrCreateAsync(
                    cacheKey,
                    async (cacheEntry) =>
                    {
                        _logger.LogInformation("Cache miss for {CacheKey} - fetching from service", cacheKey);
                        cacheEntry.SetSize(1); // Set Size to comply with SizeLimit
                        return await _zabbixService.GetHostsPagedAsync(page, pageSize, search, deviceType, cancellationToken);
                    },
                    _cacheExpiration,
                    CacheItemPriority.Normal,
                    cancellationToken);

                return new SuccessApiResponseDto<IEnumerable<ZabbixHostSummary>>(
                    "Lấy danh sách host thành công",
                    HttpStatusCodeEnum.OK,
                    hosts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách host phân trang. Page: {Page}, PageSize: {PageSize}, Search: {Search}, DeviceType: {DeviceType}",
                    page, pageSize, search, deviceType);
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy danh sách host phân trang",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        [HttpGet("hosts/{hostId}/basic")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetHostBasicDetailsAsync(
        string hostId,
        CancellationToken cancellationToken)
        {
            try
            {
                string cacheKey = $"zabbix_host_basic_{hostId}";
                if (!_cache.TryGetValue(cacheKey, out ZabbixHostDetail hostDetails))
                {
                    hostDetails = await _zabbixService.GetHostBasicDetailsAsync(hostId, cancellationToken);

                    if (hostDetails != null)
                    {
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(_cacheExpiration)
                            .SetPriority(CacheItemPriority.High);

                        _cache.Set(cacheKey, hostDetails, cacheOptions);
                    }
                }

                return hostDetails == null
                    ? new FailureApiResponseDto(
                        "Không tìm thấy host",
                        HttpStatusCodeEnum.NotFound,
                        new ErrorDto(HttpStatusCodeEnum.NotFound, $"Host ID {hostId} không tồn tại", ErrorType.Failure))
                    : new SuccessApiResponseDto<ZabbixHostDetail>(
                        "Lấy chi tiết host thành công",
                        HttpStatusCodeEnum.OK,
                        hostDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin cơ bản của host {HostId}", hostId);
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy thông tin cơ bản của host",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Lấy chi tiết host theo ID
        /// </summary>
        [HttpGet("hosts/{hostId}")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetHostDetail(
             string hostId,
            [FromQuery] bool includeProblems = true,
            [FromQuery] bool includeResources = true,
            [FromQuery] int problemLimit = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Use multiple parameters in cache key to handle different combinations
                string cacheKey = $"zabbix_host_detail_{hostId}_{includeProblems}_{includeResources}_{problemLimit}";

                if (!_cache.TryGetValue(cacheKey, out ZabbixHostDetail hostDetail))
                {
                    _logger.LogInformation("Fetching host details from Zabbix API for host {HostId}", hostId);
                    hostDetail = await _zabbixService.GetHostFullDetailsAsync(
                        hostId, includeProblems, includeResources, problemLimit, cancellationToken);

                    if (hostDetail != null)
                    {
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(_cacheExpiration)
                            .SetPriority(CacheItemPriority.High);

                        _cache.Set(cacheKey, hostDetail, cacheOptions);
                    }
                }
                else
                {
                    _logger.LogInformation("Using cached host details for host {HostId}", hostId);
                }

                return hostDetail == null
                    ? new FailureApiResponseDto(
                        "Không tìm thấy host",
                        HttpStatusCodeEnum.NotFound,
                        new ErrorDto(HttpStatusCodeEnum.NotFound, $"Host ID {hostId} không tồn tại", ErrorType.Failure))
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
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Lấy danh sách vấn đề theo host
        /// </summary>
        [HttpGet("hosts/{hostId}/problems")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetProblemsByHost(
             string hostId,
            [FromQuery] int limit = 5,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Cache problems for a shorter time as they're more volatile
                TimeSpan problemsCacheTime = TimeSpan.FromMinutes(2);
                string cacheKey = $"zabbix_host_problems_{hostId}_{limit}";

                if (!_cache.TryGetValue(cacheKey, out IEnumerable<ZabbixProblemInfo> problems))
                {
                    problems = await _zabbixService.GetRecentHostProblemsAsync(hostId, limit, cancellationToken);

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(problemsCacheTime)
                        .SetPriority(CacheItemPriority.Normal);

                    _cache.Set(cacheKey, problems, cacheOptions);
                }

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
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Lấy các vấn đề gần đây theo mức độ nghiêm trọng
        /// </summary>
        [HttpGet("problems")]
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
                        new ErrorDto(HttpStatusCodeEnum.BadRequest, "Limit phải từ 1 đến 1000", ErrorType.Failure));
                }

                // Cache problems for a shorter time
                TimeSpan problemsCacheTime = TimeSpan.FromMinutes(1);
                string cacheKey = $"zabbix_recent_problems_{severityThreshold}_{limit}";

                if (!_cache.TryGetValue(cacheKey, out IEnumerable<ZabbixProblemInfo> problems))
                {
                    _logger.LogInformation("Fetching recent problems from Zabbix API");
                    problems = await _zabbixService.GetRecentProblemsAsync(
                        severityThreshold,
                        limit,
                        cancellationToken);

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(problemsCacheTime)
                        .SetPriority(CacheItemPriority.Normal);

                    _cache.Set(cacheKey, problems, cacheOptions);
                }
                else
                {
                    _logger.LogInformation("Using cached recent problems");
                }

                return new SuccessApiResponseDto<IEnumerable<ZabbixProblemInfo>>(
                    "Lấy problems gần đây thành công",
                    HttpStatusCodeEnum.OK,
                    problems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy vấn đề gần đây");
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy vấn đề",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        /// <summary>
        /// Lấy thông tin tài nguyên của host
        /// </summary>
        [HttpGet("hosts/{hostId}/resources/paged")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetHostResourceItems(
           string hostId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(search))
                {
                    // Only cache non-search queries
                    string cacheKey = $"zabbix_host_resources_{hostId}_{page}_{pageSize}";

                    if (!_cache.TryGetValue(cacheKey, out List<ZabbixItemDetailDto> result))
                    {
                        result = await _zabbixService.GetHostResourcesPagedAsync(hostId, page, pageSize, search, cancellationToken) ?? new List<ZabbixItemDetailDto>();

                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(_cacheExpiration)
                            .SetPriority(CacheItemPriority.Normal);

                        _cache.Set(cacheKey, result, cacheOptions);
                    }

                    return new SuccessApiResponseDto<List<ZabbixItemDetailDto>>(
                        "Lấy thông tin tài nguyên thành công",
                        HttpStatusCodeEnum.OK,
                        result);
                }
                else
                {
                    // Don't cache search queries
                    var result = await _zabbixService.GetHostResourcesPagedAsync(hostId, page, pageSize, search, cancellationToken) ?? new List<ZabbixItemDetailDto>();
                    return new SuccessApiResponseDto<List<ZabbixItemDetailDto>>(
                        "Lấy thông tin tài nguyên thành công",
                        HttpStatusCodeEnum.OK,
                        result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tài nguyên cho host {HostId}", hostId);
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy tài nguyên",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }

        [HttpGet("hosts/{hostId}/resources")]
        public async Task<ActionResult<ApiResponseBaseDto>> GetHostResourceItems(
            [Required] string hostId,
            CancellationToken cancellationToken)
        {
            try
            {
                string cacheKey = $"zabbix_host_resources_all_{hostId}";

                if (!_cache.TryGetValue(cacheKey, out List<ZabbixItemDetailDto> result))
                {
                    result = await _zabbixService.GetHostResourceItemsAsync(hostId, null, cancellationToken) ?? new List<ZabbixItemDetailDto>();

                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_cacheExpiration)
                        .SetPriority(CacheItemPriority.Normal);

                    _cache.Set(cacheKey, result, cacheOptions);
                }

                return new SuccessApiResponseDto<List<ZabbixItemDetailDto>>(
                    "Lấy thông tin tài nguyên thành công",
                    HttpStatusCodeEnum.OK,
                    result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy tất cả tài nguyên cho host {HostId}", hostId);
                return new FailureApiResponseDto(
                    "Lỗi hệ thống khi lấy tài nguyên",
                    HttpStatusCodeEnum.InternalServerError,
                    new ErrorDto(HttpStatusCodeEnum.InternalServerError, ex.Message, ErrorType.InternalServerError));
            }
        }
    }
}