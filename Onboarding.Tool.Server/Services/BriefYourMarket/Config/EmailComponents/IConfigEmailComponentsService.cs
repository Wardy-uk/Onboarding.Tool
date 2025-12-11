using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.Robocop.EmailComponents;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Config.EmailComponents;

public interface IConfigEmailComponentsService
{
    public Task<List<EmailComponentLibrary>> AddDefaultComponentLibrariesToInstanceAsync(Instance instance); 
}
