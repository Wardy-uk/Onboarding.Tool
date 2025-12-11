using Onboarding.Tool.Server.Helpers.Dashboard;
using Onboarding.Tool.Server.Services.Dashboard.Background;

namespace Onboarding.Tool.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<GitSettings>(config.GetSection("Git"));
        services.Configure<InstanceSettings>(config.GetSection("Instance"));
        services.Configure<InstanceTemplateSettings>(config.GetSection("InstanceTemplate"));
        services.Configure<ConfigSettings>(config.GetSection("Config"));
        services.Configure<BuildSettings>(config.GetSection("BuildYourMarket"));
        services.Configure<OpenIdSettings>(config.GetSection("OpenId"));
        services.Configure<ApplicationGatewaySettings>(config.GetSection("ApplicationGateway"));

        services.AddScoped<ITfsService, TfsService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IConfigDatabaseService, ConfigDatabaseService>();
        services.AddScoped<IInstanceDatabaseService, InstanceDatabaseService>();
        services.AddScoped<IConfigDirectMailService, ConfigDirectMailService>();
        services.AddScoped<IConfigEmailComponentsService, ConfigEmailComponentsService>();
        services.AddScoped<IConfigReportsService, ConfigReportsService>();
        services.AddScoped<IConfigSettingsService, ConfigSettingsService>();
        services.AddScoped<IInstanceApiService, InstanceApiService>();
        services.AddScoped<IInstanceRssApiService, InstanceRssApiService>();
        services.AddScoped<IInstanceUsersApiService, InstanceUsersApiService>();
        services.AddScoped<IInstanceLookupValueService, InstanceLookupValueService>();
        services.AddScoped<IInstanceAutomationService, InstanceAutomationService>();
        services.AddScoped<IInstanceLetterService, InstanceLetterService>();
        services.AddScoped<IInstanceSqlConnectionFactory, InstanceSqlConnectionFactory>();
        services.AddScoped<IOnboardingSqlConnection, OnboardingSqlConnection>();
        services.AddScoped<IHttpHelper, HttpHelper>();
        services.AddScoped<IBuildYourMarketService, BuildYourMarketService>();
        services.AddScoped<IOnboardingDataService, OnboardingDataService>();
        services.AddScoped<IDashboardBranchService, DashboardBranchService>();
        services.AddScoped<IDashboardUserService, DashboardUserService>();
        services.AddScoped<IDashboardImageService, DashboardImageService>();
        services.AddScoped<IDashboardBuildService, DashboardBuildService>();
        services.AddScoped<IDashboardSettingsService, DashboardSettingsService>();
        services.AddScoped<IDashboardProgressService, DashboardProgressService>();
        services.AddScoped<IMatchToCrmService, MatchToCrmService>();

        services.AddSingleton<DashboardBackgroundService>();
        services.AddHostedService<DashboardBackgroundService>(provider =>
            provider.GetRequiredService<DashboardBackgroundService>());
        services.AddSingleton<IDashboardBackgroundService>(provider =>
            provider.GetRequiredService<DashboardBackgroundService>());

        services.AddSingleton<BriefYourMarketInstanceHelper>();
        services.AddSingleton<BuildYourMarketHelper>();
        services.AddSingleton<DashboardSettingsHelper>();
        return services;
    }
}
