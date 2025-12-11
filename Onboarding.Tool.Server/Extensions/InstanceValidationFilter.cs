namespace Onboarding.Tool.Server.Extensions;

public class InstanceValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        string? domain = context.HttpContext.GetRouteValue("domain")?.ToString();
        if (string.IsNullOrEmpty(domain))
            return Results.BadRequest();

        IConfigDatabaseService configDatabaseService = context.HttpContext.RequestServices.GetRequiredService<IConfigDatabaseService>();

        var (IsValid, Instance, ErrorResult) = await InstanceValidationHelper.ValidateInstanceAsync(configDatabaseService, domain);

        if (!IsValid)
            return ErrorResult;

        context.HttpContext.Items["ValidatedInstance"] = Instance;

        return await next(context);
    }
}
