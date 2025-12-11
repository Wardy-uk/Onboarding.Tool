using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.Robocop.EmailComponents;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Config.EmailComponents;

public class ConfigEmailComponentsService : IConfigEmailComponentsService
{
    private readonly IHttpHelper _httpHelper;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly ConfigSettings _configSettings;
    private readonly BriefYourMarketInstanceHelper _briefYourMarketInstanceHelper;


    public ConfigEmailComponentsService(
        IHttpHelper httpHelper, 
        IDashboardProgressService dashboardProgressService,
        IOptions<ConfigSettings> configSettings, 
        BriefYourMarketInstanceHelper briefYourMarketInstanceHelper)
    {
        _httpHelper = httpHelper;
        _dashboardProgressService = dashboardProgressService;
        _configSettings = configSettings.Value;
        _briefYourMarketInstanceHelper = briefYourMarketInstanceHelper;
    }

    public async Task<List<EmailComponentLibrary>> AddDefaultComponentLibrariesToInstanceAsync(Instance instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupComponents", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return new List<EmailComponentLibrary>();

        List<EmailComponentLibrary> emailComponentLibraries = new();

        foreach (int libraryId in _briefYourMarketInstanceHelper.InstanceConfig.EmailComponents)
        {
            EmailComponentLibrary? componentLibrary = await GetComponentLibraryAsync(libraryId);

            if (componentLibrary == null)
                continue;

            if (!componentLibrary.Instances.Select(i => i.Key).Contains(instance.Id))
            {
                componentLibrary.Instances.Add(instance.Id, instance.Subdomain);
                await UpdateComponentLibraryAsync(componentLibrary);
            }

            emailComponentLibraries.Add(componentLibrary);
        }

        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupComponents", TemplateConfirmationState.Confirmed);

        return emailComponentLibraries;
    }

    private async Task<EmailComponentLibrary?> GetComponentLibraryAsync(int libraryId)
    {
        EmailComponentLibrary? library = null;

        try
        {
            library = await _httpHelper.ExecuteRequestAsync<EmailComponentLibrary>(HttpMethod.Get, "ConfigApi", _configSettings.Domain, $"api/emailcomponentlibraries/{libraryId}", _configSettings.ApiKey);
        }
        catch (Exception) { }

        return library;
    }

    private async Task<EmailComponentLibrary> UpdateComponentLibraryAsync(EmailComponentLibrary emailComponentLibrary)
    {
        EmailComponentLibrary updatedLibrary = await _httpHelper.ExecuteRequestAsync<EmailComponentLibrary>(HttpMethod.Put, "ConfigApi", _configSettings.Domain, "api/emailcomponentlibraries", _configSettings.ApiKey, emailComponentLibrary);
        return updatedLibrary;
    }
}
