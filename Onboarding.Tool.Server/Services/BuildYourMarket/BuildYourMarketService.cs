using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BuildYourMarket;
using Onboarding.Tool.Model.BuildYourMarket.Configuration;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;
using Serilog;
using Onboarding.Tool.Model.Enums;
using System.Text.Json;
using InstanceModel = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using BuildBranch = Onboarding.Tool.Model.BuildYourMarket.Branch;
using SetupBranch = Onboarding.Tool.Model.BriefYourMarket.Branches.Branch;
using PortalAccount = Onboarding.Tool.Model.BuildYourMarket.PortalAccount;
using TemplateModel = Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels.Template;

namespace Onboarding.Tool.Server.Services.BuildYourMarket;

public class BuildYourMarketService : IBuildYourMarketService
{
    private readonly IHttpHelper _httpHelper;
    private readonly IOnboardingDataService _onboardingDataService;
    private readonly IInstanceApiService _instanceApiService;
    private readonly IInstanceLookupValueService _instanceLookupValueService;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly IConfigDatabaseService _configDatabaseService;
    private readonly BuildSettings _buildSettings;
    private readonly BuildYourMarketHelper _buildYourMarketHelper;

    public BuildYourMarketService(
        IHttpHelper httpHelper, 
        IOnboardingDataService onboardingDataService,
        IInstanceApiService instanceApiService,
        IInstanceLookupValueService instanceLookupValueService,
        IDashboardProgressService dashboardProgressService,
        IConfigDatabaseService configDatabaseService,
        IOptions<BuildSettings> buildSettings, 
        BuildYourMarketHelper buildYourMarketHelper)
    {
        _httpHelper = httpHelper;
        _onboardingDataService = onboardingDataService;
        _instanceApiService = instanceApiService;
        _instanceLookupValueService = instanceLookupValueService;
        _dashboardProgressService = dashboardProgressService;
        _configDatabaseService = configDatabaseService;
        _buildSettings = buildSettings.Value;
        _buildYourMarketHelper = buildYourMarketHelper;
    }

    public async Task<bool> AddMilestonesAsync(InstanceModel instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupBuildMilestones", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        string bearerToken = await _instanceApiService.GetBearerTokenAsync(instance);

        List<Task> tasks = new()
        {
            CreateMilestonesAsync(_buildYourMarketHelper.Config.Milestones.SaleWeeks, "weeks", "sales", bearerToken),
            CreateMilestonesAsync(_buildYourMarketHelper.Config.Milestones.LettingMonths, "months", "lettings", bearerToken),
            CreateMilestonesAsync(_buildYourMarketHelper.Config.Milestones.LettingDays, "days", "lettings", bearerToken)
        };

        await Task.WhenAll(tasks);
        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupBuildMilestones", TemplateConfirmationState.Confirmed);

        return true;
    }

    public async Task<bool> AddStandardContentAsync(InstanceModel instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupBuildContent", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        string bearerToken = await _instanceApiService.GetBearerTokenAsync(instance);

        List<LookupValue> lookupValues = await _instanceLookupValueService.GetInstanceLookupValuesFromApiAsync(instance.Subdomain, "api/branches");

        List<Task> tasks = new();

        foreach (Campaign campaign in _buildYourMarketHelper.Config.Campaigns)
        {
            Task task = AddStandardCampaignAsync(bearerToken, campaign);
            tasks.Add(task);
        }

        foreach (StandardContent content in _buildYourMarketHelper.Config.StandardContent)
        {
            foreach (LookupValue lookupValue in lookupValues)
            {
                Task task = AddStandardContentInternalAsync(bearerToken, lookupValue, content);
                tasks.Add(task);
            }
        }

        await Task.WhenAll(tasks);
        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupBuildContent", TemplateConfirmationState.Confirmed);

        return true;
    }

    public async Task<bool> SetupBranchesAsync(InstanceModel instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupBuildBranches", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        string bearerToken = await _instanceApiService.GetBearerTokenAsync(instance);

        List<LookupValue> instanceBranches = await _instanceLookupValueService.GetInstanceLookupValuesFromApiAsync(instance.Subdomain, "api/branches");
        IEnumerable<SetupBranch> branches = await _onboardingDataService.GetSetupBranchesAsync(instance);

        int? emailTemplateId = await GetEmailTemplateAsync(instance) ?? throw new InvalidOperationException("Unable to find email template for instance.");
        int? letterTemplateId = await GetLetterTemplateAsync(instance) ?? throw new InvalidOperationException("Unable to find letter template for instance.");
        int? printTemplateId = await _configDatabaseService.GetCardIdByNameAsync(instance, "BuildYourMarket");

        foreach (LookupValue lookupValue in instanceBranches)
        {
            SetupBranch? setupBranch = branches.Where(b => b.Name == lookupValue.Value).FirstOrDefault();
            
            if (setupBranch == null)
                continue;

            await AddBranchAsync(instance, bearerToken, emailTemplateId.Value, letterTemplateId.Value, printTemplateId.Value, lookupValue, setupBranch);
        }

        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupBuildBranches", TemplateConfirmationState.Confirmed);

        return true;
    }

    public async Task<bool> AddPortalAccountsAsync(InstanceModel instance)
    {
        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupBuildPortals", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        string bearerToken = await _instanceApiService.GetBearerTokenAsync(instance);
        IEnumerable<PortalAccount> portalAccounts = await _onboardingDataService.GetSetupInstancePortalAccountsAsync(instance);

        foreach (PortalAccount portalAccount in portalAccounts)
        {
            await _httpHelper.ExecuteRequestBearerAsync<object>(HttpMethod.Post, "BuildApi", _buildSettings.BaseUrl, "api/portalaccounts", bearerToken, portalAccount);
        }

        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupBuildPortals", TemplateConfirmationState.Confirmed);


        return true;
    }

    private async Task AddBranchAsync(InstanceModel instance, string token, int emailTemplateId, int letterTemplateId, int printTemplateId, LookupValue branch, SetupBranch setupBranch)
    {
        if (branch.Id == null)
            return;

        List<PostCodeDistrict> brancDistricts = await _onboardingDataService.GetBranchDistrictsByNameAsync(instance, setupBranch.Name);

        BuildBranch buildBranch = new()
        {
            BranchId = branch.Id.Value,
            Name = branch.Value,
            Brand = null,
            CreditGroupId = null,
            CustomDirty = true,
            EmailTemplateId = emailTemplateId,
            LetterTemplateId = letterTemplateId,
            PrintTemplateId = printTemplateId,
            OfficePhone = setupBranch.SalesPhoneNumber,
            PersonalLandlordSalutation = false,
            PortalAccount = null,
            Region = null,
            RentAskingPriceChange = "",
            RentAskingPriceMovementType = null,
            SignatureContactEmail = null,
            SignatureContactName = null,
            SignatureContactPosition = null,
            Updating = true,
            WholeOfUK = null,
            TenantTypes = null,
            PostCodeDistricts = brancDistricts
        };

        Log.Information("Attempting to create build branch: {Branch}", JsonSerializer.Serialize(buildBranch));
        object response = await _httpHelper.ExecuteRequestBearerAsync<object>(HttpMethod.Put, "BuildApi", _buildSettings.BaseUrl, $"api/branches/{branch.Id.Value}", token, buildBranch);
        Log.Information("Build Branch response {Response}", response);
    }

    private async Task AddStandardCampaignAsync(string token, Campaign campaign)
    {
        Log.Information("Adding Standard Build Campaign: {Campaign}", JsonSerializer.Serialize(campaign));
        await _httpHelper.ExecuteRequestBearerAsync<string>(HttpMethod.Post, "BuildApi", _buildSettings.BaseUrl, "api/campaigns", token, campaign);
    }

    private async Task AddStandardContentInternalAsync(string token, LookupValue branch, StandardContent content)
    {
        await _httpHelper.ExecuteRequestBearerAsync<object>(HttpMethod.Put, "BuildApi", _buildSettings.BaseUrl, $"api/marketingcopy/{content.Context}/{branch.Id}", token, content);
    }

    private async Task<int?> GetEmailTemplateAsync(InstanceModel instance)
    {
        List<TemplateModel> templates = await GetTemplatesAsync(instance, "api/templates");

        int? id = templates.FirstOrDefault(t => t.Name.Contains("Build Message Template"))?.Id;
        return id;
    }

    private async Task<int?> GetLetterTemplateAsync(InstanceModel instance)
    {
        List<TemplateModel> templates = await GetTemplatesAsync(instance, "api/letterheads");

        int? id = templates.FirstOrDefault()?.Id;
        return id;
    }

    private async Task<int?> GetPrintTemplateAsync(InstanceModel instance)
    {
        List<TemplateModel> templates = await GetTemplatesAsync(instance, "api/directmailproducts");

        int? id = templates.FirstOrDefault(t => t.Name.Contains("BuildYourMarket"))?.Id;
        return id;
    }

    private async Task<List<TemplateModel>> GetTemplatesAsync(InstanceModel instance, string endpoint)
    {
        List<TemplateModel> templates = await _instanceApiService.GetInstanceTemplatesAsync(instance.Subdomain, endpoint);
        return templates;
    }

    private async Task<List<Milestone>> CreateMilestonesAsync(List<int> periods, string type, string context, string token)
    {
        List<Milestone> currentMilestones = new();

        foreach (int period in periods)
        {
            Milestone milestone = new()
            {
                Id = period,
                Length = period,
                MilestoneType = type,
                MilestoneContext = context
            };

            await _httpHelper.ExecuteRequestBearerAsync<object>(HttpMethod.Post, "BuildApi", _buildSettings.BaseUrl, "api/milestones", token, milestone);
            currentMilestones.Add(milestone);
        }

        return currentMilestones;
    }
}
