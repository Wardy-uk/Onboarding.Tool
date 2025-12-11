using Onboarding.Tool.Model.Dashboard.BuildConfigs;
using Onboarding.Tool.Model.Data;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Services.Dashboard.Build;

public interface IDashboardBuildService
{
    public Task<PortalAccount?> AddPortalAccountAsync(Instance instance, string name);

    public Task<bool> DeletePortalAccountAsync(Instance instance, int portalId);

    public Task<BranchBuildDistrict?> CreateBuildDistrictAsync(Instance instance, DistrictConfig districtConfig);

    public Task<BranchBuildDistrict?> UpdateBuildDistrictAsync(Instance instance, int districtId, DistrictConfig districtConfig);

    public Task<bool> DeleteBuildDistrictAsync(Instance instance, int districtId);

    public Task<List<PortalAccount>> ImportPortalAccountsAsync(Instance instance, List<BuildPortalAccountImport> portals);
}
