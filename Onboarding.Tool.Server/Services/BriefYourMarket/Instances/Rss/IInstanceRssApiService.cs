using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances.Rss;

public interface IInstanceRssApiService
{
    public Task<List<int>> AddRssFeedsToInstanceAsync(Instance instance);
}
