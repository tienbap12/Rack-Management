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
    private static readonly JsonSerializerOptions _serializeOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions _deserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private record ZabbixRequest(
       [property: JsonPropertyName("method")] string Method,
       [property: JsonPropertyName("params")] object Params,
       [property: JsonPropertyName("auth")] string Auth,
       [property: JsonPropertyName("id")] long Id,
       [property: JsonPropertyName("jsonrpc")] string JsonRpc = "2.0"
    );

    private record ZabbixResponse<TResult>(
        [property: JsonPropertyName("jsonrpc")] string JsonRpc,
        [property: JsonPropertyName("result")] TResult? Result,
        [property: JsonPropertyName("error")] ZabbixError? Error,
        [property: JsonPropertyName("id")] long Id
    );

    private record ZabbixError(
        [property: JsonPropertyName("code")] int Code,
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("data")] string Data
    );

    public static async Task<TResult?> SendRequestAsync<TResult>(
        HttpClient httpClient,
        string apiUrl,
        string apiToken,
        string method,
        object parameters,
        CancellationToken cancellationToken = default)
    {
        var requestId = DateTime.UtcNow.Ticks;
        var requestPayload = new ZabbixRequest(
            Method: method,
            Params: parameters,
            Auth: apiToken,
            Id: requestId
        );

        HttpResponseMessage response = null!;
        try
        {
            response = await httpClient.PostAsJsonAsync(apiUrl, requestPayload, _serializeOptions, cancellationToken);

            response.EnsureSuccessStatusCode();

            var zabbixResponse = await response.Content.ReadFromJsonAsync<ZabbixResponse<TResult>>(_deserializeOptions, cancellationToken);

            if (zabbixResponse == null)
            {
                throw new ZabbixApiException($"Failed to deserialize Zabbix response for method '{method}'.");
            }

            if (zabbixResponse.Error != null)
            {
                var error = zabbixResponse.Error;
                Console.WriteLine($"ERROR: Zabbix API Error ({method}) - Code: {error.Code}, Message: {error.Message}, Data: {error.Data}");
                throw new ZabbixApiException(
                    $"Zabbix API error (Code: {error.Code}): {error.Message} - {error.Data}",
                    error.Code,
                    error.Data);
            }

            return zabbixResponse.Result;
        }
        catch (HttpRequestException ex) when (response != null)
        {
            Console.WriteLine($"ERROR: HTTP Request Failed ({method}) - Status: {response.StatusCode}, Message: {ex.Message}");
            throw new ZabbixApiException($"HTTP request to Zabbix API failed for method '{method}' with status code {response.StatusCode}.", ex);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"ERROR: HTTP Request Failed ({method}) - No response. Message: {ex.Message}");
            throw new ZabbixApiException($"HTTP request to Zabbix API failed for method '{method}'.", ex);
        }
        catch (NotSupportedException ex)
        {
            Console.WriteLine($"ERROR: JSON Deserialization Not Supported ({method}) - Message: {ex.Message}");
            throw new ZabbixApiException($"Failed to parse Zabbix API response due to unsupported content type for method '{method}'.", ex);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"ERROR: JSON Parsing Failed ({method}) - Message: {ex.Message}");
            throw new ZabbixApiException($"Failed to parse Zabbix API response for method '{method}'. Check response format.", ex);
        }
        catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine($"WARN: Request Cancelled ({method})");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Unexpected Error ({method}) - Message: {ex.Message}");
            throw new ZabbixApiException($"An unexpected error occurred while calling Zabbix API method '{method}'. See inner exception.", ex);
        }
    }
}

public class ZabbixApiException : Exception
{
    public int? ErrorCode { get; }
    public string? ErrorData { get; }

    public ZabbixApiException(string message) : base(message) { }
    public ZabbixApiException(string message, Exception innerException) : base(message, innerException) { }
    public ZabbixApiException(string message, int code, string data) : base(message)
    {
        ErrorCode = code;
        ErrorData = data;
    }
}