namespace Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;

public class DeliveryAddress
{
    public required int Id { get; init; }

    public string? Name { get; init; }

    public string? Region { get; init; }

    public string? ContactTel { get; init; }

    public string? ContactEmail { get; init; }

    public string? Recipient { get; init; }

    public required bool IsDefault { get; init; }

    public string? Address { get; init; }
}
