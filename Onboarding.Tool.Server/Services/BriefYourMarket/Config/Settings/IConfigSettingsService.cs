using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.Robocop.Settings;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Config.Settings;

public interface IConfigSettingsService
{
    public Task<List<InstanceSetting>> GetInstanceSettingsAsync(string instance);

    public Task<bool> AddDefaultSettingsToInstanceAsync(Instance instance);
}
