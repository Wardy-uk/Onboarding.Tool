namespace Onboarding.Tool.Model.Settings;

public class InstanceSettings
{
    public required string ServiceUrlTemplate { get; init; }

    public required string ApiKey { get; init; }

    public required List<string> UserPermissions { get; init; }
}
