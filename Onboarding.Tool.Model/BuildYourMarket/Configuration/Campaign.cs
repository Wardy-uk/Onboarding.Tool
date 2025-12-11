namespace Onboarding.Tool.Model.BuildYourMarket.Configuration;

public class Campaign
{
    public required string Name { get; init; }

    public required string Description { get; init; }

    public required CampaignContent Content { get; init; }
}
