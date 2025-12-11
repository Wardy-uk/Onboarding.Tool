using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BuildYourMarket.Configuration;
using Onboarding.Tool.Server.Helpers.Files;

namespace Onboarding.Tool.Server.Helpers.BuildYourMarket;

public class BuildYourMarketHelper
{
    private readonly BuildYourMarketConfig _buildYourMarketConfig;

    public BuildYourMarketHelper(IOptions<InstanceTemplateSettings> templateSettings)
    {
        _buildYourMarketConfig = JsonConfigHelper.ReadInstanceConfig<BuildYourMarketConfig>(templateSettings.Value.Directory, "Instance", "build.json");
    }

    public BuildYourMarketConfig Config => _buildYourMarketConfig;
}
