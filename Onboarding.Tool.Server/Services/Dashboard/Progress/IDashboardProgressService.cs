using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Server.Services.Dashboard.Progress;

public interface IDashboardProgressService
{
    public Task<bool> IsSetupStepStatusAsync(Instance instance, string task, TemplateConfirmationState confirmationState);

    public Task SetSetupStatusAsync(Instance instance, string task, TemplateConfirmationState confirmationState);
}
