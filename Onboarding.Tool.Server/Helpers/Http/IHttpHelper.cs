namespace Onboarding.Tool.Server.Helpers.Http;

public interface IHttpHelper
{
    public Task<TResponse> ExecuteRequestAsync<TResponse>(HttpMethod method, string clientName, string baseAddress, string endpoint, string? basicAuth, object? payload = null);

    public Task<TResponse> ExecuteRequestBearerAsync<TResponse>(HttpMethod method, string clientName, string baseAddress, string endpoint, string? bearerToken, object? payload = null);

    public Task<HttpResponseMessage> UploadFileAsync(string clientName, string baseAddress, string endpoint, string fileName, byte[] content, string? basicAuth = null);

    public MultipartFormDataContent CreateFileUploadContent(string fileName, byte[] content);
}
