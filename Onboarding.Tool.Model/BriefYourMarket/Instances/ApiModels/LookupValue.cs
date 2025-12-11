namespace Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;

public class LookupValue
{
    public int? Id { get; init; }

    public required string Value { get; init; }

    public required string Classification { get; init; }

    public required bool IsSecured { get; init; }

    public required bool IsDefault { get; init; }
}
