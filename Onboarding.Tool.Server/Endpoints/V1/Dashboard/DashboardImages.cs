using Onboarding.Tool.Model.BriefYourMarket.Images;
using Onboarding.Tool.Model.Enums.BriefYourMarket.Images;
using Onboarding.Tool.Server.Extensions;
using Onboarding.Tool.Server.Helpers.Files;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using Image = Onboarding.Tool.Model.Data.Image;
using Onboarding.Tool.Model.Dashboard;
using Serilog;

namespace Onboarding.Tool.Server.Endpoints.V1.Dashboard;

public static class DashboardImages
{
    public static void MapBriefYourMarketDashboardImageEndpoints(this WebApplication app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/v1/dashboard/instance/{domain}/image")
            .AddEndpointFilter<InstanceValidationFilter>()
            .RequireAuthorization("AllowedEmailOnly");

        group.MapPost("/upload/{imageType}", UploadImage);
        group.MapPost("/preview/remove-alternate-logo", RemoveAlternativeLogoAsync);
        group.MapPost("/preview/override", OverrideImagePositionScaleAsync);
        group.MapPost("/preview/reset", ResetImagePositionScaleAsync);
        group.MapGet("/{imageType}", GetImageAsync);
    }

    private static async Task<IResult> UploadImage(
                IDashboardImageService dashboardImageService,
                HttpContext httpContext,
                ImageType imageType,
                UploadImage uploadImage)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        if (uploadImage?.Data == null || string.IsNullOrWhiteSpace(uploadImage.FileName))
            return Results.BadRequest("Invalid image data");

        try
        {
            byte[]? processedImageBytes = ImageHelper.ProcessImage(uploadImage, imageType);
            if (processedImageBytes == null)
                return Results.BadRequest("Invalid image format or size");

            Image image = await dashboardImageService.UploadImageAsync(instance, imageType, uploadImage.FileName, processedImageBytes);

            return Results.Ok(image);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to upload image");
            return Results.Problem("Failed to upload image");
        }
    }

    private static async Task<IResult> RemoveAlternativeLogoAsync(
        IDashboardImageService dashboardImageService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await dashboardImageService.DeleteAlternativeLogoAsync(instance);
            return Results.Ok(success);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete alternative logo");
            return Results.Problem("Failed to delete alternative logo");
        }
    }

    private static async Task<IResult> OverrideImagePositionScaleAsync(
        IDashboardImageService dashboardImageService,
        HttpContext httpContext,
        CardPreviewInfo cardPreviewInfo)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await dashboardImageService.UpdateImageOverridesAsync(instance, cardPreviewInfo);
            return Results.Ok(success);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update image overrides");
            return Results.Problem("Failed to update image overrides");
        }
    }

    private static async Task<IResult> ResetImagePositionScaleAsync(
        IDashboardImageService dashboardImageService,
        HttpContext httpContext)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            bool success = await dashboardImageService.ResetImageOverridesAsync(instance);
            return Results.Ok(success);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to update image overrides");
            return Results.Problem("Failed to update image overrides");
        }
    }

    private static async Task<IResult> GetImageAsync(
        IDashboardImageService dashboardImageService,
        HttpContext httpContext,
        ImageType imageType)
    {
        Instance instance = (Instance?)httpContext.Items["ValidatedInstance"] ?? throw new InvalidOperationException();

        try
        {
            byte[] bytes = await dashboardImageService.GetImageAsync(instance, imageType);
            return Results.Ok(bytes);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to fetch image");
            return Results.Problem("Failed to fetch image");
        }
    }
}
