using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.Enums;
using Onboarding.Tool.Model.Robocop.Settings;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Config.Settings;

public class ConfigSettingsService : IConfigSettingsService
{
    private readonly IHttpHelper _httpHelper;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly ConfigSettings _configSettings;
    private readonly BriefYourMarketInstanceHelper _briefYourMarketInstanceHelper;
    
    public ConfigSettingsService(
        IHttpHelper httpHelper, 
        IDashboardProgressService dashboardProgressService,
        IOptions<ConfigSettings> configSettings, 
        BriefYourMarketInstanceHelper briefYourMarketInstanceHelper)
    {
        _httpHelper = httpHelper;
        _dashboardProgressService = dashboardProgressService;
        _briefYourMarketInstanceHelper = briefYourMarketInstanceHelper;
        _configSettings = configSettings.Value;
    }

    public async Task<List<InstanceSetting>> GetInstanceSettingsAsync(string instance)
    {
        List<InstanceSetting> settingsResponse = await _httpHelper.ExecuteRequestAsync<List<InstanceSetting>>(HttpMethod.Get, "ConfigApi", _configSettings.Domain, $"api/settings/{instance}", _configSettings.ApiKey);

        if (settingsResponse == null)
            return new List<InstanceSetting>();

        return settingsResponse;
    }

    public async Task<bool> AddDefaultSettingsToInstanceAsync(Instance instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupRobocop", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        List<InstanceSetting> settings = new();

        foreach (ExpectedSetting setting in _briefYourMarketInstanceHelper.InstanceConfig.ConfigSettings)
        {
            settings.Add(new InstanceSetting
            {
                Name = setting.Name,
                Value = setting.Value,
                Editable = false,
                Visible = false,
                LastModified = DateTime.Now
            });
        }

        await AddInstanceSettingsAsync(instance.Subdomain, settings);
        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupRobocop", TemplateConfirmationState.Confirmed);

        return true;
    }

    private async Task AddInstanceSettingsAsync(string instance, List<InstanceSetting> settings)
    {
        await _httpHelper.ExecuteRequestAsync<object>(HttpMethod.Post, "ConfigApi", _configSettings.Domain, $"api/settings/{instance}", _configSettings.ApiKey, settings);
    }
}
