using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;
using Onboarding.Tool.Model.Enums;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Serilog;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances.Users;

public class InstanceUsersApiService : IInstanceUsersApiService
{
    private readonly IHttpHelper _httpHelper;
    private readonly IOnboardingDataService _onboardingDataService;
    private readonly IInstanceLookupValueService _instanceLookupValueService;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly IInstanceDatabaseService _instanceDatabaseService;
    private readonly InstanceSettings _instanceSettings;
    private readonly BriefYourMarketInstanceHelper _bymInstanceHelper;

    public InstanceUsersApiService(
        IHttpHelper httpHelper, 
        IOnboardingDataService onboardingDataService,
        IInstanceLookupValueService instanceLookupValueService, 
        IDashboardProgressService dashboardProgressService,
        IInstanceDatabaseService instanceDatabaseService,
        IOptions<InstanceSettings> instanceOptions, 
        BriefYourMarketInstanceHelper bymInstanceHelper)
    {
        _httpHelper = httpHelper;
        _onboardingDataService = onboardingDataService;
        _instanceLookupValueService = instanceLookupValueService;
        _dashboardProgressService = dashboardProgressService;
        _instanceDatabaseService = instanceDatabaseService;
        _instanceSettings = instanceOptions.Value;
        _bymInstanceHelper = bymInstanceHelper;
    }

    public async Task<List<int>> AddUsersToInstanceAsync(Instance instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupUsers", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return new List<int>();

        IEnumerable<string> emails = await _onboardingDataService.GetSetupUserEmailsAsync(instance);

        List<int> userIds = new();
        List<LookupValue> brands = await _instanceLookupValueService.GetInstanceLookupValuesFromApiAsync(instance.Subdomain, "api/brands");
        List<LookupValue> branches = await _instanceLookupValueService.GetInstanceLookupValuesFromApiAsync(instance.Subdomain, "api/branches");
        List<DeliveryAddress> deliveryAddresses = await _instanceLookupValueService.GetDeliveryAddressesFromInstanceAsync(instance);

        foreach (string email in emails)
        {
            int? contactId = await AddUserAsContactToInstanceAsync(instance.Subdomain, email);
            int? userId = await AddUserToInstanceAsync(instance.Subdomain, email, brands, branches, deliveryAddresses, contactId);

            if (userId == null)
                continue;

            userIds.Add(userId.Value);
        }

        await _instanceDatabaseService.AddDeliveryAddressesToUsersAsync(instance);
        await _instanceDatabaseService.AddUsersToGroupsAsync(instance);

        if (userIds.Count > 0)
            await _dashboardProgressService.SetSetupStatusAsync(instance, "setupUsers", TemplateConfirmationState.Confirmed);
    
        return userIds;
    }

    private async Task<int?> AddUserAsContactToInstanceAsync(string instance, string email)
    {
        List<Contact> contacts = new()
        {
            new()
            {
                AlternateId = "Onboarding-" + email,
                EMail = email
            }
        };

        int? contactId = null;

        try
        {
            ContactResponse result = await _httpHelper.ExecuteRequestAsync<ContactResponse>(HttpMethod.Post, "InstanceApi", _bymInstanceHelper.GetFormattedInstanceString(instance), "api/contacts", _instanceSettings.ApiKey, contacts);
        
            if (result.Results[0] != null)
            {
                if (result.Results[0].ContactIds.Count == 0)
                    return null;

                contactId = result.Results[0].ContactIds[0];
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add user");
        }

        return contactId;
    }

    private async Task<int?> AddUserToInstanceAsync(string instance, string email, List<LookupValue> brands, List<LookupValue> branches, List<DeliveryAddress> deliveryAddresses, int? contactId)
    {
        User user = new()
        {
            Username = email,
            Email = email,
            Roles = _instanceSettings.UserPermissions,
            AlertsEnabled = true,
            NoBrand = false,
            Brands = brands,
            Branches = branches,
            DeliveryAddresses = deliveryAddresses,
            Enabled = true,
            ContactId = contactId
        };

        int? userId = null;
        try
        {
            userId = await _httpHelper.ExecuteRequestAsync<int>(HttpMethod.Post, "InstanceApi", _bymInstanceHelper.GetFormattedInstanceString(instance), "api/users", _instanceSettings.ApiKey, user);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add user");
        }

        return userId;
    }
}
