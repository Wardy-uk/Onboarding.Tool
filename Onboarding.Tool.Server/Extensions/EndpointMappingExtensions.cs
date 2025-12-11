using Onboarding.Tool.Server.Endpoints;
using Onboarding.Tool.Server.Endpoints.V1.BriefYourMarket;
using Onboarding.Tool.Server.Endpoints.V1.BuildYourMarket;
using Onboarding.Tool.Server.Endpoints.V1.Dashboard;

namespace Onboarding.Tool.Server.Extensions;

public static class EndpointMappingExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapAuthenticationEndpoints();
        app.MapCreateBriefYourMarketProjectEndpoints();
        app.MapSetupBriefYourMarketInstance();
        app.MapBriefYourMarketDashboardSettingsEndpoints();
        app.MapBriefYourMarketDashboardInfoEndpoints();
        app.MapBriefYourMarketDashboardBranchEndpoints();
        app.MapBriefYourMarketDashboardBuildEndpoints();
        app.MapBriefYourMarketDashboardUserEndpoints();
        app.MapBriefYourMarketDashboardImageEndpoints();
        app.MapSetupBuildYourMarket();

        return app;
    }
}
