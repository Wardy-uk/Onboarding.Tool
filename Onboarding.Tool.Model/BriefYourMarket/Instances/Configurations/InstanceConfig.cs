using Onboarding.Tool.Model.Robocop.Settings;

namespace Onboarding.Tool.Model.BriefYourMarket.Instances.Configurations;

public class InstanceConfig
{
    public required List<int> EmailComponents { get; init; }

    public required List<ExpectedSetting> ConfigSettings { get; init; }

    public List<int> DefaultPrintLibraries { get; set; } = new List<int>();

    public int? QualityAssuranceInstance { get; init; }
}
