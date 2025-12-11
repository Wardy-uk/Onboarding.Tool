namespace Onboarding.Tool.Model.Dashboard;

public class BuildDistrictDto
{
    public required int DistrictId { get; set; }

    public required string District { get; set; }

    public required bool AllSectors { get; set; }

    public required List<string> Sectors { get; set; }
}
