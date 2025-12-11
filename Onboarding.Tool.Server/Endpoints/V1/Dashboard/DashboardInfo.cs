using Onboarding.Tool.Model.Robocop;
using Onboarding.Tool.Server.Extensions;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using DatabaseInstance = Onboarding.Tool.Model.Data.Instance;
using Serilog;
using Onboarding.Tool.Model.Dashboard;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Model.Dashboard.Settings;
using Onboarding.Tool.Model.Dashboard.Setups;

namespace Onboarding.Tool.Server.Endpoints.V1.Dashboard;

public static class DashboardInfo
{
    public static void MapBriefYourMarketDashboardInfoEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/dashboard/instances", GetInstanceList)
            .RequireAuthorization("AllowedEmailOnly");

        RouteGroupBuilder group = app.MapGroup("/api/v1/dashboard/instance/{domain}/info")
            .AddEndpointFilter<InstanceValidationFilter>()
            .RequireAuthorization("AllowedEmailOnly");

        group.MapGet("/overview", GetOverviewAsync);
        group.MapGet("/branches", GetBranchesAsync);
        group.MapGet("/build", GetBuildAsync);
        group.MapGet("/users", GetUsersAsync);
        group.MapGet("/required-settings", GetRequiredSettings);
        group.MapGet("/setup-state", GetSetupStateAsync);
        group.MapGet("/preview-card-info", GetCardPreviewInfoAsync);
        
        app.MapGet("/api/v1/dashboard/instance/{domain}/info/render-preview", RenderPreviewAsync)
            .AddEndpointFilter<InstanceValidationFilter>();
    }

    private static async Task<IResult> GetInstanceList(IConfigDatabaseService configDatabaseService)
    {
        try
        {
            List<EligibleInstance> instances = await configDatabaseService.GetInstancesEligibleForSetupAsync();
            return Results.Ok(instances);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to obtain instances"); 
            return Results.Problem(detail: ex.ToString(), title: "Exception occurred", statusCode: 500);
        }
    }

    private static async Task<IResult> GetOverviewAsync(
        IOnboardingDataService onboardingDataService,
        IInstanceApiService instanceApiService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            DatabaseInstance? instanceCreated = await onboardingDataService.GetOrCreateInstanceAsync(instance.Id);
            if (instanceCreated != null)
                await instanceApiService.GetBearerTokenAsync(instance);

            OverviewDto overview = await onboardingDataService.GetInstanceOverviewAsync(instance);
            return Results.Ok(overview);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to retrieve instance information");
        }
    }

    private static async Task<IResult> GetBranchesAsync(
        IOnboardingDataService onboardingDataService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            List<BranchDto> branches = await onboardingDataService.GetBranchOverviewAsync(instance);
            return Results.Ok(branches);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to retrieve instance branches");
        }
    }

    private static async Task<IResult> GetBuildAsync(
        IOnboardingDataService onboardingDataService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            BuildOverview buildOverview = await onboardingDataService.GetBuildOverviewAsync(instance);
            return Results.Ok(buildOverview);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to retrieve instance build");
        }
    }

    private static async Task<IResult> GetUsersAsync(
            IOnboardingDataService onboardingDataService,
            HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            List<Users> users = await onboardingDataService.GetUsersAsync(instance);
            return Results.Ok(users);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to retrieve instance users");
        }
    }

    private static IResult GetRequiredSettings(
        IOnboardingDataService onboardingDataService,
        HttpContext httpContext)
    {
        try
        {
            List<BrandSettingConfig> settings = onboardingDataService.GetBrandSettingConfig();
            return Results.Ok(settings);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to retrieve required settings");
        }
    }

    private static async Task<IResult> GetSetupStateAsync(
        IOnboardingDataService onboardingDataService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            SetupStateDto state = await onboardingDataService.GetSetupStateAsync(instance);
            return Results.Ok(state);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to fetch state");
        }
    }

    private static async Task<IResult> GetCardPreviewInfoAsync(
        IOnboardingDataService onboardingDataService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            CardPreviewInfo info = await onboardingDataService.GetCardPreviewInfoAsync(instance);
            return Results.Ok(info);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to render");
        }
    }

    private static async Task<IResult> RenderPreviewAsync(
        IOnboardingDataService onboardingDataService,
        HttpContext httpContext,
        int? width = null,
        int? height = null,
        int? x = null,
        int? y = null)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            byte[] bytes = await onboardingDataService.GetPreviewPdfAsync(instance, width, height, x, y);
            return Results.File(bytes, "application/pdf");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to render PDF preview");
            return Results.Problem("Failed to render");
        }
    }
}
