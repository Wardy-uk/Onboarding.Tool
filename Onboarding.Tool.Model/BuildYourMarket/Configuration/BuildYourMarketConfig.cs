namespace Onboarding.Tool.Model.BuildYourMarket.Configuration;

public class BuildYourMarketConfig
{
    public required Milestones Milestones { get; init; }

    public required List<StandardContent> StandardContent { get; init; }

    public required List<Campaign> Campaigns { get; init; }
}
