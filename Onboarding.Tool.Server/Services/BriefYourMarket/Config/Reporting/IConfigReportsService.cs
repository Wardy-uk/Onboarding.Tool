using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Config.Reporting;

public interface IConfigReportsService
{
    public Task<bool> AddScheduledReportsToInstanceAsync(Instance instance);
}
