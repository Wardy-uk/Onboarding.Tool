using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Data;

public class BranchSetting
{
    public int Id { get; set; }

    public required string Setting { get; set; }

    public required string Value { get; set; }

    [JsonIgnore]
    public int BranchId { get; set; }

    [JsonIgnore]
    public Branch? Branch { get; set; }
}
