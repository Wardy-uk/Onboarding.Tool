using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Data;

public class BranchBuildDistrict
{
    public int Id { get; set; }

    public required string District { get; set; }

    public required bool AllSectors { get; set; }

    public ICollection<BranchBuildDistrictSector>? Sectors { get; set; }

    [JsonIgnore]
    public int BranchId { get; set; }

    [JsonIgnore]
    public Branch? Branch { get; set; }
}
