using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Rack.Application.Commons.DTOs.Zabbix;
using Rack.Application.Commons.Interfaces;
using Rack.MainInfrastructure.Services.Monitoring;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Services;

public sealed class ZabbixService : IZabbixService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string _apiToken;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

    public ZabbixService(HttpClient httpClient, IConfiguration config, IMemoryCache memoryCache)
    {
        _httpClient = httpClient;
        _apiUrl = config["Zabbix:ApiUrl"]!;
        _apiToken = config["Zabbix:ApiToken"]!;
        _cache = memoryCache;
    }

    #region Host List Methods

    public async Task<IEnumerable<ZabbixHostSummary>> GetMonitoredHostsAsync(CancellationToken cancellationToken = default)
    {
        string cacheKey = "zabbix_all_monitored_hosts";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<ZabbixHostSummary> cachedHosts))
        {
            Console.WriteLine("[GetMonitoredHostsAsync] Returning cached hosts");
            return cachedHosts;
        }

        var hostParams = new
        {
            output = new[] { "hostid", "name", "status", "available", "proxy_hostid" },
            selectTags = "extend",
            selectInterfaces = "extend",
            filter = new { status = "0" }
        };

        Console.WriteLine("[GetMonitoredHostsAsync] Requesting host info with interfaces and tags...");
        var hosts = await ZabbixApiHelper.SendRequestAsync<List<ZabbixHostSummary>>(
            _httpClient, _apiUrl, _apiToken, "host.get", hostParams, cancellationToken);

        if (hosts == null || !hosts.Any()) return Enumerable.Empty<ZabbixHostSummary>();

        Console.WriteLine($"[GetMonitoredHostsAsync] Found {hosts.Count} hosts.");

        // Process hosts
        foreach (var host in hosts)
        {
            ProcessHostSummary(host);
        }

        // Cache results
        _cache.Set(cacheKey, hosts, _cacheTimeout);

        return hosts;
    }

    public async Task<IEnumerable<ZabbixHostSummary>> GetHostsPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? searchTerm = null,
        string? deviceType = null,
        CancellationToken cancellationToken = default)
    {
        var filter = new Dictionary<string, object> { { "status", "0" } };

        // Build parameters
        var hostParams = new
        {
            output = new[] { "hostid", "name", "status", "available", "proxy_hostid" },
            selectTags = "extend",
            selectInterfaces = new[] { "ip", "type", "main" }, // Only needed interface fields
            filter,
            search = !string.IsNullOrEmpty(searchTerm) ? new { name = searchTerm } : null,
            limit = pageSize,
            offset = (page - 1) * pageSize
        };

        Console.WriteLine($"[GetHostsPagedAsync] Requesting paged hosts (page {page}, size {pageSize})...");
        var hosts = await ZabbixApiHelper.SendRequestAsync<List<ZabbixHostSummary>>(
            _httpClient, _apiUrl, _apiToken, "host.get", hostParams, cancellationToken);

        if (hosts == null || !hosts.Any()) return Enumerable.Empty<ZabbixHostSummary>();

        Console.WriteLine($"[GetHostsPagedAsync] Found {hosts.Count} hosts for page {page}.");

        // Process hosts
        foreach (var host in hosts)
        {
            ProcessHostSummary(host);
        }

        // Filter by device type if needed (client-side filter since it's derived from tags)
        if (!string.IsNullOrEmpty(deviceType))
        {
            hosts = hosts.Where(h => h.DeviceType == deviceType).ToList();
        }

        return hosts;
    }

    // Helper method to process host common details
    private void ProcessHostSummary(ZabbixHostSummary host)
    {
        // Process DeviceType
        var deviceTag = host.Tags?.FirstOrDefault(t => t.Tag == "deviceType");
        host.DeviceType = deviceTag?.Value ?? "Unknown";

        // Process IpAddress
        string? ipAddress = null;
        if (host.Interfaces != null && host.Interfaces.Any())
        {
            var mainAgentInterface = host.Interfaces.FirstOrDefault(i => i.Type == "1" && i.Main == "1");
            var mainSnmpInterface = host.Interfaces.FirstOrDefault(i => i.Type == "2" && i.Main == "1");
            var firstValidInterface = host.Interfaces.FirstOrDefault(i =>
                !string.IsNullOrEmpty(i.Ip) && i.Ip != "0.0.0.0" && i.Ip != "127.0.0.1");
            ipAddress = mainAgentInterface?.Ip ?? mainSnmpInterface?.Ip ?? firstValidInterface?.Ip;
        }
        host.IpAddress = ipAddress;
    }

    #endregion Host List Methods

    #region Host Detail Methods

    public async Task<ZabbixHostDetail?> GetHostBasicDetailsAsync(string hostId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"zabbix_host_basic_{hostId}";
        if (_cache.TryGetValue(cacheKey, out ZabbixHostDetail? cachedHostDetail))
        {
            Console.WriteLine($"[GetHostBasicDetailsAsync] Returning cached basic details for host {hostId}");
            return cachedHostDetail;
        }

        var parameters = new
        {
            output = new[] { "hostid", "name", "status", "available", "description", "host" },
            hostids = new[] { hostId },
            selectGroups = new[] { "groupid", "name" },
            selectTemplates = new[] { "templateid", "name" },
            selectTags = "extend",
            selectInterfaces = new[] { "interfaceid", "ip", "type", "main", "port", "dns" }
        };

        Console.WriteLine($"[GetHostBasicDetailsAsync] Getting basic details for hostId: {hostId}");
        var hosts = await ZabbixApiHelper.SendRequestAsync<List<ZabbixHostDetail>>(
            _httpClient, _apiUrl, _apiToken, "host.get", parameters, cancellationToken);

        var hostDetail = hosts?.FirstOrDefault();
        if (hostDetail != null)
        {
            ProcessHostSummary(hostDetail);
            _cache.Set(cacheKey, hostDetail, _cacheTimeout);
        }
        else
        {
            Console.WriteLine($"[GetHostBasicDetailsAsync] Host with ID {hostId} not found.");
        }

        return hostDetail;
    }

    public async Task<ZabbixHostDetail?> GetHostFullDetailsAsync(
        string hostId,
        bool includeProblems = true,
        bool includeResources = true,
        int problemLimit = 5,
        CancellationToken cancellationToken = default)
    {
        // First get basic details
        var hostDetail = await GetHostBasicDetailsAsync(hostId, cancellationToken);
        if (hostDetail == null) return null;

        // Create task list
        var tasks = new List<Task>();
        Task<IEnumerable<ZabbixProblemInfo>>? problemsTask = null;
        Task<List<ZabbixItemDetailDto>?>? resourcesTask = null;

        // Add problems task if needed
        if (includeProblems)
        {
            problemsTask = GetRecentHostProblemsAsync(hostId, problemLimit, cancellationToken);
            tasks.Add(problemsTask);
        }

        // Add resources task if needed
        if (includeResources)
        {
            // Determine relevant item keys based on device type (if any)
            var itemKeys = !string.IsNullOrEmpty(hostDetail.DeviceType) && hostDetail.DeviceType != "Unknown"
                ? GetItemKeysByDeviceType(hostDetail.DeviceType)
                : Enumerable.Empty<string>();

            resourcesTask = GetHostResourceItemsAsync(hostId, itemKeys, cancellationToken);
            tasks.Add(resourcesTask);
        }

        // Run all tasks in parallel
        if (tasks.Any())
        {
            Console.WriteLine($"[GetHostFullDetailsAsync] Running {tasks.Count} parallel tasks for host {hostId}...");
            await Task.WhenAll(tasks);

            // Assign results
            if (problemsTask != null)
            {
                hostDetail.RecentProblems = (await problemsTask).ToList();
            }

            if (resourcesTask != null)
            {
                hostDetail.Resources = await resourcesTask;
            }
        }

        return hostDetail;
    }

    #endregion Host Detail Methods

    #region Resource Methods

    public async Task<List<ZabbixItemDetailDto>?> GetHostResourceItemsAsync(
        string hostId,
        IEnumerable<string>? itemKeys = null,
        CancellationToken cancellationToken = default)
    {
        // Generate cache key based on host and item keys
        string keyList = itemKeys != null && itemKeys.Any()
            ? string.Join(",", itemKeys)
            : "all";
        string cacheKey = $"zabbix_host_resources_{hostId}_{keyList}";

        if (_cache.TryGetValue(cacheKey, out List<ZabbixItemDetailDto>? cachedItems))
        {
            Console.WriteLine($"[GetHostResourceItemsAsync] Returning cached items for host {hostId}");
            return cachedItems;
        }

        var parameters = new
        {
            output = new[] { "itemid", "key_", "name", "lastvalue" },
            hostids = new[] { hostId },
            selectTags = "extend",
            filter = (itemKeys != null && itemKeys.Any()) ? new { key_ = itemKeys.ToArray() } : null,
            webitems = true
        };

        string logMsg = (itemKeys != null && itemKeys.Any())
            ? $"Requesting SPECIFIC items with details and tags for host {hostId}..."
            : $"Requesting ALL items with details and tags for host {hostId}...";
        Console.WriteLine($"[GetHostResourceItemsAsync] {logMsg}");

        var items = await ZabbixApiHelper.SendRequestAsync<List<ZabbixItemDetailDto>>(
           _httpClient, _apiUrl, _apiToken, "item.get", parameters, cancellationToken);

        if (items != null)
        {
            _cache.Set(cacheKey, items, _cacheTimeout);
            Console.WriteLine($"[GetHostResourceItemsAsync] Returning {items.Count} detailed items for host {hostId}.");
            return items;
        }

        Console.WriteLine($"[GetHostResourceItemsAsync] No items returned or error occurred for host {hostId}.");
        return new List<ZabbixItemDetailDto>();
    }

    public async Task<List<ZabbixItemDetailDto>?> GetHostResourcesPagedAsync(
        string hostId,
        int page = 1,
        int pageSize = 20,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new
        {
            output = new[] { "itemid", "key_", "name", "lastvalue" },
            hostids = new[] { hostId },
            selectTags = "extend",
            search = !string.IsNullOrEmpty(searchTerm) ? new { name = searchTerm, key_ = searchTerm } : null,
            searchByAny = !string.IsNullOrEmpty(searchTerm),
            limit = pageSize,
            offset = (page - 1) * pageSize,
            webitems = true,
            sortfield = new[] { "name" },
            sortorder = "ASC"
        };

        Console.WriteLine($"[GetHostResourcesPagedAsync] Requesting paged resources for host {hostId} (page {page}, size {pageSize})");

        var items = await ZabbixApiHelper.SendRequestAsync<List<ZabbixItemDetailDto>>(
           _httpClient, _apiUrl, _apiToken, "item.get", parameters, cancellationToken);

        return items ?? new List<ZabbixItemDetailDto>();
    }

    #endregion Resource Methods

    #region Problem Methods

    public async Task<IEnumerable<ZabbixProblemInfo>> GetRecentProblemsAsync(
        int severityThreshold = 0,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        string cacheKey = $"zabbix_recent_problems_sev{severityThreshold}_limit{limit}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<ZabbixProblemInfo> cachedProblems))
        {
            Console.WriteLine($"[GetRecentProblemsAsync] Returning cached problems");
            return cachedProblems;
        }

        var parameters = new
        {
            output = new[] { "eventid", "name", "severity", "clock", "acknowledged" },
            recent = "true",
            sortfield = "eventid",
            sortorder = "DESC",
            limit = limit,
            severities = severityThreshold > 0
                ? Enumerable.Range(severityThreshold, 6 - severityThreshold).Select(s => s.ToString()).ToArray()
                : null
        };

        Console.WriteLine($"[GetRecentProblemsAsync] Requesting recent problems with severity >= {severityThreshold}, limit {limit}");

        var problems = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixProblemInfo>>(
            _httpClient, _apiUrl, _apiToken, "event.get", parameters, cancellationToken);

        if (problems != null)
        {
            _cache.Set(cacheKey, problems, TimeSpan.FromMinutes(1)); // Short cache for problems
        }

        return problems ?? Enumerable.Empty<ZabbixProblemInfo>();
    }

    public async Task<IEnumerable<ZabbixProblemInfo>> GetRecentHostProblemsAsync(
        string hostId,
        int limit = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(hostId)) return Enumerable.Empty<ZabbixProblemInfo>();

        string cacheKey = $"zabbix_host_{hostId}_problems_limit{limit}";
        if (_cache.TryGetValue(cacheKey, out IEnumerable<ZabbixProblemInfo> cachedProblems))
        {
            Console.WriteLine($"[GetRecentHostProblemsAsync] Returning cached problems for host {hostId}");
            return cachedProblems;
        }

        var parameters = new
        {
            output = new[] { "eventid", "name", "severity", "clock", "acknowledged" },
            hostids = new[] { hostId },
            sortfield = "eventid",
            sortorder = "DESC",
            limit = limit
        };

        Console.WriteLine($"[GetRecentHostProblemsAsync] Requesting recent problems for host {hostId}");
        var problems = await ZabbixApiHelper.SendRequestAsync<IEnumerable<ZabbixProblemInfo>>(
            _httpClient, _apiUrl, _apiToken, "problem.get", parameters, cancellationToken);

        if (problems != null)
        {
            _cache.Set(cacheKey, problems, TimeSpan.FromMinutes(1)); // Short cache for problems
        }

        return problems ?? Enumerable.Empty<ZabbixProblemInfo>();
    }

    #endregion Problem Methods

    #region Helper Methods

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

    #endregion Helper Methods
}