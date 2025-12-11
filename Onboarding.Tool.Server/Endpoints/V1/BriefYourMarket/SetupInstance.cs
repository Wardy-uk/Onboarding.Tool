using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;
using Onboarding.Tool.Model.Dashboard.Setups;
using Onboarding.Tool.Model.Robocop.EmailComponents;
using Onboarding.Tool.Server.Extensions;
using Serilog;

namespace Onboarding.Tool.Server.Endpoints.V1.BriefYourMarket;

public static class SetupBriefYourMarketInstance
{
    public static void MapSetupBriefYourMarketInstance(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/briefyourmarket/setup")
            .AddEndpointFilter<InstanceValidationFilter>()
            .RequireAuthorization("AllowedEmailOnly");

        group.MapPost("/{domain}/print-libraries", DefaultPrintLibrariesAsync);
        group.MapPost("/{domain}/rss", CreateRssFeeds);
        group.MapPost("/{domain}/users", CreateUsers);
        group.MapPost("/{domain}/reporting", CreateScheduledReports);
        group.MapPost("/{domain}/configuration", ConfigureInstanceSettings);
        group.MapPost("/{domain}/brands", ConfigureBrands);
        group.MapPost("/{domain}/branches", ConfigureBranches);
        group.MapPost("/{domain}/components", ConfigureComponents);
        group.MapPost("/{domain}/delivery", ConfigureDeliveryAddresses);
        group.MapPost("/{domain}/automation", ConfigureAutomation);
        group.MapPost("/{domain}/2020s", ConfigureAuto2020);
    }

    public static async Task<IResult> DefaultPrintLibrariesAsync(
        IConfigDirectMailService configDirectMailService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await configDirectMailService.AddDefaultPrintLibrariesToInstanceAsync(instance);

            if (!success)
                Results.BadRequest("Failed to add default print libraries.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Print Libraries added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add default print libraries");
            return Results.InternalServerError("Failed to add default print libraries");
        }
    }

    public static async Task<IResult> CreateRssFeeds(
        IInstanceRssApiService instanceRssApiService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            List<int> rssFeeds = await instanceRssApiService.AddRssFeedsToInstanceAsync(instance);
            if (rssFeeds.Count == 0)
                return Results.BadRequest("Failed to add RSS feeds.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"RSS Feeds added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create RSS");
            return Results.InternalServerError("Failed to insert RSS feeds.");
        }
    }

    public static async Task<IResult> CreateUsers(
        IInstanceUsersApiService instanceUsersApiService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            List<int> userIds = await instanceUsersApiService.AddUsersToInstanceAsync(instance);
            if (userIds.Count == 0)
                return Results.BadRequest("Unable to create any users.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Users added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create users");
            return Results.InternalServerError("Failed to create users.");
        }
    }

    public static async Task<IResult> CreateScheduledReports(
        IConfigReportsService configReportsService,
        IInstanceDatabaseService instanceDatabaseService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool scheduledReportIds = await configReportsService.AddScheduledReportsToInstanceAsync(instance);
            if (!scheduledReportIds)
                return Results.Problem("Failed to add scheduled reports.");

            await instanceDatabaseService.AddScheduledReportConfigurationsAsync(instance);

            SetupResult result = new()
            {
                Success = true,
                Message = $"Scheduled Reports added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create scheduled reports");
            return Results.InternalServerError("Failed to create Scheduled Reports.");
        }
    }

    public static async Task<IResult> ConfigureInstanceSettings(
        IConfigSettingsService configSettingsService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool instanceSettings = await configSettingsService.AddDefaultSettingsToInstanceAsync(instance);
            if (!instanceSettings)
                return Results.BadRequest("Unable to add instance settings.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Instance settings added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add instance settings");
            return Results.InternalServerError("Failed to add Robocop Settings.");
        }
    }

    public static async Task<IResult> ConfigureBrands(
        IInstanceLookupValueService instanceLookupValueService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool brandSuccess = await instanceLookupValueService.AddBrandsToInstanceAsync(instance);

            if (!brandSuccess)
                return Results.Problem("Failed to add brands.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Brands added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create brands");
            return Results.InternalServerError("Failed to add Brands.");
        }
    }

    public static async Task<IResult> ConfigureBranches(
        IInstanceLookupValueService instanceLookupValueService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool branchSuccess = await instanceLookupValueService.AddBranchesToInstanceAsync(instance);

            if (!branchSuccess)
                return Results.Problem("Failed to add branches.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Branches added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create branches");
            return Results.InternalServerError("Failed to add Branches");
        }
    }

    public static async Task<IResult> ConfigureComponents(
        IConfigEmailComponentsService configEmailComponentsService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            List<EmailComponentLibrary> emailComponentLibraries = await configEmailComponentsService.AddDefaultComponentLibrariesToInstanceAsync(instance);

            SetupResult result = new()
            {
                Success = true,
                Message = $"Email Libraries added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to add components");
            return Results.InternalServerError("Failed to add component libraries.");
        }
    }

    public static async Task<IResult> ConfigureDeliveryAddresses(
        IInstanceLookupValueService instanceLookupValueService,
        IInstanceDatabaseService instanceDatabaseService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            List<DeliveryAddress> deliveryAddresses = await instanceLookupValueService.GetDeliveryAddressesFromInstanceAsync(instance);
            bool success = await instanceDatabaseService.CreateDeliveryAddressesAsync(instance, deliveryAddresses);

            if (!success)
                return Results.Problem("Failed to add delivery addresses.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Delivery Addresses added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create delivery addresses");
            return Results.InternalServerError("Failed to add Delivery Addresses.");
        }
    }

    public static async Task<IResult> ConfigureAutomation(
        IInstanceAutomationService instanceAutomationService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();
        
        try
        {
            List<int> triggers = await instanceAutomationService.AddDefaultEmailTriggersToInstanceAsync(instance);
            if (triggers.Count == 0)
                return Results.Problem("Failed to add automations.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Email Automation added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create automations");
            return Results.InternalServerError("Failed to add Automations.");
        }
    }

    public static async Task<IResult> ConfigureAuto2020(
        IInstanceAutomationService instanceAutomationService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            List<int> triggers = await instanceAutomationService.AddDefault2020TriggersToInstanceAsync(instance);
            if (triggers.Count == 0)
                return Results.Problem("Failed to add 2020s.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Auto2020 Automations added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create 2020s");
            return Results.InternalServerError("Failed to add 2020s.");
        }
    }
}
