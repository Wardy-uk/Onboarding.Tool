using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.Dashboard.Settings;
using Onboarding.Tool.Server.Helpers.Files;

namespace Onboarding.Tool.Server.Helpers.Dashboard;

public class DashboardSettingsHelper
{
    private readonly List<BrandSettingConfig> _brandSettings;

    public DashboardSettingsHelper(IOptions<InstanceTemplateSettings> templateSettings)
    {
        _brandSettings = JsonConfigHelper.ReadInstanceConfig<List<BrandSettingConfig>>(templateSettings.Value.Directory, "Instance", "brandSettings.json");
    }

    public List<BrandSettingConfig> BrandSettings => _brandSettings;
}
