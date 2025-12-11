namespace Onboarding.Tool.Model.Settings;

public class ConfigSettings
{
    public required string ConnectionString { get; init; }

    public required string InstanceConnectionStringTemplate { get; init; }

    public required string DnsLevelAppend { get; init; }

    public required string Domain { get; init; }

    public required string ApiKey { get; init; }
}
