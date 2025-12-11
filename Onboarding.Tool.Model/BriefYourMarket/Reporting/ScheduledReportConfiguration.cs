using Onboarding.Tool.Model.Enums.BriefYourMarket.Reporting;

namespace Onboarding.Tool.Model.BriefYourMarket.Reporting;

public class ScheduledReportConfiguration
{
    public required ScheduledReportEvaluationFrequencies Frequency { get; init; }

    public required string Name { get; init; }
}
