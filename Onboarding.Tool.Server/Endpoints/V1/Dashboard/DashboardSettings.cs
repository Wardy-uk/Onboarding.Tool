using System.Text.Json;
using Onboarding.Tool.Model.Dashboard.Settings;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Server.Extensions;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Endpoints.V1.Dashboard
{
    public static class DashboardSettings
    {
        public static void MapBriefYourMarketDashboardSettingsEndpoints(this WebApplication app)
        {
            RouteGroupBuilder group = app.MapGroup("/api/v1/dashboard/instance/{domain}/settings")
                .AddEndpointFilter<InstanceValidationFilter>()
                .RequireAuthorization("AllowedEmailOnly");

            group.MapPost("/", ApplySettings);
            group.MapPost("/branch/{branchId}", ApplyBranchSettings);
            group.MapPost("/import", ImportSettings);
            group.MapGet("/", GetSettingsAsync);
            group.MapGet("/branch/{branchId}", GetBranchSettingsAsync);
            group.MapGet("/branch", GetBranchesAsync);
        }


        public static async Task<IResult> ApplySettings(
            IDashboardSettingsService dashboardSettingsService,
            HttpContext httpContext,
            Dictionary<string, string> settings)
        {
            Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

            if (settings?.Count == 0 || settings == null)
                return Results.BadRequest("No settings provided");

            try
            {
                List<InstanceSetting>? appliedSettings = await dashboardSettingsService.ApplyInstanceSettingsAsync(instance, settings);
                if (appliedSettings == null)
                    return Results.BadRequest("Failed to apply settings.");

                return Results.Ok(appliedSettings);
            }
            catch
            {
                return Results.Problem("Failed to apply settings.");
            }
        }

        private static async Task<IResult> ApplyBranchSettings(
            IDashboardSettingsService dashboardSettingsService,
            HttpContext httpContext,
            Dictionary<string, string> settings,
            int branchId)
        {
            Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

            if (settings?.Count == 0 || settings == null)
                return Results.BadRequest("No settings provided");

            try
            {
                List<BranchSetting>? appliedSettings = await dashboardSettingsService.ApplyBranchSettingsAsync(instance, branchId, settings);
                if (appliedSettings == null)
                    return Results.BadRequest("Failed to apply settings.");

                return Results.Ok(appliedSettings);
            }
            catch
            {
                return Results.Problem("Failed to apply settings.");
            }
        }

        public static async Task<IResult> ImportSettings(
            IDashboardSettingsService dashboardSettingsService,
            HttpContext httpContext,
            HttpRequest request)
        {
            Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

            try
            {
                using StreamReader reader = new(request.Body);
                string json = await reader.ReadToEndAsync();
                List<Dictionary<string, object>>? importedSettings = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);

                if (importedSettings == null)
                    return Results.BadRequest("Imported settings are badly formatted.");

                Dictionary<string, object>? instanceSettings =
                               importedSettings.FirstOrDefault(c => c.ContainsKey("context") && c["context"]?.ToString()?.Contains("Default", StringComparison.OrdinalIgnoreCase) == true);

                if (instanceSettings == null)
                    return Results.BadRequest("Your imported settings must contain a default context.");

                bool success = await dashboardSettingsService.ImportSettingsAsync(instance, importedSettings);
                if (!success)
                    return Results.BadRequest("Failed to import settings.");

                return Results.Ok(success);
            }
            catch
            {
                return Results.Problem("Failed to import settings.");
            }
        }

        private static async Task<IResult> GetSettingsAsync(
            IDashboardSettingsService dashboardSettingsService,
            HttpContext httpContext)
        {
            Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

            try
            {
                List<InstanceSetting> settings = await dashboardSettingsService.GetSettingsAsync(instance);
                return Results.Ok(settings);
            }
            catch
            {
                return Results.Problem("Failed to fetch settings.");
            }
        }

        private static async Task<IResult> GetBranchSettingsAsync(
            IDashboardSettingsService dashboardSettingsService,
            HttpContext httpContext,
            int branchId)
        {
            Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

            try
            {
                List<BranchSetting> branchSettings = await dashboardSettingsService.GetBranchSettingsAsync(instance, branchId);
                return Results.Ok(branchSettings);
            }
            catch
            {
                return Results.Problem("Failed to fetch settings.");
            }
        }

        private static async Task<IResult> GetBranchesAsync(
            IDashboardSettingsService dashboardSettingsService,
            HttpContext httpContext)
        {
            Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

            try
            {
                List<SettingBranchDto> branches = await dashboardSettingsService.GetBranchesAsync(instance);
                return Results.Ok(branches);
            }
            catch
            {
                return Results.Problem("Failed to fetch settings.");
            }
        }
    }
}