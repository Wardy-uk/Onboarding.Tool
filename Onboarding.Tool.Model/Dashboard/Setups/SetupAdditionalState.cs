using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Model.Dashboard.Setups;

public class SetupAdditionalState
{
    public required string SetupStep { get; set; }

    public required TemplateConfirmationState State { get; set; }
}
