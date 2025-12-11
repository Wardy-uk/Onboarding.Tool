using Dapper;
using System.Data.Common;
using System.Text;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.BriefYourMarket.Automation;
using Onboarding.Tool.Model.BriefYourMarket.Integrations;
using Onboarding.Tool.Model.Enums;
using Onboarding.Tool.Model.BriefYourMarket.Branches;
using Onboarding.Tool.Model.BriefYourMarket.Brands;
using TemplateModel = Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels.Template;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances.Automation;

public class InstanceAutomationService : IInstanceAutomationService
{
    private readonly IInstanceSqlConnectionFactory _instanceSqlConnectionFactory;
    private readonly IConfigDatabaseService _configDatabaseService;
    private readonly IInstanceApiService _instanceApiService;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly IOnboardingDataService _onboardingDataService;
    private readonly BriefYourMarketInstanceHelper _briefYourMarketInstanceHelper;

    public InstanceAutomationService(
        IInstanceSqlConnectionFactory instanceSqlConnectionFactory, 
        IConfigDatabaseService configDatabaseService, 
        IInstanceApiService instanceApiService,
        IDashboardProgressService dashboardProgressService,
        IOnboardingDataService onboardingDataService,
        BriefYourMarketInstanceHelper briefYourMarketInstanceHelper)
    {
        _instanceSqlConnectionFactory = instanceSqlConnectionFactory;
        _configDatabaseService = configDatabaseService;
        _instanceApiService = instanceApiService;
        _dashboardProgressService = dashboardProgressService;
        _onboardingDataService = onboardingDataService;
        _briefYourMarketInstanceHelper = briefYourMarketInstanceHelper;
    }

    public async Task<List<int>> AddDefaultEmailTriggersToInstanceAsync(Instance instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupAutomatedEmails", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return new List<int>();

        using DbConnection connection = await _instanceSqlConnectionFactory.CreateInstanceConnectionAsync(instance.Domain);

        IntegrationDefinition? integrationDefinition = await _configDatabaseService.GetInstanceIntegrationDefinitionAsync(instance.Domain);
        List<TemplateModel> templates = await _instanceApiService.GetInstanceTemplatesAsync(instance.Subdomain, "api/templates");
        List<int> triggerIds = new();

        foreach (EmailAutomation automation in _briefYourMarketInstanceHelper.AutomationConfiguration.Email)
        {
            int? triggerId = await AddEmailTriggerToInstanceAsync(connection, automation, integrationDefinition, templates);

            if (triggerId == null)
                continue;

            triggerIds.Add(triggerId.Value);
        }

        IEnumerable<BrandSetting> settings = await _onboardingDataService.GetSetupInstanceSettingsAsync(instance);
        await UpdateBrandSettingsInEmailTriggersAsync(connection, settings, triggerIds);

        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupAutomatedEmails", TemplateConfirmationState.Confirmed);
        return triggerIds;
    }

    public async Task<List<int>> AddDefault2020TriggersToInstanceAsync(Instance instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupAutomated2020s", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return new List<int>();

        using DbConnection connection = await _instanceSqlConnectionFactory.CreateInstanceConnectionAsync(instance.Domain);

        IntegrationDefinition? integrationDefinition = await _configDatabaseService.GetInstanceIntegrationDefinitionAsync(instance.Domain);
        IEnumerable<TemplateModel> templates = await _configDatabaseService.GetPrintCardsAsync(instance);
        IEnumerable<Branch> branches = await _onboardingDataService.GetSetupBranchesAsync(instance);
        int approver = await GetPrintApprover(connection);
        List<int> triggerIds = new();

        foreach (Branch branch in branches)
        {
            foreach (CardAutomation automation in _briefYourMarketInstanceHelper.AutomationConfiguration.Auto2020)
            {
                int? triggerId = await Add2020TriggerToInstanceAsync(connection, branch, automation, integrationDefinition, templates, approver);

                if (triggerId == null)
                    continue;

                triggerIds.Add(triggerId.Value);
            }
        }

        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupAutomated2020s", TemplateConfirmationState.Confirmed);
        return triggerIds;
    }

    private static async Task<int?> AddEmailTriggerToInstanceAsync(DbConnection connection, EmailAutomation emailAutomation, IntegrationDefinition? integrationDefinition, List<TemplateModel> templates)
    {
        int templateId = GetTemplateId(emailAutomation, templates);
        string eventName = GetEventName(emailAutomation, integrationDefinition);
        string text = DecodeBase64Representation(emailAutomation.Text);
        string html = DecodeBase64Representation(emailAutomation.Html);

        string sql = """
            INSERT INTO [Trigger]
            (
                [Name],
                [MessageSubject],
                [TriggerTypeTypeName],
                [TriggerEventName],
                [TemplateID],
                [SendToList],
                [SendToFilter],
                [TextBody],
                [HTMLBody],
                [Status],
                [OccurrenceAmount],
                [OccurrenceType],
                [EvaluateAt],
                [DateCreated],
                [CreatedBy],
                [MessageType],
                [LetterID] ,
                [TriggerFilter],
                [FromAddress],
                [LastEvaluated],
                [CustomSql],
                [CreditGroupToDebitID],
                [CampaignId],
                [CanvasCardConfig],
                [DisplayOrder],
                [EvaluationMode],
                [EvaluationModeRedirectAddress],
                [EvaluationModeExpiration],
                [RestrictRecipients],
                [Consented],
                [LegitimateInterest],
                [BranchID],
                [TrackedPhoneNumber]
            )
            OUTPUT inserted.[TriggerID]
            VALUES
            (
                @Name,
                @Subject,
                'BriefYourMarket.Model.Automation.ContactPropertyTriggerType, BriefYourMarket.Model.Automation, Version=2025.4.10.49605, Culture=neutral, PublicKeyToken=null',
                @Event,
                @TemplateId,
                NULL,
                NULL,
                @Text,
                @Html,
                1,
                @OccurrenceAmount,
                @OccurrenceType,
                10,
                GETDATE(),
                1,
                0,
                NULL,
                '<TriggerFilter xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><Expressions /></TriggerFilter>',
                NULL,
                NULL,
                NULL,
                NULL,
                NULL,
                NULL,
                1111,
                0,
                '',
                DATEADD(YY, 1, GETDATE()),
                0,
                1,
                1,
                NULL,
                NULL
            )
            """;

        return await connection.ExecuteScalarAsync<int?>(sql, new
        {
            Name = emailAutomation.Name,
            Subject = emailAutomation.Subject,
            Event = eventName,
            TemplateId = templateId,
            Text = text,
            Html = html,
            OccurrenceAmount = emailAutomation.EventOccurence,
            OccurrenceType = emailAutomation.EventOccurenceType
        });
    }

    private static async Task<int?> Add2020TriggerToInstanceAsync(DbConnection connection, Branch branch, CardAutomation cardAutomation, IntegrationDefinition? integrationDefinition, IEnumerable<TemplateModel> templates, int approver)
    {
        int templateId = GetPrintTemplateId(cardAutomation, branch, templates);
        string eventName = GetPrintEventName(cardAutomation, integrationDefinition);
        TriggerFilter triggerFilter = GetPrintTriggerFilter(cardAutomation, integrationDefinition);

        string triggerFilterExpression = string.Format("""
            ﻿<TriggerFilter xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
            	<Expressions>
            		<Expression xsi:type="TriggerValueComparisonExpression">
            			<PropertyName>{0}</PropertyName>
            			<EvaluateAs>And</EvaluateAs>
            			<Value xsi:type="xsd:string">{1}</Value>
            			<Operator>
            				<Name>Contains</Name>
            			</Operator>
            		</Expression>
            	</Expressions>
            </TriggerFilter>
            """, triggerFilter.Field, triggerFilter.Value);

        string auto2020Config = string.Format("""
            <CanvassingCardTriggerConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                <PaperTypeId>1</PaperTypeId>
                <LaminationId>1</LaminationId>
                <StockId>2</StockId>
                <DeliveryMethodId>2</DeliveryMethodId>
                <ProductId>{0}</ProductId>
                <Quantity>40</Quantity>
                <TotalCost>22</TotalCost>
                <DeliveryAddressId>1</DeliveryAddressId>
                <BypassApproval>false</BypassApproval>
                <RestrictImageQuality>false</RestrictImageQuality>
                <ApproveByField>false</ApproveByField>
                    <ApproverId>{1}</ApproverId>
                <SendNotificationToApprover>true</SendNotificationToApprover>
                <ApprovalField />
                <TwentyTwentyConfiguration>
                    <IsTwentyTwentyCampaign>true</IsTwentyTwentyCampaign>
                    <HouseNumberField>Address1</HouseNumberField>
                    <PostCodePrefixField>Postcode1</PostCodePrefixField>
                    <PostCodeSuffixField>Postcode2</PostCodeSuffixField>
                    <UsePostCodePrefixSuffix>true</UsePostCodePrefixSuffix>
                    <ApplicableTwentyTwentyContacts>0</ApplicableTwentyTwentyContacts>
                    <SearchConfiguration />
                    <RestrictTaxBand>false</RestrictTaxBand>
                </TwentyTwentyConfiguration>
                <FieldValueApprovers />
                <ExcludeExistingContacts>false</ExcludeExistingContacts>
            </CanvassingCardTriggerConfiguration>
            """, templateId, approver);

        const string sql = """
            INSERT INTO [Trigger]
            (
                [Name],
                [TriggerTypeTypeName],
                [TriggerEventName],
                [Status],
                [OccurrenceAmount],
                [OccurrenceType],
                [EvaluateAt],
                [DateCreated],
                [CreatedBy],
                [MessageType],
                [TriggerFilter],
                [CanvasCardConfig],
                [DisplayOrder],
                [EvaluationMode],
                [EvaluationModeExpiration],
                [RestrictRecipients],
                [Consented],
                [LegitimateInterest]
            )
            OUTPUT inserted.[TriggerID]
            VALUES
            (
                @Name,
                'Jwayela.BriefYourMarket.Model.Triggers.TriggerTypes.ProductPropertyTriggerType, BriefYourMarket.Model, Version=2021.12.9.11909, Culture=neutral, PublicKeyToken=null',
                @EventName,
                1,
                1,
                3,
                10,
                GETDATE(),
                @Approver,
                3,
                @TriggerFilterExpression,
                @Config,
                (
                    SELECT MAX(DisplayOrder) + 1
                    FROM [Trigger]
                ),
                0,
                GETDATE(),
                2,
                0,
                0
            )
            """;

        return await connection.ExecuteScalarAsync<int?>(sql, new
        {
            Name = string.Format("{0} - {1}", cardAutomation.Name, branch.Name),
            EventName = eventName,
            Approver = approver,
            TriggerFilterExpression = triggerFilterExpression,
            Config = auto2020Config,
        });

    }

    private static async Task<int> GetPrintApprover(DbConnection connection)
    {
        const string sql = """
            SELECT MAX(UserId)
            FROM [User] (NOLOCK)
            """;

        return await connection.ExecuteScalarAsync<int>(sql);
    }

    private static string GetEventName(EmailAutomation automation, IntegrationDefinition? integrationDefinition)
    {
        string defaultEvent = automation.EventNames.FirstOrDefault(e => e.Name == "Default")?.Field ?? "DateCreated";

        if (integrationDefinition == null)
            return defaultEvent;

        string? matchingEvent = automation.EventNames.FirstOrDefault(e => e.Name == integrationDefinition.Name)?.Field;
        return matchingEvent ?? defaultEvent;
    }

    private static int GetTemplateId(EmailAutomation automation, List<TemplateModel> templates)
    {
        int defaultTemplate = templates.FirstOrDefault(t => t.Name == "Blank")?.Id ?? 1;
        int? matchingTemplate = templates.FirstOrDefault(t => t.Name == automation.Template)?.Id;

        return matchingTemplate ?? defaultTemplate;
    }

    private static int GetPrintTemplateId(CardAutomation cardAutomation, Branch branch, IEnumerable<TemplateModel> templates)
    {
        int? templateId = templates.FirstOrDefault(t => t.Name.Contains(cardAutomation.TemplateName) && t.Name.Contains(branch.Name))?.Id;

        return templateId ?? templates.First().Id;
    }

    private static string GetPrintEventName(CardAutomation automation, IntegrationDefinition? integrationDefinition)
    {
        string defaultEvent = automation.EventNames.FirstOrDefault(e => e.Name == "Default")?.Field ?? "Product.DateCreated";

        if (integrationDefinition == null)
            return defaultEvent;

        string? matchingEvent = automation.EventNames.FirstOrDefault(e => e.Name == integrationDefinition.Name)?.Field;
        return matchingEvent ?? defaultEvent;
    }

    private static TriggerFilter GetPrintTriggerFilter(CardAutomation automation, IntegrationDefinition? integrationDefinition)
    {
        TriggerFilter filter = automation.TriggerFilter.FirstOrDefault(e => e.Name == "Default") ?? new()
        { 
            Name = "Default",
            Field = "Product.Status",
            Value = "For Sale"
        };

        if (integrationDefinition == null)
        {
            return filter;
        }

        TriggerFilter? matchingFilter = automation.TriggerFilter.FirstOrDefault(e => e.Name == integrationDefinition.Name);
        return matchingFilter ?? filter;
    }

    private static string DecodeBase64Representation(string text)
    {
        byte[] bytes = Convert.FromBase64String(text);
        return Encoding.UTF8.GetString(bytes);
    }

    private static async Task UpdateBrandSettingsInEmailTriggersAsync(DbConnection connection, IEnumerable<BrandSetting> settings, List<int> triggerIds)
    {
        string primaryColor = settings.Where(s => s.Setting == "theme.colourPrimary").First().Value;

        const string sql = """
            UPDATE [Trigger] SET [HTMLBody] = REPLACE(CAST(HTMLBody as VARCHAR(MAX)), '$Contact.GetBrandSetting(''theme.colourPrimary'')', @PrimaryThemeColour)
            WHERE [TriggerID] IN @TriggerIds
            """;

        await connection.ExecuteAsync(sql, new
        {
            PrimaryThemeColour = primaryColor,
            TriggerIds = triggerIds
        });
    }
}
