using Rack.Application.Commons.DTOs.Zabbix;

namespace Rack.Application.Commons.Interfaces;

// Interface cho Zabbix Service
public interface IZabbixService
{
    // Host list methods
    Task<IEnumerable<ZabbixHostSummary>> GetMonitoredHostsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<ZabbixHostSummary>> GetHostsPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? searchTerm = null,
        string? deviceType = null,
        CancellationToken cancellationToken = default);

    // Host details methods
    Task<ZabbixHostDetail?> GetHostBasicDetailsAsync(string hostId, CancellationToken cancellationToken = default);

    Task<ZabbixHostDetail?> GetHostFullDetailsAsync(
        string hostId,
        bool includeProblems = true,
        bool includeResources = true,
        int problemLimit = 5,
        CancellationToken cancellationToken = default);

    // Resources methods
    Task<List<ZabbixItemDetailDto>?> GetHostResourceItemsAsync(
        string hostId,
        IEnumerable<string>? itemKeys = null,
        CancellationToken cancellationToken = default);

    Task<List<ZabbixItemDetailDto>?> GetHostResourcesPagedAsync(
        string hostId,
        int page = 1,
        int pageSize = 20,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    // Problem methods
    Task<IEnumerable<ZabbixProblemInfo>> GetRecentProblemsAsync(
        int severityThreshold = 0,
        int limit = 100,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ZabbixProblemInfo>> GetRecentHostProblemsAsync(
        string hostId,
        int limit = 5,
        CancellationToken cancellationToken = default);
}