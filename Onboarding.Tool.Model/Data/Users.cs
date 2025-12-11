using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Data;

public class Users
{
    public int Id { get; set; }

    public required string Username { get; set; }

    [JsonIgnore]
    public int InstanceId { get; set; }

    [JsonIgnore]
    public Instance? Instance { get; set; }
}
