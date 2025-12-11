using Onboarding.Tool.Model.Dashboard.Users;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Server.Extensions;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Endpoints.V1.Dashboard;

public static class DashboardUser
{
    public static void MapBriefYourMarketDashboardUserEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/dashboard/instance/{domain}/user")
            .AddEndpointFilter<InstanceValidationFilter>()
            .RequireAuthorization("AllowedEmailOnly");

        group.MapPost("/create", CreateUser);
        group.MapPost("/delete/{userId}", DeleteUser);
        group.MapPost("/edit/{userId}", EditUser);
        group.MapPost("/import", ImportUsers);
    }

    public static async Task<IResult> CreateUser(
                IDashboardUserService dashboardUserService,
                HttpContext httpContext,
                ImportUser importUser)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        if (string.IsNullOrWhiteSpace(importUser?.Email))
            return Results.BadRequest("Email is required");

        try
        {
            Users? user = await dashboardUserService.CreateUserAsync(importUser, instance);

            return user == null ? throw new InvalidOperationException("There was a problem creating the user.") : Results.Ok(user);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to create user");
        }
    }

    public static async Task<IResult> DeleteUser(
        IDashboardUserService dashboardUserService,
        HttpContext httpContext,
        int userId)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await dashboardUserService.DeleteUser(userId, instance);

            return Results.Ok(success);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to delete user");
        }
    }

    public static async Task<IResult> EditUser(
        IDashboardUserService dashboardUserService,
        HttpContext httpContext,
        int userId,
        ImportUser newUserData)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        if (string.IsNullOrWhiteSpace(newUserData?.Email))
            return Results.BadRequest("Email is required");

        try
        {
            Users? editedUser = await dashboardUserService.UpdateUser(userId, newUserData, instance);

            return editedUser == null ? throw new InvalidOperationException("Unable to modify user with the provided data.") : Results.Ok(editedUser);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to edit user");
        }
    }

    public static async Task<IResult> ImportUsers(
        IDashboardUserService dashboardUserService,
        HttpContext httpContext,
        List<ImportUser> importUsers)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        if (importUsers?.Count == 0 || importUsers == null)
            return Results.BadRequest("No users to import");

        try
        {
            List<Users> newUsers = await dashboardUserService.ImportUsersAsync(importUsers, instance);
            return Results.Ok(newUsers);
        }
        catch (Exception)
        {
            return Results.Problem("Failed to import users");
        }
    }
}
