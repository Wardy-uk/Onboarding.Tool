namespace Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;

public class Newsfeed
{
    public required bool IsBymFeed { get; init; }

    public required string Description { get; init; }

    public required string ImageTitle { get; init; }

    public required string ImageDescription { get; init; }

    public required string ImageUrl { get; init; }

    public string? Instance { get; init; }

    public string? ContentChannel { get; init; }

    public required bool AutomaticallyCreateArticles { get; init; }

    public required string FeedUrl { get; init; }

    public required string CategoryAssignType { get; init; }

    public required bool IsBookMarked { get; init; }

    public required int Type { get; init; }

    public required int TimeToLive { get; init; }

    public required bool Active { get; init; }
}
