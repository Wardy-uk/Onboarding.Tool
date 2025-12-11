using Onboarding.Tool.Model.Dashboard.Settings;
using Onboarding.Tool.Model.Data;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Services.Dashboard.Settings;

public interface IDashboardSettingsService
{
    public Task<List<InstanceSetting>?> ApplyInstanceSettingsAsync(Instance instance, Dictionary<string, string> instanceSettings);

    public Task<List<BranchSetting>?> ApplyBranchSettingsAsync(Instance instance, int branchId, Dictionary<string, string> branchSettings);

    public Task<bool> ImportSettingsAsync(Instance instance, List<Dictionary<string, object>> importSettings);

    public Task<List<InstanceSetting>> GetSettingsAsync(Instance instance);

    public Task<List<BranchSetting>> GetBranchSettingsAsync(Instance instance, int branchId);

    public Task<List<SettingBranchDto>> GetBranchesAsync(Instance instance);
}
