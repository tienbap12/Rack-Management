using System.Text.Json.Serialization;

namespace Rack.Application.Commons.DTOs.Zabbix
{
    public class ZabbixHostDetail : ZabbixHostSummary // Kế thừa các trường cơ bản
    {
        // Thuộc tính map từ JSON của host.get
        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("host")] // Tên kỹ thuật của host
        public string? TechnicalName { get; set; }

        [JsonPropertyName("interfaces")] // Danh sách interfaces trả về từ host.get
        public List<ZabbixInterfaceDto>? Interfaces { get; set; }

        [JsonPropertyName("groups")] // Danh sách group objects
        public List<ZabbixGroupDto>? Groups { get; set; }

        [JsonPropertyName("templates")] // Danh sách template objects
        public List<ZabbixTemplateDto>? Templates { get; set; }

        // Các thuộc tính được tính toán/lấy từ API khác, không cần JsonPropertyName
        // IpAddress được kế thừa và sẽ được tính/gán giá trị
        public List<ZabbixProblemInfo>? RecentProblems { get; set; }

        public List<ZabbixItemDetailDto>? Resources { get; set; }

        // Bỏ GroupNames, TemplateNames vì đã có Groups, Templates (list objects)
        // public List<string>? GroupNames { get; set; }
        // public List<string>? TemplateNames { get; set; }
    }

    public class ZabbixGroupDto
    {
        [JsonPropertyName("groupid")]
        public string GroupId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    // --- DTO cho Template ---
    public class ZabbixTemplateDto
    {
        [JsonPropertyName("templateid")]
        public string TemplateId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}