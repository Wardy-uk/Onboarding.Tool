using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Model.Data;

public class Instance
{
    public int? Id { get; set; }

    public TemplateConfirmationState TemplatesConfirmed { get; set; } = TemplateConfirmationState.NotCreated;

    public TemplateConfirmationState DirectMailConfirmed { get; set; } = TemplateConfirmationState.NotCreated;

    public TemplateConfirmationState LetterheadConfirmed { get; set; } = TemplateConfirmationState.NotCreated;

    public ICollection<Branch> Branches { get; set; } = new List<Branch>();

    public ICollection<PortalAccount> PortalAccounts { get; set; } = new List<PortalAccount>();

    public ICollection<Users>? Users { get; set; }

    public ICollection<Image>? Images { get; set; }

    public ICollection<InstanceSetting> Settings { get; set; } = new List<InstanceSetting>();

    public ICollection<InstanceSetupStep> SetupSteps { get; set; } = new List<InstanceSetupStep>();
}
