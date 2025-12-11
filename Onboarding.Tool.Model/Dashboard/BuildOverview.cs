using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Model.Dashboard;

public class BuildOverview
{
    public List<BuildBranchDto> Branches { get; set; } = new();

    public List<PortalAccount> PortalAccounts { get; set; } = new();
}
