namespace Rack.Application.Commons.DTOs.Zabbix
{
    // Record cơ sở
    public record ZabbixHostSummary(
        string HostId,
        string Name,
        string Status,
        string Availability
    );
}