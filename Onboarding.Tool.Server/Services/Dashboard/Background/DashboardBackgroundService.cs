using Onboarding.Tool.Model.BriefYourMarket.Brands;
using Onboarding.Tool.Model.BriefYourMarket.Images;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Onboarding.Tool.Model.Enums;
using Onboarding.Tool.Model.Enums.BriefYourMarket.Images;
using Onboarding.Tool.Server.Helpers.Files;
using Serilog;
using System.Threading.Channels;

namespace Onboarding.Tool.Server.Services.Dashboard.Background;

public class DashboardBackgroundService : BackgroundService, IDashboardBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Channel<Instance> _queue;
    private readonly ChannelWriter<Instance> _writer;
    private readonly ChannelReader<Instance> _reader;

    public DashboardBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var options = new BoundedChannelOptions(100)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = false
        };

        _queue = Channel.CreateBounded<Instance>(options);
        _writer = _queue.Writer;
        _reader = _queue.Reader;
    }

    public Task QueueCardCreationAsync(Instance instance)
    {
        if (!_writer.TryWrite(instance))
        {
            Log.Warning("Failed to queue card creation for instance {InstanceId} - queue may be full", instance.Id);
            return Task.FromException(new InvalidOperationException($"Failed to queue card creation for instance {instance.Id} - queue may be full"));
        }

        Log.Information("Successfully queued card creation for instance {InstanceId}", instance.Id);
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Information("DashboardBackgroundService started");

        try
        {
            await foreach (var instance in _reader.ReadAllAsync(stoppingToken))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await ProcessCardCreationAsync(instance);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error processing card creation for instance {InstanceId}", instance.Id);
                    }
                }, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("DashboardBackgroundService cancellation requested");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error in DashboardBackgroundService execution");
        }
        finally
        {
            Log.Information("DashboardBackgroundService stopped");
        }
    }

    private async Task ProcessCardCreationAsync(Instance instance)
    {
        using var scope = _serviceProvider.CreateScope();
        IOnboardingDataService onboardingDataService = scope.ServiceProvider.GetRequiredService<IOnboardingDataService>();
        IConfigDirectMailService robocopService = scope.ServiceProvider.GetRequiredService<IConfigDirectMailService>();
        ITemplateService templateService = scope.ServiceProvider.GetRequiredService<ITemplateService>();
        IDashboardProgressService dashboardProgressService = scope.ServiceProvider.GetRequiredService<IDashboardProgressService>();

        try
        {
            Log.Information("Starting card creation for instance {InstanceId}", instance.Id);

            await CreateCardsInternal(onboardingDataService, robocopService, instance);

            await templateService.UpdateInstanceDirectMailStatusAsync(
                instance,
                TemplateConfirmationState.Processing,
                TemplateConfirmationState.Unconfirmed);

            Log.Information("Completed card creation for instance {InstanceId}", instance.Id);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Card creation failed for instance {InstanceId}", instance.Id);

            try
            {
                await templateService.UpdateInstanceDirectMailStatusAsync(
                    instance,
                    TemplateConfirmationState.Processing,
                    TemplateConfirmationState.NotCreated);
            }
            catch (Exception updateEx)
            {
                Log.Error(updateEx, "Failed to update status after card creation failure for instance {InstanceId}", instance.Id);
            }
        }
    }

    private static async Task CreateCardsInternal(
        IOnboardingDataService onboardingDataService,
        IConfigDirectMailService robocopService,
        Instance instance)
    {
        Task<int> folderTask = robocopService.FindOrCreateFolderAsync($"DirectMail/{instance.Subdomain}");
        Task<IEnumerable<BrandSetting>> brandSettingsTask = onboardingDataService.GetSetupInstanceSettingsAsync(instance);
        Task<IEnumerable<Image>> imagesTask = onboardingDataService.GetSetupImagesAsync(instance);

        await Task.WhenAll(folderTask, brandSettingsTask, imagesTask);

        int folder = await folderTask;
        IEnumerable<BrandSetting> brandSettings = await brandSettingsTask;
        IEnumerable<Image> images = await imagesTask;

        string primaryColour = brandSettings.FirstOrDefault(b => b.Setting == "theme.colourPrimary")?.Value ?? "#ffffff";
        string secondaryColour = brandSettings.FirstOrDefault(b => b.Setting == "theme.colourSecondary")?.Value ?? "#ffffff";

        Image logo = images.FirstOrDefault(i => i.Type == ImageType.PrintLogoAlternate)
            ?? images.FirstOrDefault(i => i.Type == ImageType.PrintLogo)
            ?? throw new FileNotFoundException("No logo found");

        Image? secondaryLogo = null;
        if (logo.Type == ImageType.PrintLogoAlternate)
        {
            secondaryLogo = images.FirstOrDefault(i => i.Type == ImageType.PrintLogo);
        }


        List<UploadImage> uploadImages = new()
        {
            new() { FileName = logo.Name, Data = logo.Data },
            new() { FileName = "colourblockprimary.tif", Data = ImageHelper.GetBrandColourBytes(primaryColour) },
            new() { FileName = "colourblocksecondary.tif", Data = ImageHelper.GetBrandColourBytes(secondaryColour) }
        };

        if (secondaryLogo != null)
            uploadImages.Add(new UploadImage() { FileName = secondaryLogo.Name, Data = secondaryLogo.Data });

        Log.Information("Starting file upload and card creation for instance {InstanceId}", instance.Id);

        await Task.WhenAll(
            robocopService.UploadFilesAsync(folder, uploadImages),
            robocopService.CreateCardsForInstanceAsync(instance));

        Log.Information("Completed file upload and card creation for instance {InstanceId}", instance.Id);
    }
}