using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances.Automation;

public interface IInstanceAutomationService
{
    public Task<List<int>> AddDefaultEmailTriggersToInstanceAsync(Instance instance);

    public Task<List<int>> AddDefault2020TriggersToInstanceAsync(Instance instance);
}
