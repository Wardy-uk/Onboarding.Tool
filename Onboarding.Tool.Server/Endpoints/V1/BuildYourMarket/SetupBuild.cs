using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.Dashboard.Setups;
using Onboarding.Tool.Server.Extensions;
using Onboarding.Tool.Server.Services.BuildYourMarket;
using Serilog;

namespace Onboarding.Tool.Server.Endpoints.V1.BuildYourMarket;

public static class SetupBuildYourMarket
{
    public static void MapSetupBuildYourMarket(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/buildyourmarket")
            .AddEndpointFilter<InstanceValidationFilter>()
            .RequireAuthorization("AllowedEmailOnly");

        group.MapPost("/{domain}/milestones", SetupMilestones);
        group.MapPost("/{domain}/portals", SetupPortals);
        group.MapPost("/{domain}/branches", SetupBranches);
        group.MapPost("/{domain}/content", SetupContent);
        group.MapPost("/{domain}/matchtocrm", SetupMatchToCrm);
    }

    public static async Task<IResult> SetupMilestones(
        IBuildYourMarketService buildYourMarketService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await buildYourMarketService.AddMilestonesAsync(instance);

            if (!success)
                return Results.Problem("Failed to add milestones.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Build Milestones added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to setup milestones");
            return Results.InternalServerError("Failed to setup Milestones.");
        }
    }

    public static async Task<IResult> SetupPortals(
        IBuildYourMarketService buildYourMarketService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await buildYourMarketService.AddPortalAccountsAsync(instance);

            if (!success)
                return Results.Problem("Failed to add portal accounts.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Build Portals added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to setup portals");
            return Results.InternalServerError("Failed to setup Portal Accounts.");
        }
    }

    public static async Task<IResult> SetupBranches(
        IBuildYourMarketService buildYourMarketService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await buildYourMarketService.SetupBranchesAsync(instance);

            if (!success)
                return Results.Problem("Failed to add branches.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Build Branches added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to setup branches");
            return Results.InternalServerError("Failed to setup Branches.");
        }
    }

    public static async Task<IResult> SetupContent(
        IBuildYourMarketService buildYourMarketService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await buildYourMarketService.AddStandardContentAsync(instance);

            if (!success)
                return Results.Problem("Failed to add content.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Build Content added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to setup content");
            return Results.InternalServerError("Failed to add Content.");
        }
    }

    public static async Task<IResult> SetupMatchToCrm(
        IMatchToCrmService matchToCrmService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await matchToCrmService.AddMatchToCrmToInstanceAsync(instance);

            if (!success)
                return Results.Problem("Failed to add match to crm.");

            SetupResult result = new()
            {
                Success = true,
                Message = $"Build Match to CRM added."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to setup match to crm");
            return Results.InternalServerError("Failed to add match to crm.");
        }
    }
}
