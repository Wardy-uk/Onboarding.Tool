using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;

public class User
{
    [JsonPropertyName("UserName")]
    public required string Username { get; init; }

    public required string Email { get; init; }

    public required bool AlertsEnabled { get; init; }

    public required List<string> Roles { get; init; }

    public required List<LookupValue> Brands { get; init; }

    public required List<LookupValue> Branches { get; init; }

    public required List<DeliveryAddress> DeliveryAddresses { get; init; }

    public required bool NoBrand { get; init; }

    public required bool Enabled { get; init; }

    public int? ContactId { get; init; }
}
