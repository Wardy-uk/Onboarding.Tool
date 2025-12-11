using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Data;

public class Branch
{
    public int Id { get; set; }

    public required bool IsDefault { get; set; }

    public required string Name { get; set; }

    public required string SalesEmail { get; set; }

    public required string SalesPhone { get; set; }

    public required string LettingsEmail { get; set; }

    public required string LettingsPhone { get; set; }

    public ICollection<BranchSetting> Settings { get; set; } = new List<BranchSetting>();

    public ICollection<BranchBuildDistrict> Districts { get; set; } = new List<BranchBuildDistrict>();

    [JsonIgnore]
    public int? AddressId { get; set; }

    public Address? Address { get; set; }

    [JsonIgnore]
    public int InstanceId { get; set; }

    [JsonIgnore]
    public Instance? Instance { get; set; } = null!;
}
