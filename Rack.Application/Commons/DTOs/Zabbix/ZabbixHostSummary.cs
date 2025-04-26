// File: Rack.Application/Commons/DTOs/Zabbix/ZabbixDtos.cs (hoặc file riêng)
using System.Text.Json.Serialization; // Quan trọng: Dùng System.Text.Json

namespace Rack.Application.Commons.DTOs.Zabbix
{
    // --- DTO cho Interface (cần thiết để nhận dữ liệu từ API) ---
    public class ZabbixInterfaceDto // Đặt tên khác nếu muốn, ví dụ ZabbixHostInterface
    {
        [JsonPropertyName("hostid")] // Map từ JSON field "hostid"
        public string HostId { get; set; } = string.Empty;

        [JsonPropertyName("ip")] // Map từ JSON field "ip"
        public string Ip { get; set; } = string.Empty;

        [JsonPropertyName("main")] // Map từ JSON field "main" ("0" hoặc "1")
        public string Main { get; set; } = string.Empty;

        [JsonPropertyName("type")] // Map từ JSON field "type" ("1": Agent, "2": SNMP, ...)
        public string Type { get; set; } = string.Empty;

        // Thêm các trường khác nếu bạn select chúng, ví dụ: port, dns, useip
        [JsonPropertyName("port")]
        public string? Port { get; set; }

        [JsonPropertyName("dns")]
        public string? Dns { get; set; }
    }

    // --- DTO cho Tag (đảm bảo có sẵn) ---
    public class ZabbixTag
    {
        [JsonPropertyName("tag")] // Map từ JSON field "tag"
        public string Tag { get; set; } = string.Empty;

        [JsonPropertyName("value")] // Map từ JSON field "value"
        public string Value { get; set; } = string.Empty;
    }

    // --- CẬP NHẬT DTO ZabbixHostSummary ---
    public class ZabbixHostSummary
    {
        [JsonPropertyName("hostid")] // Map từ JSON field "hostid"
        public string HostId { get; set; } = string.Empty;

        [JsonPropertyName("name")] // Map từ JSON field "name"
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("status")] // Map từ JSON field "status"
        public string Status { get; set; } = string.Empty; // "0": Monitored, "1": Not monitored

        [JsonPropertyName("available")] // Map từ JSON field "available"
        public string Available { get; set; } = string.Empty; // "0": Unknown, "1": Available, "2": Unavailable

        [JsonPropertyName("proxy_hostid")] // Map từ JSON field "proxy_hostid"
        public string? ProxyHostId { get; set; } // Có thể là null hoặc "0"

        [JsonPropertyName("tags")] // Map từ JSON field "tags"
        public List<ZabbixTag>? Tags { get; set; } // Danh sách tags gốc

        // ===> THÊM PROPERTY NÀY để nhận Interfaces từ API <===
        [JsonPropertyName("interfaces")] // Map từ JSON field "interfaces"
        // JsonIgnore để không gửi trường List<Interface> này về frontend
        [JsonIgnore]
        public List<ZabbixInterfaceDto>? Interfaces { get; set; }

        // ===> Thuộc tính này sẽ được tính toán và gửi về frontend <===
        public string? IpAddress { get; set; } // IP được chọn lọc

        // ===> Thuộc tính này sẽ được tính toán và gửi về frontend <===
        public string? DeviceType { get; set; } // Type suy luận từ Tags
    }
}