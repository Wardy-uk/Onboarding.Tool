using Dapper;
using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BriefYourMarket.Integrations;
using Onboarding.Tool.Model.Robocop;
using System.Data.SqlClient;
using InstanceModel = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using TemplateModel = Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels.Template;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Config;

public class ConfigDatabaseService : IConfigDatabaseService
{
    private readonly ConfigSettings _options;
    private readonly BriefYourMarketInstanceHelper _briefYourMarketInstanceHelper;

    public ConfigDatabaseService(
        IOptions<ConfigSettings> options, 
        BriefYourMarketInstanceHelper briefYourMarketInstanceHelper)
    {
        _options = options.Value;
        _briefYourMarketInstanceHelper = briefYourMarketInstanceHelper;
    }

    public async Task<string> GetInstanceConnectionStringAsync(string instance)
    {
        using SqlConnection configConnection = new(_options.ConnectionString);

        string sql = """
            SELECT TOP 1 [Instance].[InstanceID] AS [Id],
            [ServerSettings].[Value] AS [Server],
            [DatabaseSettings].[Value] AS [Database]
            FROM [Domain] (NOLOCK)
            LEFT JOIN [Instance] (NOLOCK) ON [Instance].[InstanceID] = [Domain].[InstanceID]
            LEFT JOIN [Settings] [ServerSettings] (NOLOCK) ON [ServerSettings].[InstanceID] = [Instance].[InstanceID] AND [ServerSettings].[Name] = 'DatabaseServer'
            LEFT JOIN [Settings] [DatabaseSettings] (NOLOCK) ON [DatabaseSettings].[InstanceID] = [Instance].[InstanceID] AND [DatabaseSettings].[Name] = 'Database'
            WHERE [Domain].[Host] = @InstanceDomain
            """;

        InstanceModel result = await configConnection.QueryFirstOrDefaultAsync<InstanceModel>(sql, new { InstanceDomain = instance }) ?? throw new Exception("Unable to find instance " + instance);

        SqlConnectionStringBuilder sqlConnectionStringBuilder = new(_options.InstanceConnectionStringTemplate)
        {
            DataSource = string.Format("{0}{1}", result.Server, _options.DnsLevelAppend),
            InitialCatalog = result.Database
        };

        return sqlConnectionStringBuilder.ConnectionString;
    }

    public async Task<bool> DoesFileExistAsync(int directoryId, string fileName)
    {
        using SqlConnection configConnection = new(_options.ConnectionString);

        string sql = """
            SELECT COUNT(*)
            FROM [Image] (NOLOCK)
            WHERE [DirectoryID] = @Directory
            AND [FileName] = @FileName
            """;

        int count = await configConnection.QueryFirstOrDefaultAsync<int>(sql, new { Directory = directoryId, FileName = fileName });
        return count > 0;
    }

    public async Task DeleteFileAsync(int directoryId, string fileName)
    {
        using SqlConnection configConnection = new(_options.ConnectionString);

        string sql = """
            DELETE FROM [Image]
            WHERE [DirectoryID] = @Directory
            AND [FileName] = @FileName
            """;

        int count = await configConnection.ExecuteAsync(sql, new { Directory = directoryId, FileName = fileName });
    }

    public async Task<List<EligibleInstance>> GetInstancesEligibleForSetupAsync()
    {
        using SqlConnection configConnection = new(_options.ConnectionString);

        const string sql = """
            SELECT DISTINCT([Settings].[InstanceId]), MAX([Domain].[Host]) [Host]
            FROM [Domain] (NOLOCK)
            INNER JOIN [Settings] (NOLOCK) ON [Settings].[InstanceID] = [Domain].[InstanceID] AND [Settings].[Name] = 'SetupAutomation.Eligible' AND [Settings].[Value] = 'True'
            WHERE [Domain].[Host] NOT LIKE 'www.%'
            GROUP BY [Settings].[InstanceId]
            """;

        IEnumerable<EligibleInstance> eligibleInstances = await configConnection.QueryAsync<EligibleInstance>(sql);
        return [.. eligibleInstances];
    }

    public async Task<bool> IsInstanceEligibleForSetupAsync(InstanceModel instance)
    {
        using SqlConnection configConnection = new(_options.ConnectionString);

        const string sql = """
            SELECT DISTINCT([Settings].[InstanceId]), MAX([Domain].[Host]) [Host]
            FROM [Domain] (NOLOCK)
            INNER JOIN [Settings] (NOLOCK) ON [Settings].[InstanceID] = [Domain].[InstanceID] AND [Settings].[Name] = 'SetupAutomation.Eligible' AND [Settings].[Value] = 'True'
            WHERE [Domain].[Host] NOT LIKE 'www.%'
            AND [Settings].[InstanceId] = @InstanceId
            GROUP BY [Settings].[InstanceId]
            """;

        EligibleInstance? result = await configConnection.QueryFirstOrDefaultAsync<EligibleInstance?>(sql, new { InstanceId = instance.Id });
        return (result != null);
    }

    public async Task<InstanceModel> GetInstanceDefinitionAsync(string instance)
    {
        using SqlConnection configConnection = new(_options.ConnectionString);
        string[] parts = instance.Split(".");

        string sql = """
            SELECT TOP 1 [Instance].[InstanceID] AS [Id],
            @Domain AS [Domain],
            @Subdomain AS [Subdomain],
            [ServerSettings].[Value] AS [Server],
            [DatabaseSettings].[Value] AS [Database]
            FROM [Domain] (NOLOCK)
            LEFT JOIN [Instance] (NOLOCK) ON [Instance].[InstanceID] = [Domain].[InstanceID]
            LEFT JOIN [Settings] [ServerSettings] (NOLOCK) ON [ServerSettings].[InstanceID] = [Instance].[InstanceID] AND [ServerSettings].[Name] = 'DatabaseServer'
            LEFT JOIN [Settings] [DatabaseSettings] (NOLOCK) ON [DatabaseSettings].[InstanceID] = [Instance].[InstanceID] AND [DatabaseSettings].[Name] = 'Database'
            WHERE [Domain].[Host] = @InstanceDomain
            """;

        InstanceModel result = await configConnection.QueryFirstOrDefaultAsync<InstanceModel>(sql, new { Domain = instance, Subdomain = parts[0], InstanceDomain = instance }) ?? throw new Exception("Unable to find instance " + instance);
        return result;
    }

    public async Task<IntegrationDefinition?> GetInstanceIntegrationDefinitionAsync(string instance)
    {
        using SqlConnection configConnection = new(_options.ConnectionString);
        InstanceModel instanceDetails = await GetInstanceDefinitionAsync(instance);
        List<int> validDefinitionIds = _briefYourMarketInstanceHelper.IntegrationConfiguration.Integrations.Select(i => i.DefinitionId).ToList();

        string sql = """
            SELECT [IntegrationDefinition].[IntegrationDefinitionID] [DefinitionId],
            [IntegrationDefinition].[Name]
            FROM [InstanceIntegration] (NOLOCK)
            INNER JOIN [IntegrationDefinition] (NOLOCK) ON [InstanceIntegration].[IntegrationDefinitionStoreID] = [IntegrationDefinition].[IntegrationDefinitionID] AND [InstanceIntegration].[InstanceID] = @InstanceId
            WHERE [IntegrationDefinition].[IntegrationDefinitionID] IN @ValidDefinitions
            ORDER BY [InstanceIntegration].[IntegrationID] DESC
            """;

        return await configConnection.QueryFirstOrDefaultAsync<IntegrationDefinition?>(sql, new
        {
            InstanceId = instanceDetails.Id,
            ValidDefinitions = validDefinitionIds
        });
    }

    public async Task<int> GetCardIdByNameAsync(InstanceModel instance, string name)
    {
        using SqlConnection configConnection = new(_options.ConnectionString);

        if (!name.StartsWith('%'))
            name = "%" + name;
        if (!name.EndsWith('%'))
            name += "%";

        const string sql = """
            SELECT TOP 1 [product].[ProductID] 
            FROM [DirectMailInstanceLibrary] lib WITH(NOLOCK) 
            LEFT JOIN [DirectMailLibraryProduct] libproduct WITH(NOLOCK) ON libproduct.LibraryID = lib.LibraryID 
            LEFT JOIN [DirectMailProduct] product WITH(NOLOCK) ON product.ProductID = libproduct.ProductID 
            WHERE lib.InstanceID = @InstanceId AND product.Title LIKE @SearchTerm
            """;

        int result = await configConnection.QueryFirstOrDefaultAsync<int>(sql, new { InstanceId = instance.Id, SearchTerm = name });
        return result;
    }

    public async Task<IEnumerable<TemplateModel>> GetPrintCardsAsync(InstanceModel instance)
    {
        using SqlConnection configConnection = new(_options.ConnectionString);

        const string sql = """
            SELECT [product].[ProductID] [Id],
            [product].[Title] [Name]
            FROM [DirectMailInstanceLibrary] lib WITH(NOLOCK) 
            LEFT JOIN [DirectMailLibraryProduct] libproduct WITH(NOLOCK) ON libproduct.LibraryID = lib.LibraryID 
            LEFT JOIN [DirectMailProduct] product WITH(NOLOCK) ON product.ProductID = libproduct.ProductID 
            WHERE lib.InstanceID = @InstanceId
            """;

        return await configConnection.QueryAsync<TemplateModel>(sql, new { InstanceId = instance.Id });
    }

    public async Task<bool> AddLibraryCardtoInstanceAsync(InstanceModel instance, int libraryId)
    {
        using SqlConnection connection = new(_options.ConnectionString);

        const string sql = """
            IF NOT EXISTS (
                SELECT 1
                FROM [DirectMailInstanceLibrary]
                WHERE [LibraryID] = @LibraryId
                  AND [InstanceID] = @InstanceId
            )
            BEGIN
                INSERT INTO [DirectMailInstanceLibrary] ([LibraryID], [InstanceID])
                VALUES (@LibraryId, @InstanceId);
            END
            """;

        int rowsAffected = await connection.ExecuteAsync(sql, new
        {
            LibraryId = libraryId,
            InstanceId = instance.Id
        });

        return rowsAffected > 0;
    }
}
