using Microsoft.EntityFrameworkCore;
using Onboarding.Tool.Model.Enums.BriefYourMarket.Images;
using Onboarding.Tool.Data;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Server.Helpers.Files;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using EfImage = Onboarding.Tool.Model.Data.Image;
using Onboarding.Tool.Model.Dashboard;

namespace Onboarding.Tool.Server.Services.Dashboard.Images;

public class DashboardImageService : IDashboardImageService
{
    private readonly AppDbContext _context;

    public DashboardImageService(AppDbContext contex)
    {
        _context = contex;
    }

    public async Task<Image> UploadImageAsync(Instance instance, ImageType type, string fileName, byte[] bytes)
    {
        await RemoveExistingImageAsync(instance, type);

        Image image = ImageHelper.CreateDatabaseImage(fileName, bytes, type, instance.Id);
        _context.Image.Add(image);

        await _context.SaveChangesAsync();

        if (type == ImageType.Logo)
        {
            await CreatePrintImageAsync(instance, ImageType.PrintLogo, fileName, bytes);
        }
        else if (type == ImageType.logoAlternate)
        {
            await CreatePrintImageAsync(instance, ImageType.PrintLogoAlternate, fileName, bytes);
        }

        return image;
    }
    
    public async Task<bool> UpdateImageOverridesAsync(Instance instance, CardPreviewInfo cardPreviewInfo)
    {
        EfImage? logo = await _context.Set<EfImage>()
            .Where(i => i.InstanceId == instance.Id &&
                (i.Type == ImageType.PrintLogoAlternate || i.Type == ImageType.PrintLogo))
            .OrderBy(i => i.Type == ImageType.PrintLogoAlternate ? 0 : 1)
            .FirstOrDefaultAsync();

        if (logo == null)
            return false;

        logo.CardWidthOverride = cardPreviewInfo.Width;
        logo.CardHeightOverride = cardPreviewInfo.Height;
        logo.CardXOverride = cardPreviewInfo.X;
        logo.CardYOverride = cardPreviewInfo.Y;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAlternativeLogoAsync(Instance instance)
    {
        List<EfImage> images = await _context.Set<EfImage>()
            .Where(i => i.InstanceId == instance.Id && (i.Type == ImageType.logoAlternate || i.Type == ImageType.PrintLogoAlternate))
            .ToListAsync();

        if (images.Count == 0)
            return true;

        _context.Image.RemoveRange(images);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ResetImageOverridesAsync(Instance instance)
    {
        EfImage? logo = await _context.Set<EfImage>()
                    .Where(i => i.InstanceId == instance.Id && i.Type == ImageType.PrintLogo)
                    .FirstOrDefaultAsync();

        if (logo == null)
            return false;

        logo.CardWidthOverride = null;
        logo.CardHeightOverride = null;
        logo.CardXOverride = null;
        logo.CardYOverride = null;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<byte[]> GetImageAsync(Instance instance, ImageType type)
    {
        Image? existingImage = await _context.Image.FirstOrDefaultAsync(i => i.InstanceId == instance.Id && i.Type == type);
        if (existingImage == null)
            return [];

        return existingImage.Data;
    }

    private async Task CreatePrintImageAsync(Instance instance, ImageType type, string fileName, byte[] bytes)
    {
        await RemoveExistingImageAsync(instance, type);

        string tifName = Path.GetFileNameWithoutExtension(fileName) + ".tif";
        Image image = ImageHelper.CreatePrintImage(tifName, type, bytes, instance.Id);

        _context.Image.Add(image);

        await _context.SaveChangesAsync();
    }

    private async Task RemoveExistingImageAsync(Instance instance, ImageType type)
    {
        Image? existingImage = await _context.Image.FirstOrDefaultAsync(i => i.InstanceId == instance.Id && i.Type == type);
        if (existingImage != null)
            _context.Image.Remove(existingImage);
    }
}
