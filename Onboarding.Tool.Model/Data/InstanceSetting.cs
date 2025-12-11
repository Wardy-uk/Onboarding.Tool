using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Data;

public class InstanceSetting
{
    public int Id { get; set; }

    public required string Setting { get; set; }

    public required string Value { get; set; }

    [JsonIgnore]
    public int InstanceId { get; set; }

    [JsonIgnore]
    public Instance? Instance { get; set; }
}
