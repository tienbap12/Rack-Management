using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rack.Contracts.Zabbix
{
    public record ZabbixProblemInfo
    {
        [JsonPropertyName("eventid")]
        public string EventId { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("severity")]
        public string Severity { get; init; } = string.Empty;

        [JsonPropertyName("clock")]
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Clock { get; init; }

        [JsonPropertyName("acknowledged")]
        [JsonConverter(typeof(StringToBooleanConverter))]
        public bool Acknowledged { get; init; }
    }

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
}