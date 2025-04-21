namespace Rack.Application.Commons.DTOs.Zabbix
{
    public record ZabbixHostDetail(
        string HostId,
        string Name,
        string Status,
        string Availability
        ) : ZabbixHostSummary(HostId, Name, Status, Availability)
    {
        public string? Description { get; init; }
        public string? IpAddress { get; init; }
        public List<ZabbixProblemInfo>? RecentProblems { get; init; }
        public Dictionary<string, string>? Resources { get; init; }
        public List<string>? GroupNames { get; init; }
        public List<string>? TemplateNames { get; init; }
    }
}