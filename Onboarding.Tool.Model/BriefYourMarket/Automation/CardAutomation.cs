namespace Onboarding.Tool.Model.BriefYourMarket.Automation;

public class CardAutomation
{
    public required string Name { get; init; }

    public required List<EventNameAutomation> EventNames { get; init; }

    public required List<TriggerFilter> TriggerFilter { get; init; }

    public required string TemplateName { get; init; }
}
