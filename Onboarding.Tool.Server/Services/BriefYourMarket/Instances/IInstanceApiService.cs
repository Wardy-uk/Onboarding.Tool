using Onboarding.Tool.Model.BriefYourMarket.Instances;
using TemplateModel = Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels.Template;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances;

public interface IInstanceApiService
{
    public Task<string> GetBearerTokenAsync(Instance instance);

    public Task<List<TemplateModel>> GetInstanceTemplatesAsync(string instance, string endpoint);
}
