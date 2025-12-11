using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;
using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances.Rss;

public class InstanceRssApiService : IInstanceRssApiService
{
    private readonly IHttpHelper _httpHelper;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly InstanceSettings _instanceSettings;
    private readonly BriefYourMarketInstanceHelper _briefYourMarketInstanceHelper;

    public InstanceRssApiService(
        IHttpHelper httpHelper, 
        IDashboardProgressService dashboardProgressService,
        IOptions<InstanceSettings> instanceOptions, 
        BriefYourMarketInstanceHelper instanceHelper)
    {
        _httpHelper = httpHelper;
        _dashboardProgressService = dashboardProgressService;
        _instanceSettings = instanceOptions.Value;
        _briefYourMarketInstanceHelper = instanceHelper;
    }

    public async Task<List<int>> AddRssFeedsToInstanceAsync(Instance instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupRss", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return new List<int>();

        List<int> newsfeedIds = new();

        foreach (Newsfeed newsfeed in _briefYourMarketInstanceHelper.RssConfiguration.RssFeeds)
        {
            int? feedId = await AddRssFeedToInstanceAsync(instance.Subdomain, newsfeed);

            if (feedId == null)
                continue;

            newsfeedIds.Add(feedId.Value);
        }

        if (newsfeedIds.Count > 0)
            await _dashboardProgressService.SetSetupStatusAsync(instance, "setupRss", TemplateConfirmationState.Confirmed);

        return newsfeedIds;
    }

    private async Task<int?> AddRssFeedToInstanceAsync(string instance, Newsfeed newsfeed)
    {
        int? rssFeedId = null;

        try
        {
            rssFeedId = await _httpHelper.ExecuteRequestAsync<int>(HttpMethod.Post, "InstanceApi", _briefYourMarketInstanceHelper.GetFormattedInstanceString(instance), "api/newsfeeds", _instanceSettings.ApiKey, newsfeed);
        }
        catch (Exception) { }

        return rssFeedId;
    }
}
