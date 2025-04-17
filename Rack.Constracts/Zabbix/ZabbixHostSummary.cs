namespace Rack.Contracts.Zabbix
{
    // Record cơ sở
    public record ZabbixHostSummary(
        string HostId,
        string Name,
        string Status,
        string Availability
    );
}