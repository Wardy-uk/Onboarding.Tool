namespace Onboarding.Tool.Model.Robocop.Settings;

public class InstanceSetting
{
    public required string Name { get; init; }

    public required string Value { get; init; }

    public required bool Visible { get; init; }

    public required bool Editable { get; init; }

    public required DateTime LastModified { get; init; }

    public int? InstanceId { get; init; }
}
