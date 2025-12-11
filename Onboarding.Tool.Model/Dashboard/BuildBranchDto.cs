using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Model.Dashboard;

public class BuildBranchDto
{
    public required int Id { get; set; }

    public required string Name { get; set; }

    public IList<BuildDistrictDto>? Districts { get; set; }
}
