using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Rack.Application.Commons.Interfaces;
using Rack.MainInfrastructure.Services.Monitoring;

namespace Rack.MainInfrastructure.Services;

public sealed class ZabbixService : IZabbixService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string _apiToken;

    public ZabbixService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiUrl = config["Zabbix:ApiUrl"]!;
        _apiToken = config["Zabbix:ApiToken"]!;
    }

    // Lấy danh sách các host đang được giám sát
    public async Task<IEnumerable<ZabbixHostSummary>> GetMonitoredHostsAsync(CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            output = new[] { "hostid", "name", "status", "available" },
            filter = new
            {
                status = "0" // Giả sử "0" là host đang hoạt động (theo cấu hình Zabbix)
            }
        };

        var hosts = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixHostSummary>>(
            _httpClient,
            _apiUrl,
            _apiToken,
            "host.get",
            parameters,
            cancellationToken
        );

        return hosts;
    }

    // Lấy danh sách các vấn đề (problems) gần đây dựa theo ngưỡng severity và giới hạn số lượng
    public async Task<IEnumerable<ZabbixProblemInfo>> GetRecentProblemsAsync(int severityThreshold = 0, int limit = 100, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            output = "extend",
            sortfield = "eventid",  // Sửa lại thành "eventid" theo yêu cầu của API
            sortorder = "DESC",
            limit,
            filter = new
            {
                severity = severityThreshold.ToString()
            }
        };

        return await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixProblemInfo>>(
            _httpClient,
            _apiUrl,
            _apiToken,
            "problem.get",
            parameters,
            cancellationToken
        );
    }

    // Lấy chi tiết của một host dựa theo hostId
    public async Task<ZabbixHostDetail?> GetHostDetailsAsync(string hostId, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            output = "extend",
            hostids = new[] { hostId },
            selectGroups = new[] { "name" },
            selectTemplates = new[] { "name" },
            selectInterfaces = new[] { "ip" },
            // Nếu cần lấy thêm các item hoặc thông tin khác thì thêm các field tương ứng
        };

        // API host.get trả về một mảng
        var hosts = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixHostDetail>>(
            _httpClient,
            _apiUrl,
            _apiToken,
            "host.get",
            parameters,
            cancellationToken
        );

        var hostDetail = hosts.FirstOrDefault();
        if (hostDetail != null)
        {
            // Lấy thông tin vấn đề cho host và cập nhật vào RecentProblems
            var problems = await GetProblemsByHostAsync(hostId, cancellationToken);
            hostDetail = hostDetail with { RecentProblems = problems.ToList() };

            // Lấy thông tin các resource items cho host nếu cần
            var resources = await GetHostResourceItemsAsync(hostId, new[] { "system.cpu.load", "system.mem.free" }, cancellationToken);
            hostDetail = hostDetail with { Resources = resources };
        }

        return hostDetail;
    }

    // Lấy danh sách các vấn đề của host cụ thể
    public async Task<IEnumerable<ZabbixProblemInfo>> GetProblemsByHostAsync(
string hostId,
CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            output = "extend",
            hostids = new[] { hostId },
            sortfield = "eventid",
            sortorder = "DESC"
        };

        return await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixProblemInfo>>(
            _httpClient,
            _apiUrl,
            _apiToken,
            "problem.get",
            parameters,
            cancellationToken
        );
    }

    // Lấy giá trị của các item cụ thể theo key cho host
    public async Task<Dictionary<string, string>> GetHostResourceItemsAsync(string hostId, IEnumerable<string> itemKeys, CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            output = new[] { "itemid", "key_", "lastvalue" },
            hostids = new[] { hostId },
            filter = new
            {
                key_ = itemKeys.ToArray()
            }
        };

        // Sử dụng DTO nội bộ để map các trường item từ Zabbix
        var items = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixItemDto>>(
            _httpClient,
            _apiUrl,
            _apiToken,
            "item.get",
            parameters,
            cancellationToken
        );

        return items.ToDictionary(item => item.Key, item => item.LastValue);
    }
}

// DTO nội bộ để mapping thông tin Item từ API Zabbix
internal record ZabbixItemDto(
    [property: JsonPropertyName("itemid")] string ItemId,
    [property: JsonPropertyName("key_")] string Key,
    [property: JsonPropertyName("lastvalue")] string LastValue
);