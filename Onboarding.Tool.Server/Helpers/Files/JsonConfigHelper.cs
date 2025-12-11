using Serilog;
using System.Text.Json;

namespace Onboarding.Tool.Server.Helpers.Files;

public static class JsonConfigHelper
{
    public static T ReadInstanceConfig<T>(string basePath, string directory, string file)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        string configContents = GetConfigContents(basePath, directory, file);
        T config = JsonSerializer.Deserialize<T>(configContents, options) ?? throw new InvalidOperationException($"Could not read instance settings config file.");

        return config;
    }

    public static string GetConfigContents(string basePath, string directory, string file)
    {
        try
        {
            string instanceConfigPath = Path.Combine(basePath, directory, file);
            string configContents = File.ReadAllText(instanceConfigPath);

            return configContents;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Problem reading config.");
            throw;
        }
    }
}
