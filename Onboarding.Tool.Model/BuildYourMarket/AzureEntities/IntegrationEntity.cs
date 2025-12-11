using Microsoft.WindowsAzure.Storage.Table;

namespace Onboarding.Tool.Model.BuildYourMarket.AzureEntities;

public class IntegrationEntity : TableEntity
{
    public IntegrationEntity(string key, string row)
    {
        PartitionKey = key;
        RowKey = row;
    }

    public required string Domain { get; set; }
    
    public required string ProviderType { get; set; }
}
