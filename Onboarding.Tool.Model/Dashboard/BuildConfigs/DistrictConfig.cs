namespace Onboarding.Tool.Model.Dashboard.BuildConfigs;

public class DistrictConfig
{
    public int? DistrictId { get; init; }

    public required string District { get; init; }

    public int BranchId { get; init; }

    public required bool AllSectors { get; init; }

    public required List<string> Sectors { get; init; }
}
