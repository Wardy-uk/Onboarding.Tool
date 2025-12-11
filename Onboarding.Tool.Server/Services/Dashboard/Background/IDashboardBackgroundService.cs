using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Services.Dashboard.Background;

public interface IDashboardBackgroundService
{
    public Task QueueCardCreationAsync(Instance instance);
}
