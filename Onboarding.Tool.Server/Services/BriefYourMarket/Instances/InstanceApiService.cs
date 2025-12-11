using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using TemplateModel = Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels.Template;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances;

public class InstanceApiService : IInstanceApiService
{
    private readonly IHttpHelper _httpHelper;
    private readonly InstanceSettings _instanceSettings;
    private readonly BriefYourMarketInstanceHelper _briefYourMarketInstanceHelper;

    public InstanceApiService(IHttpHelper httpHelper, IOptions<InstanceSettings> instanceOptions, BriefYourMarketInstanceHelper instanceHelper)
    {
        _httpHelper = httpHelper;
        _instanceSettings = instanceOptions.Value;
        _briefYourMarketInstanceHelper = instanceHelper;
    }

    public async Task<string> GetBearerTokenAsync(Instance instance)
    {
        Authorize? authorization = await _httpHelper.ExecuteRequestAsync<Authorize?>(HttpMethod.Get, "InstanceApi", _briefYourMarketInstanceHelper.GetFormattedInstanceString(instance.Subdomain), "api/authorize", _instanceSettings.ApiKey);

        return authorization == null
            ? throw new InvalidOperationException("Failed to obtain a bearer token from instance.")
            : authorization.BearerToken;
    }

    public async Task<List<TemplateModel>> GetInstanceTemplatesAsync(string instance, string endpoint)
    {
        List<TemplateModel> templates = await _httpHelper.ExecuteRequestAsync<List<TemplateModel>>(HttpMethod.Get, "InstanceApi", _briefYourMarketInstanceHelper.GetFormattedInstanceString(instance), endpoint, _instanceSettings.ApiKey);
        return templates;
    }
}
