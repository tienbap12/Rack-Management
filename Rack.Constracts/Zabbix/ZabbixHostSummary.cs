namespace Rack.Contracts.Zabbix
{
    public record ZabbixHostSummary(
    string HostId,
    string Name,
    string Status,
    string Availability
);
}