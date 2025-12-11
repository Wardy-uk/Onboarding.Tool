namespace Onboarding.Tool.Model.BriefYourMarket.Automation;

public class AutomationConfiguration
{
    public required List<EmailAutomation> Email { get; init; }

    public required List<CardAutomation> Auto2020 { get; init; }
}
