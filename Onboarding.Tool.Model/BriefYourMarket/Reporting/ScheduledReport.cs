namespace Onboarding.Tool.Model.BriefYourMarket.Reporting;

public class ScheduledReport
{
    public required int DefinitionId { get; init; }

    public required string Name { get; init; }

    public required ScheduledReportConfiguration Configuration { get; init; }
}
