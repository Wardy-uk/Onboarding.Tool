using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.MatchToCrm;

public interface IMatchToCrmService
{
    public Task<bool> AddMatchToCrmToInstanceAsync(Instance instance);
}
