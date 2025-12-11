using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Services.BuildYourMarket;

public interface IBuildYourMarketService
{
    public Task<bool> AddMilestonesAsync(Instance instance);

    public Task<bool> AddStandardContentAsync(Instance instance);

    public Task<bool> SetupBranchesAsync(Instance instance);

    public Task<bool> AddPortalAccountsAsync(Instance instance);
}
