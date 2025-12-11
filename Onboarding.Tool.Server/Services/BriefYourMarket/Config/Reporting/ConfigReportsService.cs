using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Config.Reporting;

public class ConfigReportsService : IConfigReportsService
{
    private readonly IHttpHelper _httpHelper;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly ConfigSettings _configSettings;
    private readonly BriefYourMarketInstanceHelper _briefYourMarketInstanceHelper;

    public ConfigReportsService(
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

    public async Task<bool> AddScheduledReportsToInstanceAsync(Instance instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupScheduledReports", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        List<int> reportDefinitionIds = _briefYourMarketInstanceHelper.ScheduledReportConfiguration.ScheduledReports.Select(sr => sr.DefinitionId).ToList();

        foreach (int reportDefinitionId in reportDefinitionIds)
        {
            await AddScheduledReportToInstanceAsync(instance.Id, reportDefinitionId);
        }

        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupScheduledReports", TemplateConfirmationState.Confirmed);
        return true;
    }

    private async Task AddScheduledReportToInstanceAsync(int instanceId, int definitionId)
    {
        object payload = new
        {
            InstanceId = instanceId,
            ReportDefinitionId = definitionId
        };

        await _httpHelper.ExecuteRequestAsync<object>(HttpMethod.Post, "ConfigApi", _configSettings.Domain, "api/scheduledreports/definitions", _configSettings.ApiKey, payload);
    }
}
