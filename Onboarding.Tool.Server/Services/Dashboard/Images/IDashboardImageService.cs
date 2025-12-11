using Onboarding.Tool.Model.Dashboard;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Model.Enums.BriefYourMarket.Images;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Services.Dashboard.Images;

public interface IDashboardImageService
{
    public Task<Image> UploadImageAsync(Instance instance, ImageType type, string fileName, byte[] bytes);

    public Task<bool> UpdateImageOverridesAsync(Instance instance, CardPreviewInfo cardPreviewInfo);

    public Task<bool> DeleteAlternativeLogoAsync(Instance instance);

    public Task<bool> ResetImageOverridesAsync(Instance instance);

    public Task<byte[]> GetImageAsync(Instance instance, ImageType type);
}
