using Onboarding.Tool.Model.BriefYourMarket.Images;
using Onboarding.Tool.Model.BriefYourMarket.Instances;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Config.DirectMail;

public interface IConfigDirectMailService
{
    public Task<int?> FindFolderByNameAsync(string name);

    public Task<int> CreateFolderAsync(string name, int? parentDirectoryId = null);

    public Task<int> FindOrCreateFolderAsync(string name);

    public Task<bool> UploadFilesAsync(int directory, List<UploadImage> uploadImages);

    public Task<List<int>> CreateCardsForInstanceAsync(Instance instance);

    public Task<bool> AddDefaultPrintLibrariesToInstanceAsync(Instance instance);
}
