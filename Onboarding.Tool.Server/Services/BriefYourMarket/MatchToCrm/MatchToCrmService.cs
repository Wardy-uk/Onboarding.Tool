using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.BriefYourMarket.Integrations;
using Onboarding.Tool.Model.BuildYourMarket.AzureEntities;
using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.MatchToCrm;

public class MatchToCrmService : IMatchToCrmService
{
    private readonly IConfigDatabaseService _configDatabaseService;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly BuildSettings _buildSettings;

    public MatchToCrmService(
        IConfigDatabaseService configDatabaseService, 
        IDashboardProgressService dashboardProgressService,
        IOptions<BuildSettings> options)
    {
        _configDatabaseService = configDatabaseService;
        _dashboardProgressService = dashboardProgressService;
        _buildSettings = options.Value;
    }

    public async Task<bool> AddMatchToCrmToInstanceAsync(Instance instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupMatchToCrm", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        IntegrationDefinition? integrationDefinition = await _configDatabaseService.GetInstanceIntegrationDefinitionAsync(instance.Domain);

        if (integrationDefinition == null)
        {
            await _dashboardProgressService.SetSetupStatusAsync(instance, "setupMatchToCrm", TemplateConfirmationState.Confirmed);

            return true;
        }

        IntegrationEntity? entity = null;
        CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(_buildSettings.AzureStorageConnectionString);
        CloudTableClient cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
        CloudTable cloudTable = cloudTableClient.GetTableReference("Integration");

        if (integrationDefinition.Name == "Property Schema")
        {
            entity = new IntegrationEntity(instance.Subdomain, "")
            {
                Timestamp = DateTime.UtcNow,
                Domain = instance.Domain,
                ProviderType = "BuildYourMarket.Service.Common.Integration.PropertySchemaProvider"
            };
        }
        else if (integrationDefinition.Name.Contains("Vebra Alto"))
        {
            entity = new IntegrationEntity(instance.Subdomain, "")
            {
                Timestamp = DateTime.UtcNow,
                Domain = instance.Domain,
                ProviderType = "BuildYourMarket.Service.Common.Integration.VebraAltoProvider"
            };
        }

        if (entity != null)
        {
            TableOperation operation = TableOperation.Insert(entity);
            await cloudTable.ExecuteAsync(operation);
        }

        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupMatchToCrm", TemplateConfirmationState.Confirmed);

        return true;
    }
}
