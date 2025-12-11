using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Data;

public class BranchBuildDistrictSector
{
    public int Id { get; set; }

    public required string Sector { get; set; }

    [JsonIgnore]
    public int DistrictId { get; set; }

    [JsonIgnore]
    public BranchBuildDistrict? District { get; set; }
}
