using Onboarding.Tool.Model.Enums;
using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.Data;

public class InstanceSetupStep
{
    [JsonIgnore]
    public int Id { get; set; }

    public required string SetupStep { get; set; }

    public TemplateConfirmationState Status { get; set; } = TemplateConfirmationState.NotCreated;

    [JsonIgnore]
    public int? InstanceId { get; set; }

    [JsonIgnore]
    public Instance? Instance { get; set; }
}
