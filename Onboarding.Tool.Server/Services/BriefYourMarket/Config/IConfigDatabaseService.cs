using Onboarding.Tool.Model.BriefYourMarket.Integrations;
using Onboarding.Tool.Model.Robocop;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using TemplateModel = Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels.Template;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Config;

public interface IConfigDatabaseService
{
    public Task<string> GetInstanceConnectionStringAsync(string instance);

    public Task<bool> DoesFileExistAsync(int directoryId, string fileName);

    public Task DeleteFileAsync(int directoryId, string fileName);

    public Task<List<EligibleInstance>> GetInstancesEligibleForSetupAsync();

    public Task<bool> IsInstanceEligibleForSetupAsync(Instance instance);

    public Task<Instance> GetInstanceDefinitionAsync(string instance);

    public Task<IntegrationDefinition?> GetInstanceIntegrationDefinitionAsync(string instance);

    public Task<int> GetCardIdByNameAsync(Instance instance, string name);

    public Task<IEnumerable<TemplateModel>> GetPrintCardsAsync(Instance instance);

    public Task<bool> AddLibraryCardtoInstanceAsync(Instance instance, int libraryId);
}
