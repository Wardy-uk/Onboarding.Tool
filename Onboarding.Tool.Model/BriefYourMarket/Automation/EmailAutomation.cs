namespace Onboarding.Tool.Model.BriefYourMarket.Automation;

public class EmailAutomation
{
    public required string Name { get; init; }

    public required string Subject { get; init; }

    public required List<EventNameAutomation> EventNames { get; init; }

    public required int EventOccurence { get; init; }

    public required int EventOccurenceType { get; init; }

    public required string Template { get; init; }

    public required string Text { get; init; }

    public required string Html { get; init; }
}
