using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Services.Monitoring;

internal static class ZabbixApiHelper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Cấu trúc request
    private record ZabbixRequest(
    [property: JsonPropertyName("method")] string Method,
    [property: JsonPropertyName("params")] object Params,
    [property: JsonPropertyName("auth")] string Auth,
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("jsonrpc")] string JsonRpc = "2.0"
);

    // Cấu trúc response chung (có thể có result hoặc error)
    private record ZabbixResponse<TResult>(
        [property: JsonPropertyName("jsonrpc")] string JsonRpc,
        [property: JsonPropertyName("result")] TResult? Result,
        [property: JsonPropertyName("error")] ZabbixError? Error,
        [property: JsonPropertyName("id")] long Id
    );

    // Cấu trúc lỗi Zabbix
    private record ZabbixError(
        [property: JsonPropertyName("code")] int Code,
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("data")] string Data
    );

    // Phương thức chung để gửi request và xử lý response
    public static async Task<TResult> SendRequestAsync<TResult>(
        HttpClient httpClient,
        string apiUrl,
        string apiToken,
        string method,
        object parameters, // Dùng object để linh hoạt params
        CancellationToken cancellationToken = default)
    {
        var requestId = DateTime.UtcNow.Ticks; // Hoặc dùng số tăng dần
        var requestPayload = new ZabbixRequest(
            Method: method,
            Params: parameters,
            Auth: apiToken,
            Id: requestId
        );

        HttpResponseMessage response = null!;
        try
        {
            // Sử dụng PostAsJsonAsync để tự động serialize
            response = await httpClient.PostAsJsonAsync(apiUrl, requestPayload, _jsonOptions, cancellationToken);

            // Đọc nội dung response để debug nếu cần
            // string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            // Console.WriteLine($"Zabbix Response for {method}: {responseContent}"); // Debug log

            response.EnsureSuccessStatusCode(); // Ném lỗi nếu status code không phải 2xx

            // Deserialize response JSON
            // Sử dụng ReadFromJsonAsync để tự động deserialize
            var zabbixResponse = await response.Content.ReadFromJsonAsync<ZabbixResponse<TResult>>(_jsonOptions, cancellationToken);

            if (zabbixResponse == null)
            {
                throw new ZabbixApiException("Failed to deserialize Zabbix response.");
            }

            if (zabbixResponse.Error != null)
            {
                throw new ZabbixApiException(
                    $"Zabbix API error (Code: {zabbixResponse.Error.Code}): {zabbixResponse.Error.Message} - {zabbixResponse.Error.Data}",
                    zabbixResponse.Error.Code,
                    zabbixResponse.Error.Data);
            }

            if (zabbixResponse.Result == null)
            {
                // Một số API trả về result rỗng khi thành công (ví dụ: update, delete)
                // Hoặc có thể là lỗi logic nếu API này bắt buộc phải có result
                Console.WriteLine($"WARN: Zabbix response for method {method} has null result.");
                // Trả về default nếu TResult là kiểu giá trị hoặc nullable, nếu không thì ném lỗi
                return default!;
            }

            return zabbixResponse.Result;
        }
        catch (HttpRequestException ex)
        {
            // Lỗi kết nối hoặc HTTP status code không thành công
            throw new ZabbixApiException($"HTTP request to Zabbix API failed: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            // Lỗi khi deserialize JSON
            throw new ZabbixApiException($"Failed to parse Zabbix API response: {ex.Message}", ex);
        }
        catch (Exception ex) // Bắt các lỗi khác
        {
            if (ex is ZabbixApiException) throw; // Ném lại lỗi ZabbixApiException đã bắt
            throw new ZabbixApiException($"An unexpected error occurred while calling Zabbix API method {method}: {ex.Message}", ex);
        }
    }
}

// Định nghĩa Exception tùy chỉnh cho lỗi Zabbix API
public class ZabbixApiException : Exception
{
    public int? ErrorCode { get; }
    public string? ErrorData { get; }

    public ZabbixApiException(string message) : base(message)
    {
    }

    public ZabbixApiException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ZabbixApiException(string message, int code, string data) : base(message)
    {
        ErrorCode = code;
        ErrorData = data;
    }
}