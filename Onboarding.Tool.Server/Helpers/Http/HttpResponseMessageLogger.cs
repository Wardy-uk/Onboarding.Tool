using System.Net;
using System.Text;

namespace Onboarding.Tool.Server.Helpers.Http;

public static class HttpResponseMessageLogger
{
    public static async Task<string> ToLogStringAsync(this HttpResponseMessage response)
    {
        if (response == null) return "HttpResponseMessage is null";

        var sb = new StringBuilder();
        
        sb.AppendLine($"StatusCode: {(int)response.StatusCode} ({response.StatusCode})");
        sb.AppendLine($"ReasonPhrase: {response.ReasonPhrase}");
        sb.AppendLine($"Version: {response.Version}");
        sb.AppendLine($"Host: {response.RequestMessage?.RequestUri?.Host}");

        sb.AppendLine("Headers:");
        foreach (var header in response.Headers)
        {
            sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }

        if (response.Content != null)
        {
            sb.AppendLine("Content Headers:");
            foreach (var header in response.Content.Headers)
            {
                sb.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }

            string content = await response.Content.ReadAsStringAsync();
            sb.AppendLine("Content:");
            sb.AppendLine(content);
        }
        else
        {
            sb.AppendLine("Content: <null>");
        }

        return sb.ToString();
    }
}
