using System.Text.Json.Serialization;

namespace Rack.Application.Commons.DTOs.Zabbix
{
    public class ZabbixItemDetailDto
    {
        [JsonPropertyName("itemid")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("key_")] // JSON trả về key_
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("name")] // Tên item dễ đọc
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("lastvalue")]
        public string? LastValue { get; set; } // Giá trị có thể null

        [JsonPropertyName("tags")]
        public List<ZabbixTag>? Tags { get; set; } // Danh sách tags của item này
    }
}