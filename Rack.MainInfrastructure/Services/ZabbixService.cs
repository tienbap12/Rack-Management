using Microsoft.Extensions.Configuration;
using Rack.Application.Commons.Interfaces;
using Rack.Contracts.Zabbix;
using Rack.MainInfrastructure.Services.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

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
            output = new[] { "hostid", "name", "status", "available", "proxy_hostid" },
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
            sortfield = "eventid",
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
            var resources = await GetHostResourceItemsAsync(hostId, new[] { "dell.server.hw.cpu1.model[systemModelName]", "dell.server.hw.cpu2.model[systemModelName]", "dell.server.hw.model[systemModelName]", "dell.server.hw.diskarray.model[controllerName.1]" }, cancellationToken);
            hostDetail = hostDetail with { Resources = resources };
        }

        return hostDetail;
    }

    // Lấy danh sách các vấn đề của host cụ thể
    public async Task<IEnumerable<ZabbixProblemInfo>> GetProblemsByHostAsync(
    string hostId,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(hostId))
        {
            return Enumerable.Empty<ZabbixProblemInfo>();
        }

        // Use problem.get with hostids filter directly
        var parameters = new
        {
            // Specify the fields needed for ZabbixProblemInfo
            output = new[] {
                "eventid",
                "name",
                "severity",
                "clock",
                "acknowledged"
            },
            hostids = new[] { hostId },
            sortfield = "eventid", // Sort by time
            sortorder = "DESC" // Newest problems first
        };

        try
        {
            var problems = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixProblemInfo>>(
                _httpClient,
                _apiUrl,
                _apiToken,
                "problem.get", // Use the problem.get method
                parameters,
                cancellationToken
            );

            // Handle potential null return from helper, although returning empty list is preferred
            if (problems == null)
            {
                return Enumerable.Empty<ZabbixProblemInfo>();
            }

            return problems;
        }
        catch (Exception ex)
        {
            return Enumerable.Empty<ZabbixProblemInfo>(); // Safest default for IEnumerable
        }
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

    private async Task<IEnumerable<string>> GetTriggerIdsByHostAsync(
    string hostId,
    CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            output = new[] { "triggerid" },
            hostids = new[] { hostId }
        };

        var triggers = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixTriggerDto>>(
            _httpClient,
            _apiUrl,
            _apiToken,
            "trigger.get",
            parameters,
            cancellationToken
        );

        return triggers.Select(t => t.TriggerId);
    }
}

// DTO cho trigger
internal record ZabbixTriggerDto(
    [property: JsonPropertyName("triggerid")] string TriggerId
);
// DTO nội bộ để mapping thông tin Item từ API Zabbix
internal record ZabbixItemDto(
    [property: JsonPropertyName("itemid")] string ItemId,
    [property: JsonPropertyName("key_")] string Key,
    [property: JsonPropertyName("lastvalue")] string LastValue
);