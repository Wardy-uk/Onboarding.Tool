using Onboarding.Tool.Model.BriefYourMarket.Brands;
using Onboarding.Tool.Model.Dashboard.Branches;
using Onboarding.Tool.Model.Dashboard.Users;
using PortalAccount = Onboarding.Tool.Model.BuildYourMarket.PortalAccount;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using Branch = Onboarding.Tool.Model.BriefYourMarket.Branches.Branch;
using Image = Onboarding.Tool.Model.BriefYourMarket.Images.Image;
using EfInstance = Onboarding.Tool.Model.Data.Instance;
using Onboarding.Tool.Model.BuildYourMarket;
using Onboarding.Tool.Model.Dashboard;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Model.Dashboard.Settings;
using Onboarding.Tool.Model.Dashboard.Setups;

namespace Onboarding.Tool.Server.Services.Onboarding;

public interface IOnboardingDataService
{
    public Task<IEnumerable<BrandSetting>> GetSetupInstanceSettingsAsync(Instance instance);

    public Task<IEnumerable<PortalAccount>> GetSetupInstancePortalAccountsAsync(Instance instance);

    public Task<IEnumerable<Image>> GetSetupImagesAsync(Instance instance);

    public Task<IEnumerable<string>> GetSetupUserEmailsAsync(Instance instance);

    public Task<IEnumerable<Branch>> GetSetupBranchesAsync(Instance instance);

    public Task<IEnumerable<BrandSetting>> GetSetupBranchSettingsAsync(Branch branch);

    public Task UpdateBranchDefaultStatusAsync(SaveBranch saveBranch, Instance instance);

    public Task<bool> DoesUserExistAsync(ImportUser user, Instance instance);

    public Task<EfInstance?> GetOrCreateInstanceAsync(int instanceId);

    public Task<OverviewDto> GetInstanceOverviewAsync(Instance instance);

    public Task<List<PostCodeDistrict>> GetBranchDistrictsByNameAsync(Instance instance, string name);

    public Task<List<BranchDto>> GetBranchOverviewAsync(Instance instance);

    public Task<BuildOverview> GetBuildOverviewAsync(Instance instance);

    public Task<List<Users>> GetUsersAsync(Instance instance);

    public List<BrandSettingConfig> GetBrandSettingConfig();

    public Task<SetupStateDto> GetSetupStateAsync(Instance instance);

    public Task<CardPreviewInfo> GetCardPreviewInfoAsync(Instance instance);

    public Task<byte[]> GetPreviewPdfAsync(Instance instance, int? width = null, int? height = null, int? x = null, int? y = null);
}
