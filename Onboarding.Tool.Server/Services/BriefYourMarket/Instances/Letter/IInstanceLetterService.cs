using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances.Letter;

public interface IInstanceLetterService
{
    public Task<LetterHeadResult> CreateDefaultLetterHeadAsync(Instance instance);
}
