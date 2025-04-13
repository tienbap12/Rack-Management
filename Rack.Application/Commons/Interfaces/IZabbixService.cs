using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rack.Application.Commons.Interfaces;

// Record cơ sở
public record ZabbixHostSummary(
    string HostId,
    string Name,
    string Status,
    string Availability
);

// Record cho problem
public record ZabbixProblemInfo
{
    [JsonPropertyName("eventid")]
    public string EventId { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }

    [JsonPropertyName("severity")]
    public string Severity { get; init; }

    [JsonPropertyName("clock")]
    [JsonConverter(typeof(UnixDateTimeConverter))]
    public DateTime Clock { get; init; }

    [JsonPropertyName("acknowledged")]
    [JsonConverter(typeof(StringToBooleanConverter))]
    public bool Acknowledged { get; init; }
}

// Record chi tiết, kế thừa ZabbixHostSummary
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
public class StringToBooleanConverter : JsonConverter<bool>
{
    public override bool Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Xử lý cả số và chuỗi
        return reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt32() != 0,
            JsonTokenType.String => reader.GetString()! switch
            {
                "1" => true,
                "0" => false,
                _ => throw new JsonException($"Invalid boolean string: {reader.GetString()}")
            },
            _ => throw new JsonException($"Unexpected token type: {reader.TokenType}")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        bool value,
        JsonSerializerOptions options)
        => writer.WriteStringValue(value ? "1" : "0");
}

public class UnixDateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Xử lý cả số và chuỗi chứa số
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString()!;
            if (long.TryParse(stringValue, out var unixTime))
            {
                return DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            return DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64()).UtcDateTime;
        }

        throw new JsonException($"Invalid Unix timestamp format: {reader.TokenType}");
    }

    public override void Write(
        Utf8JsonWriter writer,
        DateTime value,
        JsonSerializerOptions options)
        => writer.WriteNumberValue(new DateTimeOffset(value).ToUnixTimeSeconds());
}