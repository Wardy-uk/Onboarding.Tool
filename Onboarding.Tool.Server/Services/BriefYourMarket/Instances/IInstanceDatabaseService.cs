using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances;

public interface IInstanceDatabaseService
{
    public Task<bool> CreateDeliveryAddressesAsync(Instance instance, List<DeliveryAddress> existingAddresses);

    public Task AddDeliveryAddressesToUsersAsync(Instance instance);

    public Task AddUsersToGroupsAsync(Instance instance);

    public Task AddScheduledReportConfigurationsAsync(Instance instance);
}
