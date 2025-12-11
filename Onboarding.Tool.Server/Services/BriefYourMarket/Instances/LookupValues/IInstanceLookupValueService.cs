using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances.LookupValues;

public interface IInstanceLookupValueService
{
    public Task<bool> AddBrandsToInstanceAsync(Instance instance);

    public Task<bool> AddBranchesToInstanceAsync(Instance instance);

    public Task<List<LookupValue>> GetInstanceLookupValuesFromApiAsync(string instance, string endpoint);

    public Task<List<DeliveryAddress>> GetDeliveryAddressesFromInstanceAsync(Instance instance);
}
