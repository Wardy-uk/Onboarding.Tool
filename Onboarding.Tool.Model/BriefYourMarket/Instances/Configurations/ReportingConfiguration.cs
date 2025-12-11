using Onboarding.Tool.Model.BriefYourMarket.Reporting;

namespace Onboarding.Tool.Model.BriefYourMarket.Instances.Configurations;

public class ReportingConfiguration
{
    public required List<ScheduledReport> ScheduledReports { get; init; }
}
