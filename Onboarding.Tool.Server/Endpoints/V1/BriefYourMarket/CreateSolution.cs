using Microsoft.TeamFoundation.SourceControl.WebApi;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;
using Onboarding.Tool.Model.Enums;
using Onboarding.Tool.Server.Extensions;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Serilog;
using Onboarding.Tool.Server.Services.Dashboard.Background;
using Onboarding.Tool.Model.Dashboard.Setups;

namespace Onboarding.Tool.Server.Endpoints.V1.BriefYourMarket;

public static class CreateBriefYourMarketSolution
{
    public static void MapCreateBriefYourMarketProjectEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/briefyourmarket/project")
            .AddEndpointFilter<InstanceValidationFilter>()
            .RequireAuthorization("AllowedEmailOnly");

        group.MapPost("/{domain}/templates", CreateEmailSolution);
        group.MapPost("/{domain}/templates/confirm", ConfirmEmailSolution);

        group.MapPost("/{domain}/cards", CreateCards);
        group.MapPost("/{domain}/cards/confirm", ConfirmCards);
        group.MapGet("/{domain}/cards/status", GetCardStatus);

        group.MapPost("/{domain}/letter", CreateLetters);
        group.MapPost("/{domain}/letter/confirm", ConfirmLetters);
    }

    private static async Task<IResult> CreateEmailSolution(
        ITfsService tfsService,
        ITemplateService templateService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            GitPush push = await templateService.CreateTemplateGitPushAsync(instance);

            GitPush? branchCreated = await tfsService.CreateGitBranchAsync(push);

            if (branchCreated == null)
                return Results.BadRequest(new { error = "Failed to create branch", branchName = push.RefUpdates.First().Name });

            GitPullRequest? pullRequest = await tfsService.CreatePullRequestAsync(branchCreated);
            if (pullRequest == null)
                return Results.BadRequest("Failed to create pull request.");

            bool updated = await templateService.UpdateInstanceTemplateStatusAsync(instance, TemplateConfirmationState.NotCreated, TemplateConfirmationState.Unconfirmed);
            if (!updated)
                return Results.BadRequest("This instance is not pending template creation.");

            string pullRequestUrl = string.Format("{0}/pullrequest/{1}", pullRequest.Repository.RemoteUrl, pullRequest.CodeReviewId);

            SetupResult result = new()
            {
                Success = true,
                Message = $"Created pull request at {pullRequestUrl}, please review and confirm once the templates have been pushed."
            };

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create templates");
            return Results.InternalServerError("Failed to create solution.");
        }
    }

    private static async Task<IResult> ConfirmEmailSolution(
        ITemplateService templateService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        bool updated = await templateService.UpdateInstanceTemplateStatusAsync(instance, TemplateConfirmationState.Unconfirmed, TemplateConfirmationState.Confirmed);
        if (!updated)
            return Results.BadRequest("This instance is not pending template confirmation.");

        SetupResult setupResult = new()
        {
            Success = true,
            Message = "Confirmed templates."
        };

        return Results.Ok(setupResult);
    }

    private static async Task<IResult> CreateCards(
        ITemplateService templateService,
        IDashboardBackgroundService dashboardBackgroundService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            await dashboardBackgroundService.QueueCardCreationAsync(instance);
            
            bool updated = await templateService.UpdateInstanceDirectMailStatusAsync(instance, TemplateConfirmationState.NotCreated, TemplateConfirmationState.Processing);
            if (!updated)
                return Results.BadRequest("This instance is not pending direct mail creation.");

            SetupResult setupResult = new()
            {
                Success = true,
                Message = "Queued DirectMail creation."
            };

            return Results.Ok(setupResult);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create cards");
            return Results.BadRequest();
        }
    }

    private static async Task<IResult> ConfirmCards(
        ITemplateService templateService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        bool updated = await templateService.UpdateInstanceDirectMailStatusAsync(instance, TemplateConfirmationState.Unconfirmed, TemplateConfirmationState.Confirmed);
        if (!updated)
            return Results.BadRequest("This instance is not pending direct mail confirmation.");

        SetupResult setupResult = new()
        {
            Success = true,
            Message = "Confirmed cards."
        };

        return Results.Ok(setupResult);
    }

    private static async Task<IResult> GetCardStatus(
        ITemplateService templateService,
        HttpContext context)
    {
        Instance instance = (Instance?)context.Items["ValidatedInstance"] ?? throw new InvalidOperationException();
        TemplateConfirmationState templateConfirmationState = await templateService.GetInstanceDirectMailStatusAsync(instance);

        return Results.Ok(templateConfirmationState);
    }

    private static async Task<IResult> CreateLetters(
        IInstanceLetterService instanceLetterService,
        ITemplateService templateService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            LetterHeadResult result = await instanceLetterService.CreateDefaultLetterHeadAsync(instance);
            
            bool updated = await templateService.UpdateInstanceLetterStatusAsync(instance, TemplateConfirmationState.NotCreated, TemplateConfirmationState.Unconfirmed);
            if (!updated)
                return Results.BadRequest("This instance is not pending letter creation.");

            SetupResult setupResult = new()
            {
                Success = true,
                Message = "Create letterhead in instance."
            };

            return Results.Ok(setupResult);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create letterhead");
            return Results.InternalServerError();
        }
    }

    private static async Task<IResult> ConfirmLetters(
        ITemplateService templateService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        bool updated = await templateService.UpdateInstanceLetterStatusAsync(instance, TemplateConfirmationState.Unconfirmed, TemplateConfirmationState.Confirmed);
        if (!updated)
            return Results.BadRequest("This instance is not pending letter confirmation.");

        SetupResult setupResult = new()
        {
            Success = true,
            Message = "Confirmed letterhead."
        };

        return Results.Ok(setupResult);
    }
}
