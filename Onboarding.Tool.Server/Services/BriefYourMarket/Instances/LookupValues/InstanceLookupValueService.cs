using Microsoft.Extensions.Options;
using Onboarding.Tool.Data;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;
using Onboarding.Tool.Model.Enums;
using InstanceModel = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using Branch = Onboarding.Tool.Model.BriefYourMarket.Branches.Branch;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances.LookupValues;

public class InstanceLookupValueService : IInstanceLookupValueService
{
    private readonly IHttpHelper _httpHelper;
    private readonly IOnboardingDataService _onboardingDataService;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly InstanceSettings _instanceSettings;
    private readonly BriefYourMarketInstanceHelper _instanceHelper;
    private readonly AppDbContext _context;

    public InstanceLookupValueService(
        IHttpHelper httpHelper, 
        IOptions<InstanceSettings> instanceSettings,
        IOnboardingDataService onboardingDataService,
        IDashboardProgressService dashboardProgressService,
        BriefYourMarketInstanceHelper briefYourMarketInstanceHelper,
        AppDbContext context)
    {
        _httpHelper = httpHelper;
        _instanceSettings = instanceSettings.Value;
        _onboardingDataService = onboardingDataService;
        _dashboardProgressService = dashboardProgressService;
        _instanceHelper = briefYourMarketInstanceHelper;
        _context = context;
    }

    public async Task<bool> AddBrandsToInstanceAsync(InstanceModel instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupBrands", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        List<LookupValue> existingBrands = await GetInstanceLookupValuesFromApiAsync(instance.Subdomain, "api/brands"); // temporary whilst PUT api is dysfunctional
        List<LookupValue> brands = await GetInstanceBrandsAsync(instance);
        brands = FilterDuplicateLookupValues(brands, existingBrands);

        await _httpHelper.ExecuteRequestAsync<object>(HttpMethod.Post, "InstanceApi", _instanceHelper.GetFormattedInstanceString(instance.Subdomain), "api/brands", _instanceSettings.ApiKey, brands);
        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupBrands", TemplateConfirmationState.Confirmed);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddBranchesToInstanceAsync(InstanceModel instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupBranches", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        List<LookupValue> existingBranches = await GetInstanceLookupValuesFromApiAsync(instance.Subdomain, "api/branches"); // temporary whilst PUT api is dysfunctional
        List<LookupValue> branches = await GetInstanceBrandsAsync(instance, "Branches");
        branches = FilterDuplicateLookupValues(branches, existingBranches);

        await _httpHelper.ExecuteRequestAsync<object>(HttpMethod.Post, "InstanceApi", _instanceHelper.GetFormattedInstanceString(instance.Subdomain), "api/branches", _instanceSettings.ApiKey, branches);
        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupBranches", TemplateConfirmationState.Confirmed);

        return true;
    }

    public async Task<List<LookupValue>> GetInstanceLookupValuesFromApiAsync(string instance, string endpoint)
    {
        List<LookupValue> brands = await _httpHelper.ExecuteRequestAsync<List<LookupValue>>(HttpMethod.Get, "InstanceApi", _instanceHelper.GetFormattedInstanceString(instance), endpoint, _instanceSettings.ApiKey);
        return brands;
    }

    public async Task<List<DeliveryAddress>> GetDeliveryAddressesFromInstanceAsync(InstanceModel instance)
    {
        List<DeliveryAddress> deliveryAddresses = await _httpHelper.ExecuteRequestAsync<List<DeliveryAddress>>(HttpMethod.Get, "InstanceApi", _instanceHelper.GetFormattedInstanceString(instance.Subdomain), "api/deliveryaddress", _instanceSettings.ApiKey);
        return deliveryAddresses;
    }

    private async Task<List<LookupValue>> GetInstanceBrandsAsync(InstanceModel instance, string classification = "")
    {
        IEnumerable<Branch> brandDefinitions = await _onboardingDataService.GetSetupBranchesAsync(instance);

        List<LookupValue> brands = brandDefinitions.Select(b => new LookupValue
        {
            Value = b.Name,
            Classification = classification,
            IsSecured = true,
            IsDefault = b.Default
        }).ToList();

        return brands;
    }

    private static List<LookupValue> FilterDuplicateLookupValues(List<LookupValue> target, List<LookupValue> source)
    {
        HashSet<string> existingBrandValues = new(
            source.Select((LookupValue b) => b.Value),
            StringComparer.OrdinalIgnoreCase
        );

        List<LookupValue> filtered = target.Where((LookupValue b) => !existingBrandValues.Contains(b.Value)).ToList();
        return filtered;
    }
}
