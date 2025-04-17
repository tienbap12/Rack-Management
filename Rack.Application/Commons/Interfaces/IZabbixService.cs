using Rack.Contracts.Zabbix;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rack.Application.Commons.Interfaces;

// Interface cho Zabbix Service
public interface IZabbixService
{
    Task<IEnumerable<ZabbixHostSummary>> GetMonitoredHostsAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<ZabbixProblemInfo>> GetRecentProblemsAsync(int severityThreshold = 0, int limit = 100, CancellationToken cancellationToken = default);

    Task<ZabbixHostDetail?> GetHostDetailsAsync(string hostId, CancellationToken cancellationToken = default);

    Task<IEnumerable<ZabbixProblemInfo>> GetProblemsByHostAsync(string hostId, CancellationToken cancellationToken = default);

    Task<Dictionary<string, string>> GetHostResourceItemsAsync(string hostId, IEnumerable<string> itemKeys, CancellationToken cancellationToken = default);
}

// Converter từ "0"/"1" hoặc chuỗi "true"/"false" thành bool