using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances.Users;

public interface IInstanceUsersApiService
{
    public Task<List<int>> AddUsersToInstanceAsync(Instance instance);
}
