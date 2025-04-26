using Microsoft.Extensions.Configuration;
using Rack.Application.Commons.DTOs.Zabbix;
using Rack.Application.Commons.Interfaces;
using Rack.MainInfrastructure.Services.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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

    public async Task<IEnumerable<ZabbixHostSummary>> GetMonitoredHostsAsync(CancellationToken cancellationToken = default)
    {
        // Bước 1: Lấy host cơ bản
        var hostParams = new
        {
            output = new[] { "hostid", "name", "status", "available", "proxy_hostid" },
            selectTags = "extend",
            filter = new { status = "0" }
        };
        Console.WriteLine("[GetMonitoredHostsAsync] Step 1: Requesting basic host info...");
        var hosts = await ZabbixApiHelper.SendRequestAsync<List<ZabbixHostSummary>>(
            _httpClient, _apiUrl, _apiToken, "host.get", hostParams, cancellationToken);

        if (hosts == null || !hosts.Any()) return Enumerable.Empty<ZabbixHostSummary>();
        Console.WriteLine($"[GetMonitoredHostsAsync] Step 1: Found {hosts.Count} hosts.");

        var hostIds = hosts.Select(h => h.HostId).ToArray(); // Sửa: Dùng HostId (PascalCase)

        // Bước 2: Lấy interfaces
        var interfaceParams = new { output = "extend", hostids = hostIds };
        Console.WriteLine("[GetMonitoredHostsAsync] Step 2: Requesting interfaces...");
        var allInterfaces = await ZabbixApiHelper.SendRequestAsync<List<ZabbixInterfaceDto>>(
            _httpClient, _apiUrl, _apiToken, "hostinterface.get", interfaceParams, cancellationToken);

        var interfacesByHostId = allInterfaces?
            .GroupBy(i => i.HostId) // Sửa: Dùng HostId (PascalCase)
            .ToDictionary(g => g.Key, g => g.ToList())
            ?? new Dictionary<string, List<ZabbixInterfaceDto>>();
        Console.WriteLine($"[GetMonitoredHostsAsync] Step 2: Grouped {interfacesByHostId.Count} hosts interfaces.");

        // Bước 3: Map dữ liệu
        Console.WriteLine("[GetMonitoredHostsAsync] Step 3: Processing...");
        foreach (var host in hosts)
        {
            // Sửa: Dùng Name, HostId (PascalCase)
            Console.WriteLine($"[GetMonitoredHostsAsync] - Processing host: {host.Name} ({host.HostId})");
            // Sửa: Dùng Tags, Tag, Value (PascalCase)
            var deviceTag = host.Tags?.FirstOrDefault(t => t.Tag == "deviceType");
            host.DeviceType = deviceTag?.Value ?? "Unknown";
            Console.WriteLine($"[GetMonitoredHostsAsync] -   DeviceType: {host.DeviceType}");

            string? ipAddress = null;
            // Sửa: Dùng HostId (PascalCase)
            if (interfacesByHostId.TryGetValue(host.HostId, out var hostInterfaces) && hostInterfaces.Any())
            {
                // Sửa: Dùng Type, Main, Ip (PascalCase)
                var mainAgentInterface = hostInterfaces.FirstOrDefault(i => i.Type == "1" && i.Main == "1");
                var mainSnmpInterface = hostInterfaces.FirstOrDefault(i => i.Type == "2" && i.Main == "1");
                var firstValidInterface = hostInterfaces.FirstOrDefault(i => !string.IsNullOrEmpty(i.Ip) && i.Ip != "0.0.0.0" && i.Ip != "127.0.0.1");
                ipAddress = mainAgentInterface?.Ip ?? mainSnmpInterface?.Ip ?? firstValidInterface?.Ip;
                Console.WriteLine($"[GetMonitoredHostsAsync] -   Selected IP: {ipAddress ?? "None suitable"}");
            }
            else
            {
                Console.WriteLine($"[GetMonitoredHostsAsync] -   No interfaces found for this host via hostinterface.get.");
            }
            // Gán vào thuộc tính PascalCase
            host.IpAddress = ipAddress;
        }
        Console.WriteLine("[GetMonitoredHostsAsync] Step 3: Finished.");
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

    public async Task<ZabbixHostDetail?> GetHostDetailsAsync(string hostId, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[GetHostDetailsAsync] Getting details for hostId: {hostId}");
        // Yêu cầu các trường JSON gốc
        var parameters = new
        {
            output = "extend", // Lấy nhiều trường hơn
            hostids = new[] { hostId },
            selectGroups = "extend", // Lấy object group
            selectTemplates = "extend", // Lấy object template
            selectInterfaces = "extend", // Lấy object interface
            selectTags = "extend" // Lấy object tag
        };

        // Deserialize vào DTO ZabbixHostDetail (đã cập nhật)
        var hosts = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixHostDetail>>(
            _httpClient, _apiUrl, _apiToken, "host.get", parameters, cancellationToken);

        var hostDetail = hosts?.FirstOrDefault();

        if (hostDetail != null)
        {
            // Xử lý DeviceType từ Tags (PascalCase)
            var deviceTag = hostDetail.Tags?.FirstOrDefault(t => t.Tag == "deviceType");
            hostDetail.DeviceType = deviceTag?.Value ?? "Unknown";
            Console.WriteLine($"[GetHostDetailsAsync] - DeviceType: {hostDetail.DeviceType}");

            // Xử lý IpAddress từ Interfaces (PascalCase)
            string? detailIpAddress = null;
            if (hostDetail.Interfaces != null && hostDetail.Interfaces.Any())
            {
                var mainAgentInterface = hostDetail.Interfaces.FirstOrDefault(i => i.Type == "1" && i.Main == "1");
                var mainSnmpInterface = hostDetail.Interfaces.FirstOrDefault(i => i.Type == "2" && i.Main == "1");
                var firstValidInterface = hostDetail.Interfaces.FirstOrDefault(i => !string.IsNullOrEmpty(i.Ip) && i.Ip != "0.0.0.0" && i.Ip != "127.0.0.1");
                detailIpAddress = mainAgentInterface?.Ip ?? mainSnmpInterface?.Ip ?? firstValidInterface?.Ip;
                Console.WriteLine($"[GetHostDetailsAsync] - Determined Detail IP: {detailIpAddress ?? "N/A"}");
            }
            // Gán vào thuộc tính PascalCase
            hostDetail.IpAddress = detailIpAddress;

            // Lấy Problems và Resources (giả sử backend lấy hết resources)
            var problemsTask = GetProblemsByHostAsync(hostId, cancellationToken);
            var resourcesTask = GetHostResourceItemsAsync(hostId, Enumerable.Empty<string>(), cancellationToken); // Lấy hết items
            Console.WriteLine($"[GetHostDetailsAsync] - Requesting problems and all resource items...");

            await Task.WhenAll(problemsTask, resourcesTask);

            // Gán vào thuộc tính PascalCase
            hostDetail.RecentProblems = (await problemsTask).ToList();
            hostDetail.Resources = await resourcesTask;
            Console.WriteLine($"[GetHostDetailsAsync] - Fetched {hostDetail.RecentProblems.Count} problems and {hostDetail.Resources.Count} resource items.");
        }
        else
        {
            Console.WriteLine($"[GetHostDetailsAsync] Host with ID {hostId} not found.");
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
        if (itemKeys == null || !itemKeys.Any())
        {
            Console.WriteLine($"[GetHostResourceItemsAsync] itemKeys is empty for host {hostId}. Requesting ALL items.");
            var parametersAll = new { output = "extend", hostids = new[] { hostId } }; // extend lấy itemid, key_, lastvalue...
            var allItems = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixItemDto>>( // Dùng DTO internal đã sửa
               _httpClient, _apiUrl, _apiToken, "item.get", parametersAll, cancellationToken);

            if (allItems == null) return new Dictionary<string, string>();
            // Sửa: Dùng Key, LastValue (PascalCase)
            var allResultDictionary = allItems.ToDictionary(item => item.Key, item => item.LastValue ?? string.Empty);
            Console.WriteLine($"[GetHostResourceItemsAsync] Returning {allResultDictionary.Count} items (all) for host {hostId}.");
            return allResultDictionary;

        }
        else
        {
            var parametersSpecific = new { output = "extend", hostids = new[] { hostId }, filter = new { key_ = itemKeys.ToArray() } };
            Console.WriteLine($"[GetHostResourceItemsAsync] Requesting items for host {hostId} with specific keys...");
            var items = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixItemDto>>( // Dùng DTO internal đã sửa
               _httpClient, _apiUrl, _apiToken, "item.get", parametersSpecific, cancellationToken);

            if (items == null) return new Dictionary<string, string>();
            // Sửa: Dùng Key, LastValue (PascalCase)
            var resultDictionary = items.ToDictionary(item => item.Key, item => item.LastValue ?? string.Empty);
            Console.WriteLine($"[GetHostResourceItemsAsync] Returning {resultDictionary.Count} specific items for host {hostId}.");
            return resultDictionary;
        }
    }
    public async Task<string> GetDeviceTypeAsync(string hostId, CancellationToken cancellationToken)
    {
        var parameters = new
        {
            output = "extend", // Hoặc output = new[] { "hostid" } nếu chỉ cần tags
            hostids = new[] { hostId },
            selectTags = "extend" // Đảm bảo tham số này đúng
        };

        // ---- THÊM LOGGING Ở ĐÂY ----
        Console.WriteLine($"GetDeviceTypeAsync - HostId: {hostId} - Requesting parameters: {JsonSerializer.Serialize(parameters)}");
        // Hoặc sử dụng ILogger nếu bạn đã cấu hình logging

        var hosts = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixHostTagDto>>(
            _httpClient,
            _apiUrl,
            _apiToken,
            "host.get",
            parameters,
            cancellationToken
        );

        // ---- THÊM LOGGING KẾT QUẢ Ở ĐÂY ----
        Console.WriteLine($"GetDeviceTypeAsync - HostId: {hostId} - Received hosts DTO: {JsonSerializer.Serialize(hosts)}");

        var host = hosts?.FirstOrDefault(); // Thêm ?. để an toàn
        if (host?.Tags != null) // host và Tags đều không null
        {
            Console.WriteLine($"GetDeviceTypeAsync - HostId: {hostId} - Found tags: {JsonSerializer.Serialize(host.Tags)}");
            var deviceTag = host.Tags.FirstOrDefault(t => t.Tag == "deviceType");
            if (deviceTag != null)
            {
                Console.WriteLine($"GetDeviceTypeAsync - HostId: {hostId} - Found deviceType tag: {deviceTag.Value}");
                return deviceTag.Value;
            }
            else
            {
                Console.WriteLine($"GetDeviceTypeAsync - HostId: {hostId} - 'deviceType' tag not found in the list.");
            }
        }
        else
        {
            Console.WriteLine($"GetDeviceTypeAsync - HostId: {hostId} - Host or Tags property was null.");
        }

        Console.WriteLine($"GetDeviceTypeAsync - HostId: {hostId} - Returning 'Unknown'.");
        return "Unknown";
    }

    private IEnumerable<string> GetItemKeysByDeviceType(string deviceType)
    {
        return deviceType switch
        {
            "Switch" => new[]
            {
            "net.if.in[ifname]",
            "net.if.out[ifname]",
            "ifOperStatus[ifname]",
            "system.cpu.util",
            "vm.memory.size[used]"
        },
            "Router" => new[]
            {
            "net.if.in[wan0]",
            "net.if.out[wan0]",
            "icmpping",
            "system.cpu.util",
            "vm.memory.size[used]"
        },
            "Firewall" => new[]
            {
            "fw.sessions.current",
            "system.cpu.util",
            "vm.memory.size[used]",
            "sensor.temp[xxx]"
        },
            "Server" => new[]
            {
            "system.cpu.util[,idle]",
            "system.cpu.load",
            "vm.memory.size[used]",
            "vm.memory.size[available]",
            "vfs.fs.size[/,used]",
            "vfs.fs.size[/,free]",
            "net.if.in[eth0]",
            "net.if.out[eth0]"
        },
            _ => Array.Empty<string>()
        };
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

internal record ZabbixHostTagDto(
    [property: JsonPropertyName("tags")] List<ZabbixTagDto> Tags
);

internal record ZabbixTagDto(
    [property: JsonPropertyName("tag")] string Tag,
    [property: JsonPropertyName("value")] string Value
);