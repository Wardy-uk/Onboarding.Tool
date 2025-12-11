using Onboarding.Tool.Model.Dashboard;
using Onboarding.Tool.Model.Dashboard.BuildConfigs;
using Onboarding.Tool.Model.Dashboard.Users;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Server.Extensions;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Endpoints.V1.Dashboard;

public static class DashboardBuild
{
    public static void MapBriefYourMarketDashboardBuildEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/dashboard/instance/{domain}/build")
            .AddEndpointFilter<InstanceValidationFilter>()
            .RequireAuthorization("AllowedEmailOnly");

        group.MapPost("/portal/create/{portalName}", CreatePortal);
        group.MapPost("/portal/import", ImportPortalAccountsAsync);
        group.MapPost("/portal/delete/{portalId}", DeletePortal);
        group.MapPost("/districts/create", CreateDistrict);
        group.MapPost("/districts/save/{districtId}", SaveDistrict);
        group.MapPost("/districts/delete/{districtId}", DeleteDistrict);
    }

    private static async Task<IResult> CreatePortal(
                IDashboardBuildService dashboardBuildService,
                HttpContext httpContext,
                string portalName)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            PortalAccount? portalAccount = await dashboardBuildService.AddPortalAccountAsync(instance, portalName);

            if (portalAccount == null)
                return Results.BadRequest("Issue adding portal account, it may already exist.");

            return Results.Ok(portalAccount);
        }
        catch
        {
            return Results.Problem("There was a problem adding the specified portal account.");
        }
    }

    private static async Task<IResult> ImportPortalAccountsAsync(
        IDashboardBuildService dashboardBuildService,
        HttpContext httpContext,
        List<BuildPortalAccountImport> importPortalAccounts)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        if (importPortalAccounts?.Count == 0 || importPortalAccounts == null)
            return Results.BadRequest("No users to import");

        try
        {
            List<PortalAccount> portalAccounts = await dashboardBuildService.ImportPortalAccountsAsync(instance, importPortalAccounts);
            return Results.Ok(portalAccounts);
        }
        catch
        {
            return Results.Problem("There was a problem adding the specified portal account.");
        }
    }

    private static async Task<IResult> DeletePortal(
        IDashboardBuildService dashboardBuildService,
        HttpContext httpContext,
        int portalId)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await dashboardBuildService.DeletePortalAccountAsync(instance, portalId);

            if (!success)
                return Results.BadRequest("Failed to delete portal account.");

            return Results.Ok(success);
        }
        catch
        {
            return Results.Problem("There was a problem removing the specified portal account.");
        }
    }

    private static async Task<IResult> CreateDistrict(
        IDashboardBuildService dashboardBuildService,
        HttpContext httpContext,
        DistrictConfig districtConfig)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        if (districtConfig.AllSectors == false && districtConfig.Sectors.Count == 0)
            return Results.BadRequest("You must either select all districts, or provide districts to populate.");

        try
        {
            BranchBuildDistrict? branchBuildDistrict = await dashboardBuildService.CreateBuildDistrictAsync(instance, districtConfig);
            if (branchBuildDistrict == null)
                return Results.BadRequest("Failed to create branch build district.");

            return Results.Ok(new BuildDistrictDto
            {
                DistrictId = branchBuildDistrict.Id,
                District = branchBuildDistrict.District,
                AllSectors = branchBuildDistrict.AllSectors,
                Sectors = branchBuildDistrict.Sectors?.Select(s => s.Sector).ToList() ?? []
            });
        }
        catch
        {
            return Results.Problem("Failed to create branch build district.");
        }
    }

    private static async Task<IResult> SaveDistrict(
        IDashboardBuildService dashboardBuildService,
        HttpContext httpContext,
        int districtId,
        DistrictConfig districtConfig)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            BranchBuildDistrict? updatedDistrict = await dashboardBuildService.UpdateBuildDistrictAsync(instance, districtId, districtConfig);
            if (updatedDistrict == null)
                return Results.BadRequest("Failed to update branch build district.");

            return Results.Ok(new BuildDistrictDto
            {
                DistrictId = updatedDistrict.Id,
                District = updatedDistrict.District,
                AllSectors = updatedDistrict.AllSectors,
                Sectors = updatedDistrict.Sectors?.Select(s => s.Sector).ToList() ?? []
            });
        }
        catch
        {
            return Results.Problem("Failed to update branch build district.");
        }
    }

    private static async Task<IResult> DeleteDistrict(
        IDashboardBuildService dashboardBuildService,
        HttpContext httpContext,
        int districtId)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await dashboardBuildService.DeleteBuildDistrictAsync(instance, districtId);
            if (!success)
                return Results.BadRequest("Failed to delete branch build district.");

            return Results.Ok(success);
        }
        catch
        {
            return Results.Problem("Failed to delete branch build district.");
        }
    }
}
