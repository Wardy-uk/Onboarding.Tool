using Microsoft.TeamFoundation.SourceControl.WebApi;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Template;

public interface ITemplateService
{
    public Task<GitPush> CreateTemplateGitPushAsync(Instance instance);

    public Task<bool> UpdateInstanceTemplateStatusAsync(Instance instance, TemplateConfirmationState expectedState, TemplateConfirmationState newState);

    public Task<bool> UpdateInstanceDirectMailStatusAsync(Instance instance, TemplateConfirmationState expectedState, TemplateConfirmationState newState);

    public Task<TemplateConfirmationState> GetInstanceDirectMailStatusAsync(Instance instance);

    public Task<bool> UpdateInstanceLetterStatusAsync(Instance instance, TemplateConfirmationState expectedState, TemplateConfirmationState newState);
}
