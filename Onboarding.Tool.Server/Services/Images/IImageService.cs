using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Services.Images;

public interface IImageService
{
    public Task<string> UploadAsync(Instance instance, string fileName, byte[] bytes);
}
