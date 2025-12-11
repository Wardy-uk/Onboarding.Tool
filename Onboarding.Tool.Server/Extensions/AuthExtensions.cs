using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Serilog;
using System.Security.Claims;

namespace Onboarding.Tool.Server.Extensions;

static class AuthExtensions
{
    public static void AddAuthentication(this WebApplicationBuilder builder, OpenIdSettings openIdSettings)
    {
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = "oidc";
            options.DefaultSignOutScheme = "oidc";
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = "NurturOnboarding";
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        })
        .AddOpenIdConnect("oidc", options =>
        {
            options.Authority = openIdSettings.Authority;
            options.ClientId = openIdSettings.ClientId;
            options.ClientSecret = openIdSettings.ClientSecret;
            options.ResponseType = "code";
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");

            if (openIdSettings.ApiScope != null)
            {
                foreach (string scope in openIdSettings.ApiScope)
                {
                    options.Scope.Add(scope);
                }
            }

            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.MapInboundClaims = false;

            options.CallbackPath = "/signin-oidc";
            options.Events = new OpenIdConnectEvents
            {
                OnRemoteFailure = context =>
                {
                    var errorMessage = $"OIDC Error: {context.Failure?.Message}";
                    Console.WriteLine(errorMessage);

                    Log.Error(errorMessage);

                    context.Response.Redirect($"/auth/error?message={Uri.EscapeDataString(errorMessage)}");
                    context.HandleResponse();

                    return Task.CompletedTask;
                }
            };
        });
    }

    public static IServiceCollection AddAllowedEmailPolicy(this IServiceCollection services)
    {
        string[] allowedDomains = new[]
        {
            "@nurtur.tech",
            "@briefyourmarket.co.uk"
        };

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AllowedEmailOnly", policy =>
            {
                policy.RequireAssertion(context =>
                {
                    var email = context.User?.FindFirst(ClaimTypes.Email)?.Value
                             ?? context.User?.FindFirst("email")?.Value
                             ?? context.User?.Identity?.Name;

                    if (string.IsNullOrWhiteSpace(email))
                        return false;

                    return allowedDomains.Any(domain =>
                        email.EndsWith(domain, StringComparison.OrdinalIgnoreCase));
                });
            });
        });

        return services;
    }
}

