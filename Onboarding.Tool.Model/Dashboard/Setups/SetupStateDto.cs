namespace Onboarding.Tool.Model.Dashboard.Setups;

public class SetupStateDto
{
    public required SetupTemplateState TemplateSteps { get; set; }

    public required List<SetupAdditionalState> AdditionalSteps { get; set; }
}
