using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Services.Images;

public class ImageService : IImageService
{
    private static string _imageServiceUrl = "https://bymmedia-dev.azurewebsites.net/api/v1/media/onboarding";
    private readonly IHttpClientFactory _httpClientFactory;

    public ImageService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> UploadAsync(Instance instance, string fileName, byte[] bytes)
    {
        using HttpClient client = _httpClientFactory.CreateClient();

        using HttpRequestMessage request = new(HttpMethod.Post, $"{_imageServiceUrl}/${instance.Subdomain}");
        using MultipartFormDataContent requestContent = new();

        using var ms = new MemoryStream(bytes);
        using var fileContent = new StreamContent(ms);

        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        requestContent.Add(fileContent, "file", Path.GetFileName(fileName));
        request.Content = requestContent;

        HttpResponseMessage response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

}
