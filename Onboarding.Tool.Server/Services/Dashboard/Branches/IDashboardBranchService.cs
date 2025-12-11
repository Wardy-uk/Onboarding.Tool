using Onboarding.Tool.Model.Dashboard.Branches;
using Onboarding.Tool.Model.Data;
using InstanceModel = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Services.Dashboard.Branches;

public interface IDashboardBranchService
{
    public Task<Branch?> CreateBranchAsync(int instanceId, ImportBranch createdBranch);

    public Task<Branch?> UpdateBranchAsync(SaveBranch updatedBranch, InstanceModel instance);

    public Task<bool> DeleteBranchAsync(int branchId, int instanceId);

    public Task<List<Branch>> ImportBranchesAsync(int instanceId, List<ImportBranch> importBranches);
}
