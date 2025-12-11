using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1;

namespace Onboarding.Tool.Server.Endpoints;

public static class Authentication
{
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        app.MapGet("/login", Login);
        app.MapGet("/logout", Logout);
        app.MapGet("/auth/me", CheckAuth);
        app.MapGet("/auth/error", (HttpContext http) =>
        {
            var message = http.Request.Query["message"];
            return Results.Text($"Authentication Error: {message}", "text/plain");
        });
    }

    private static async Task Login(
        HttpContext context,
        IOptions<ApplicationGatewaySettings> options)
    {
        try
        {
            await context.ChallengeAsync("oidc", new AuthenticationProperties
            {
                RedirectUri = options.Value.FrontEndUrl
            });
            return;
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync($"Authentication challenge failed: {ex.Message}");
        }
    }

    private static IResult CheckAuth(
        HttpContext context)
    {
        var user = context.User;

        if (user?.Identity is not { IsAuthenticated: true })
            return Results.Unauthorized();

        var claims = user.Claims.Select(c => new { type = c.Type, value = c.Value });

        string email = claims.Where(c => c.type == "email").FirstOrDefault()?.value ?? "";
        if (!IsAllowedEmail(email))
            return Results.Unauthorized();

        return Results.Json(new
        {
            authenticated = true,
            name = claims.Where(c => c.type == "name").Select(c => c.value).FirstOrDefault() ?? "",
            claims
        });
    }

    private static async Task Logout(
        HttpContext context)
    {
        await context.SignOutAsync("Cookies");

        await context.SignOutAsync("oidc", new AuthenticationProperties
        {
            RedirectUri = "/"
        });
    }

    private static bool IsAllowedEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string[] allowedDomains = new[]
        {
            "@nurtur.tech",
            "@briefyourmarket.co.uk"
        };

        return allowedDomains.Any(domain => email.EndsWith(domain, StringComparison.OrdinalIgnoreCase));
    }
}
