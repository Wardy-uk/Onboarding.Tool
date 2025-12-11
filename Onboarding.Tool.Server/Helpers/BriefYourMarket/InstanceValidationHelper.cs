using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Helpers.BriefYourMarket;

public static class InstanceValidationHelper
{
    public static async Task<(bool IsValid, Instance? Instance, IResult ErrorResult)> ValidateInstanceAsync(IConfigDatabaseService configDatabaseService, string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return (false, null, Results.BadRequest("Domain is required"));

        Instance? instance = await configDatabaseService.GetInstanceDefinitionAsync(domain);
        if (instance == null)
            return (false, null, Results.NotFound("Instance not found"));

        bool isValid = await configDatabaseService.IsInstanceEligibleForSetupAsync(instance);
        if (!isValid)
            return (false, null, Results.BadRequest("Instance not eligible for setup"));

        return (true, instance, Results.Ok());
    }
}
