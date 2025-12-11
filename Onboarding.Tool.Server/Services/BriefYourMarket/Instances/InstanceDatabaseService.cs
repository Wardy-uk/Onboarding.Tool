using Dapper;
using SixLabors.ImageSharp;
using System.Data.Common;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.BriefYourMarket.Branches;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;
using Onboarding.Tool.Model.Enums;
using Onboarding.Tool.Model.BriefYourMarket.Instances.Configurations;
using Onboarding.Tool.Model.BriefYourMarket.Reporting;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances;

public class InstanceDatabaseService : IInstanceDatabaseService
{
    private readonly IInstanceSqlConnectionFactory _instanceSqlConnectionFactory;
    private readonly IOnboardingDataService _onboardingDataService;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly BriefYourMarketInstanceHelper _briefYourMarketInstanceHelper;

    public InstanceDatabaseService(
        IInstanceSqlConnectionFactory instanceSqlConnectionFactory,
        IOnboardingDataService onboardingDataService,
        IDashboardProgressService dashboardProgressService,
        BriefYourMarketInstanceHelper briefYourMarketInstanceHelper)
    {
        _instanceSqlConnectionFactory = instanceSqlConnectionFactory;
        _onboardingDataService = onboardingDataService;
        _dashboardProgressService = dashboardProgressService;
        _briefYourMarketInstanceHelper = briefYourMarketInstanceHelper;
    }

    public async Task<bool> CreateDeliveryAddressesAsync(Instance instance, List<DeliveryAddress> existingAddresses)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupDeliveryAddresses", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        using DbConnection instanceConnection = await _instanceSqlConnectionFactory.CreateInstanceConnectionAsync(instance.Domain);
        IEnumerable<Branch> branches = await _onboardingDataService.GetSetupBranchesAsync(instance);

        foreach (Branch branch in branches)
        {
            if (existingAddresses.Select(d => d.Name).Contains(branch.Name))
                continue;

            int addressId = await CreateAddressAsync(instanceConnection, branch);

            await CreateDeliveryAddressInternalAsync(instanceConnection, branch, addressId);
        }

        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupDeliveryAddresses", TemplateConfirmationState.Confirmed);
        return true;
    }

    public async Task AddDeliveryAddressesToUsersAsync(Instance instance)
    {
        using DbConnection instanceConnection = await _instanceSqlConnectionFactory.CreateInstanceConnectionAsync(instance.Domain);

        const string sql = """
            INSERT INTO UserDeliveryAddress (UserID, DeliveryAddressID)
            SELECT u.UserID, d.DeliveryAddressID
            FROM [User] u
            CROSS JOIN DeliveryAddress d
            WHERE NOT EXISTS (
                SELECT 1
                FROM UserDeliveryAddress uda
                WHERE uda.UserID = u.UserID AND uda.DeliveryAddressID = d.DeliveryAddressID
            )
            """;

        await instanceConnection.ExecuteAsync(sql);
    }

    public async Task AddUsersToGroupsAsync(Instance instance)
    {
        using DbConnection instanceConnection = await _instanceSqlConnectionFactory.CreateInstanceConnectionAsync(instance.Domain);

        const string sql = """
            INSERT INTO GroupMembership (UserID, UserGroupID)
            SELECT u.UserID, ug.UserGroupID
            FROM [User] u
            CROSS JOIN UserGroup ug
            WHERE NOT EXISTS (
                SELECT 1
                FROM GroupMembership gm
                WHERE gm.UserID = u.UserID AND gm.UserGroupID = ug.UserGroupID
            )
            """;

        await instanceConnection.ExecuteAsync(sql);
    }

    public async Task AddScheduledReportConfigurationsAsync(Instance instance)
    {
        using DbConnection instanceConnection = await _instanceSqlConnectionFactory.CreateInstanceConnectionAsync(instance.Domain);
        IEnumerable<InstanceReportDefinition> instanceReportDefinitions = await GetInstanceReportsAsync(instanceConnection);

        foreach (ScheduledReport reportingConfiguration in _briefYourMarketInstanceHelper.ScheduledReportConfiguration.ScheduledReports)
        {
            await CreateScheduledReportConfigurationAsync(instanceConnection, reportingConfiguration, instanceReportDefinitions);
        }

        await AddUsersAsRecipientsToScheduledReportsAsync(instanceConnection);
    }

    private static async Task AddUsersAsRecipientsToScheduledReportsAsync(DbConnection connection)
    {
        const string sql = """
            INSERT INTO ScheduledReportRecipientList (UserID, ScheduledReportId)
            SELECT u.UserID, s.ScheduledReportId
            FROM [User] u
            CROSS JOIN ScheduledReport s
            WHERE u.LastName <> '(BYM Staff)'
            AND NOT EXISTS (
                SELECT 1
                FROM ScheduledReportRecipientList srrl
                WHERE srrl.UserID = u.UserID AND srrl.ScheduledReportId = s.ScheduledReportId
            )
            """;

        await connection.ExecuteAsync(sql);
    }

    private static async Task<IEnumerable<InstanceReportDefinition>> GetInstanceReportsAsync(DbConnection connection)
    {
        const string sql = """
            SELECT [ReportDefinitionId],
            [Name]
            FROM [ReportDefinition] (NOLOCK)
            """;

        return await connection.QueryAsync<InstanceReportDefinition>(sql);
    }

    private static async Task CreateScheduledReportConfigurationAsync(DbConnection connection, ScheduledReport scheduledReport, IEnumerable<InstanceReportDefinition> definitions)
    {
        int? definitionId = definitions.Where(d => d.Name == scheduledReport.Name).FirstOrDefault()?.ReportDefinitionId;
        if (definitionId == null)
            return;

        int createdById = await GetReportCreatorAsync(connection);

        const string sql = """
            INSERT INTO [ScheduledReport] ([ReportDefinitionId], [BrandId], [Name], [Active], [Recipient], [EvaluationFrequency], [LastEvaluated], [CreatedBy], [LastModified], [LastModifiedBy], [ReceiveHtml], [ReceiveCsv], [Parameters])
            VALUES (@ReportId, -1, @ReportName, 0, NULL, @Frequency, DATEADD(DD, -7, GETDATE()), @CreatedBy, GETDATE(), @CreatedBy, 0, 1, '<?xml version="1.0" encoding="utf-16"?><Parameters />')
            """;

        await connection.ExecuteAsync(sql, new 
        {
            ReportId = definitionId,
            ReportName = scheduledReport.Configuration.Name,
            Frequency = scheduledReport.Configuration.Frequency,
            CreatedBy = createdById
        });
    }

    private static async Task<int> GetReportCreatorAsync(DbConnection connection)
    {
        const string sql = """
            SELECT MAX([UserId])
            FROM [User] (NOLOCK)
            """;

        return await connection.ExecuteScalarAsync<int>(sql);
    }

    private static async Task<int> CreateAddressAsync(DbConnection connection, Branch branch)
    {
        const string sql = """
                INSERT INTO [Address] ([Organisation], [Line1], [Line2], [Line3], [Town], [PostCode], [Valid])
                OUTPUT inserted.[AddressID]
                VALUES (@Name, @Line1, @Line2, @Line3, @Town, @Postcode, 1)
                """;

        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            Name = branch.Name,
            Line1 = branch.Address1,
            Line2 = branch.Address2,
            Line3 = branch.Address3,
            Town = branch.Town,
            Postcode = string.Join(" ", branch.PostCode1, branch.PostCode2)
        });
    }

    private static async Task CreateDeliveryAddressInternalAsync(DbConnection connection, Branch branch, int address)
    {
        const string sql = """
            INSERT INTO [DeliveryAddress] ([Name], [Recipient], [IsDefault], [AddressId], [Region], [ContactTel], [ContactEmail])
            VALUES (@Name, @Name, @IsDefault, @Address, @Town, @Phone, @Email)
            """;
        await connection.ExecuteAsync(sql, new
        {
            Name = branch.Name,
            Address = address,
            Town = branch.Town,
            Phone = branch.SalesPhoneNumber,
            Email = branch.SalesEmail,
            IsDefault = branch.Default
        });
    }
}
