namespace Onboarding.Tool.Model.Settings;

public class OpenIdSettings
{
    public required string AuthorizationUrl { get; set; }

    public required string TokenUrl { get; set; } = "https://identity-dev.nurtur.tech/connect/token";

    public required string Authority { get; set; } = "https://identity-dev.nurtur.tech";

    public required string ClientId { get; set; }

    public required string ClientSecret { get; set; }

    public string[] ApiScope { get; set; } = Array.Empty<string>();
}
