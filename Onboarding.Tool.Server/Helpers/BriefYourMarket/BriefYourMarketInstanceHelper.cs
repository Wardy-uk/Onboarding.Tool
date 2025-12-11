using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BriefYourMarket.Automation;
using Onboarding.Tool.Model.BriefYourMarket.Instances.Configurations;
using Onboarding.Tool.Model.BriefYourMarket.Integrations;
using Onboarding.Tool.Server.Helpers.Files;

namespace Onboarding.Tool.Server.Helpers.BriefYourMarket;

public class BriefYourMarketInstanceHelper
{
    private readonly string _instanceFormat;
    private readonly InstanceConfig _instanceConfig;
    private readonly RssConfiguration _rssConfiguration;
    private readonly ReportingConfiguration _scheduledReportConfiguration;
    private readonly IntegrationConfiguration _integrationConfiguration;
    private readonly AutomationConfiguration _automationConfiguration;

    public BriefYourMarketInstanceHelper(IOptions<InstanceSettings> instanceOptions, IOptions<InstanceTemplateSettings> templateSettings)
    {
        _instanceFormat = instanceOptions.Value.ServiceUrlTemplate;

        _instanceConfig = JsonConfigHelper.ReadInstanceConfig<InstanceConfig>(templateSettings.Value.Directory, "Instance", "instance.json");
        _rssConfiguration = JsonConfigHelper.ReadInstanceConfig<RssConfiguration>(templateSettings.Value.Directory, "Instance", "rss.json");
        _scheduledReportConfiguration = JsonConfigHelper.ReadInstanceConfig<ReportingConfiguration>(templateSettings.Value.Directory, "Instance", "scheduledreport.json");
        _integrationConfiguration = JsonConfigHelper.ReadInstanceConfig<IntegrationConfiguration>(templateSettings.Value.Directory, "Instance", "integrations.json");
        _automationConfiguration = JsonConfigHelper.ReadInstanceConfig<AutomationConfiguration>(templateSettings.Value.Directory, "Instance", "automation.json");
    }


    public string GetFormattedInstanceString(string instance)
    {
        return string.Format(_instanceFormat, instance);
    }

    public InstanceConfig InstanceConfig => _instanceConfig;

    public RssConfiguration RssConfiguration => _rssConfiguration;

    public ReportingConfiguration ScheduledReportConfiguration => _scheduledReportConfiguration;

    public IntegrationConfiguration IntegrationConfiguration => _integrationConfiguration;

    public AutomationConfiguration AutomationConfiguration => _automationConfiguration;
}
