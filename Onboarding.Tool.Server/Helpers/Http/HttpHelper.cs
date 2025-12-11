using Serilog;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Onboarding.Tool.Server.Helpers.Http;

public class HttpHelper : IHttpHelper
{
    private readonly IHttpClientFactory _httpClientFactory;

    public HttpHelper(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<TResponse> ExecuteRequestAsync<TResponse>(
        HttpMethod method,
        string clientName,
        string baseAddress,
        string endpoint,
        string? basicAuth = null,
        object? payload = null)
    {
        using HttpClient client = CreateHttpClient(clientName, baseAddress);
        client.Timeout = TimeSpan.FromMinutes(5);

        using HttpRequestMessage request = new(method, endpoint);

        if (!string.IsNullOrEmpty(basicAuth))
            SetBasicAuthorizationHeader(request, basicAuth);

        if (payload != null)
        {
            string json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        Log.Information("Sending HTTP Request to {Url} with {Content}", new { Url = request.RequestUri, Content = request.Content?.ToString() });

        HttpResponseMessage response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            Log.Information("HTTP Response: \n{Response}", await response.ToLogStringAsync());

            throw new HttpRequestException($"Request to {endpoint} failed with status {response.StatusCode}: {response.ReasonPhrase}");
        }

        string responseContent = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Empty response received from {endpoint}");

            return default!;
        }

        return JsonSerializer.Deserialize<TResponse>(responseContent) ?? throw new InvalidOperationException($"Failed to deserialize response from {endpoint}");
    }

    public async Task<TResponse> ExecuteRequestBearerAsync<TResponse>(
        HttpMethod method,
        string clientName,
        string baseAddress,
        string endpoint,
        string? bearerToken = null,
        object? payload = null)
    {
        using HttpClient client = CreateHttpClient(clientName, baseAddress);
        using HttpRequestMessage request = new(method, endpoint);

        if (!string.IsNullOrEmpty(bearerToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

        if (payload != null)
        {
            string json = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        Log.Information("Sending HTTP Request to {Url} with {Content}", new { Url = request.RequestUri, Content = request.Content?.ToString() });

        HttpResponseMessage response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            Log.Information("HTTP Response: \n{Response}", await response.ToLogStringAsync());

            throw new HttpRequestException($"Request to {endpoint} failed with status {response.StatusCode}: {response.ReasonPhrase}");
        }

        string responseContent = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"Empty response received from {endpoint}");

            return default!;
        }

        if (typeof(TResponse) == typeof(string))
        {
            object result = responseContent.StartsWith("\"") && responseContent.EndsWith("\"")
                ? JsonSerializer.Deserialize<string>(responseContent)!
                : responseContent;

            return (TResponse)result;
        }

        return JsonSerializer.Deserialize<TResponse>(responseContent) ?? throw new InvalidOperationException($"Failed to deserialize response from {endpoint}");
    }


    public async Task<HttpResponseMessage> UploadFileAsync(string clientName, string baseAddress, string endpoint, string fileName, byte[] content, string? basicAuth = null)
    {
        using HttpClient client = CreateHttpClient(clientName, baseAddress);
        using HttpRequestMessage request = new(HttpMethod.Post, endpoint);

        if (!string.IsNullOrEmpty(basicAuth))
            SetBasicAuthorizationHeader(request, basicAuth);

        Log.Information("Attempting to upload {fileName} with content {fileContent} to {endpoint}", fileName, Convert.ToBase64String(content), request.RequestUri);
        request.Content = CreateFileUploadContent(fileName, content);

        HttpResponseMessage httpResponse = await client.SendAsync(request);

        return httpResponse;
    }

    public MultipartFormDataContent CreateFileUploadContent(string fileName, byte[] content)
    {
        ByteArrayContent fileContent = new(content)
        {
            Headers =
            {
                ContentType = new MediaTypeHeaderValue("application/octet-stream"),
                ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "\"file\"",
                    FileName = $"\"{fileName}\""
                }
            }
        };

        return new MultipartFormDataContent { { fileContent, "file", fileName } };
    }

    private HttpClient CreateHttpClient(string clientName, string baseAddress)
    {
        HttpClient client = _httpClientFactory.CreateClient(clientName);
        client.BaseAddress = new Uri(baseAddress);
        return client;
    }

    private static void SetBasicAuthorizationHeader(HttpRequestMessage request, string basicAuth)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
    }
}
