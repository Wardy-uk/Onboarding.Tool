using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Data;

public class PortalAccount
{
    public int Id { get; set; }

    public required string PortalName { get; set; }

    [JsonIgnore]
    public int InstanceId { get; set; }

    [JsonIgnore]
    public Instance? Instance { get; set; }
}
