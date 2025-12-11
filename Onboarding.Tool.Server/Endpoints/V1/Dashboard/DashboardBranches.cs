using Onboarding.Tool.Model.Dashboard.Branches;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Server.Extensions;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Endpoints.V1.Dashboard;

public static class DashboardBranches
{
    public static void MapBriefYourMarketDashboardBranchEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/dashboard/instance/{domain}/branch")
            .AddEndpointFilter<InstanceValidationFilter>()
            .RequireAuthorization("AllowedEmailOnly");

        group.MapPost("/create", CreateBranch);
        group.MapPost("/save", SaveBranch);
        group.MapPost("/delete/{branchId}", DeleteBranch);
        group.MapPost("/import", ImportBranches);
    }

    public static async Task<IResult> CreateBranch(
                IDashboardBranchService dashboardBranchService,
                HttpContext httpContext,
                ImportBranch import)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            Branch? branch = await dashboardBranchService.CreateBranchAsync(instance.Id, import);

            return branch == null ? throw new InvalidOperationException("Unable to create new branch.") : Results.Ok(branch);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to create branch");
        }
    }

    public static async Task<IResult> SaveBranch(
        IDashboardBranchService dashboardBranchService,
        HttpContext httpContext,
        SaveBranch saveBranch)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            Branch? branch = await dashboardBranchService.UpdateBranchAsync(saveBranch, instance);

            return branch == null ? throw new InvalidOperationException("Unable to find or update given branch.") : Results.Ok(branch);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to save branch");
        }
    }

    public static async Task<IResult> DeleteBranch(
        IDashboardBranchService dashboardBranchService,
        HttpContext httpContext,
        int branchId)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await dashboardBranchService.DeleteBranchAsync(branchId, instance.Id);
            return Results.Ok(success);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to delete branch");
        }
    }

    public static async Task<IResult> ImportBranches(
        IDashboardBranchService dashboardBranchService,
        HttpContext httpContext,
        List<ImportBranch> importBranches)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        if (importBranches.Count == 0 || importBranches == null)
            return Results.BadRequest("No branches to import");

        try
        {
            List<Branch> branches = await dashboardBranchService.ImportBranchesAsync(instance.Id, importBranches);
            return Results.Ok(branches);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to import branches");
        }
    }
}
