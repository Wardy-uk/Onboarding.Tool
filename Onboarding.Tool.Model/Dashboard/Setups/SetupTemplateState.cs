using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Model.Dashboard.Setups;

public class SetupTemplateState
{
    public required TemplateConfirmationState TemplatesConfirmed { get; set; }
    
    public required TemplateConfirmationState DirectMailConfirmed { get; set; }

    public required TemplateConfirmationState LetterheadConfirmed { get; set; }
}
