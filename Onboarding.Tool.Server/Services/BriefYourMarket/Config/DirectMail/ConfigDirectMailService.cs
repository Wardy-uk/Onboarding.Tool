using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BriefYourMarket.Branches;
using Onboarding.Tool.Model.BriefYourMarket.Brands;
using Onboarding.Tool.Model.BriefYourMarket.Cards;
using Onboarding.Tool.Model.BriefYourMarket.Images;
using Onboarding.Tool.Model.Enums.BriefYourMarket.DirectMail;
using Onboarding.Tool.Model.Enums.BriefYourMarket.Images;
using Onboarding.Tool.Model.Robocop.DirectMail;
using Onboarding.Tool.Model.Robocop.Images;
using System.Text;
using System.Text.Json;
using Onboarding.Tool.Model.BriefYourMarket.Instances;
using Serilog;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using Onboarding.Tool.Model.BriefYourMarket.Instances.Configurations;
using Onboarding.Tool.Server.Helpers.Files;
using Onboarding.Tool.Model.Enums;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Config.DirectMail;

public class ConfigDirectMailService : IConfigDirectMailService
{
    private readonly IHttpHelper _httpHelper;
    private readonly IConfigDatabaseService _configService;
    private readonly IOnboardingDataService _onboardingDataService;
    private readonly IDashboardProgressService _dashboardProgressService;
    private readonly ConfigSettings _settings;
    private readonly InstanceTemplateSettings _templateSettings;
    private readonly InstanceConfig _instanceConfig;
    private readonly int _directMailDirectoryId;

    public ConfigDirectMailService(
        IHttpHelper httpHelper,
        IConfigDatabaseService configService,
        IOnboardingDataService onboardingDataService,
        IDashboardProgressService dashboardProgressService,
        IOptions<ConfigSettings> options,
        IOptions<InstanceTemplateSettings> templateOptions)
    {
        _httpHelper = httpHelper;
        _configService = configService;
        _onboardingDataService = onboardingDataService;
        _dashboardProgressService = dashboardProgressService;
        _settings = options.Value;
        _templateSettings = templateOptions.Value;
        _directMailDirectoryId = FindFolderByNameAsync("DirectMail").Result ?? throw new InvalidOperationException("Cannot find base DirectMail directory");

        _instanceConfig = JsonConfigHelper.ReadInstanceConfig<InstanceConfig>(templateOptions.Value.Directory, "Instance", "instance.json");
    }

    public async Task<int?> FindFolderByNameAsync(string name)
    {
        string cleanName = name.Replace("-", "");
        List<ImageDirectory>? directories = await GetFoldersAsync();

        return directories?.FirstOrDefault(d => d.Path == $"{cleanName}/")?.Id;
    }

    public async Task<int> CreateFolderAsync(string name, int? parentDirectoryId = null)
    {
        string folderName = Path.GetFileName(name);
        string cleanFolderName = folderName.Replace("-", "");

        CreateDirectoryPayload payload = new()
        {
            Name = cleanFolderName,
            ParentDirectoryId = parentDirectoryId
        };

        ImageDirectory directory = await _httpHelper.ExecuteRequestAsync<ImageDirectory>(HttpMethod.Post, "ConfigApi", _settings.Domain, "api/folders", _settings.ApiKey, payload);

        return directory.Id;
    }

    public async Task<int> FindOrCreateFolderAsync(string name)
    {
        int? folder = await FindFolderByNameAsync(name);
        if (folder.HasValue)
            return folder.Value;

        return await CreateFolderAsync(name, _directMailDirectoryId);
    }

    public async Task<bool> UploadFilesAsync(int directory, List<UploadImage> uploadImages)
    {
        List<Task> uploadFileTasks = new();

        foreach (UploadImage image in uploadImages)
        {
            Task task = UploadFileAsync(directory, image.FileName, image.Data);
            uploadFileTasks.Add(task);
        }

        await Task.WhenAll(uploadFileTasks);

        return true;
    }

    public async Task<List<int>> CreateCardsForInstanceAsync(Instance instance)
    {
        var (defaultBrandSettings, branches, cardSettings) = await GetCardCreationDataAsync(instance);
        Dictionary<DirectMailCategory, int> libraryIds = await GetDirectMailLibrariesAsync(instance, cardSettings);
        IEnumerable<Image> images = await _onboardingDataService.GetSetupImagesAsync(instance);
        Image logo = images.FirstOrDefault(i => i.Type == ImageType.PrintLogoAlternate)
            ?? images.FirstOrDefault(i => i.Type == ImageType.PrintLogo)
            ?? throw new FileNotFoundException("No logo found");

        var tasks = new List<Task>();

        var categoryCardIds = new ConcurrentDictionary<DirectMailCategory, List<int>>();

        foreach (CardSetting card in cardSettings)
        {
            if (card.Category == DirectMailCategory.Build)
            {
                tasks.Add(Task.Run(async () =>
                {
                    int cardId = await CreateCardAsync(instance.Subdomain, defaultBrandSettings!.ToList(), card, logo);
                    categoryCardIds.AddOrUpdate(
                        card.Category,
                        _ => new List<int> { cardId },
                        (_, list) => { lock (list) list.Add(cardId); return list; }
                    );
                }));
            }
            else
            {
                foreach (Branch branch in branches)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        List<BrandSetting> branchBrandSettings = GetMergedBrandSettings(branch, defaultBrandSettings);
                        int cardId = await CreateCardAsync(instance.Subdomain, branch, branchBrandSettings, card, logo);
                        categoryCardIds.AddOrUpdate(
                            card.Category,
                            _ => new List<int> { cardId },
                            (_, list) => { lock (list) list.Add(cardId); return list; }
                        );
                    }));
                }
            }
        }

        await Task.WhenAll(tasks);
        await AddDirectMailCardsToLibraries(libraryIds, categoryCardIds.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

        if (_instanceConfig.QualityAssuranceInstance != null)
            await AddDirectMailLibrariesToQaInstanceAsync(_instanceConfig.QualityAssuranceInstance.Value, libraryIds.Values.ToList());


        await AddDirectMailLibrariesToQaInstanceAsync(instance.Id, libraryIds.Values.ToList());

        return [];
    }

    public async Task<bool> AddDefaultPrintLibrariesToInstanceAsync(Instance instance)
    {
        if (_instanceConfig.DefaultPrintLibraries == null || _instanceConfig.DefaultPrintLibraries.Count == 0)
            return false;

        bool isSetup = await _dashboardProgressService.IsSetupStepStatusAsync(instance, "setupDefaultPrint", TemplateConfirmationState.Confirmed);
        if (isSetup)
            return false;

        try
        {
            List<Task> addLibraryTasks = new();

            foreach (int library in _instanceConfig.DefaultPrintLibraries)
            {
                addLibraryTasks.Add(Task.Run(async () =>
                {
                    await _configService.AddLibraryCardtoInstanceAsync(instance, library);
                }));
            }

            await Task.WhenAll(addLibraryTasks);

        }
        catch
        {
            throw;
        }

        await _dashboardProgressService.SetSetupStatusAsync(instance, "setupDefaultPrint", TemplateConfirmationState.Confirmed);
        return true;
    }

    private async Task AddDirectMailLibrariesToQaInstanceAsync(int instanceId, List<int> libraryIds)
    {
        List<Task> tasks = new();
        Instance instance = new()
        {
            Id = instanceId,
            Domain = "",
            Subdomain = "",
            Database = "",
            Server = ""
        };

        foreach (int library in libraryIds)
        {
            tasks.Add(Task.Run(async () =>
            {
                await _configService.AddLibraryCardtoInstanceAsync(instance, library);
            }));
        }

        await Task.WhenAll(tasks);
    }

    private async Task AddDirectMailCardsToLibraries(Dictionary<DirectMailCategory, int> libraryIds, Dictionary<DirectMailCategory, List<int>> cardIds)
    {
        List<Task> addCardTasks = new();

        foreach (DirectMailCategory category in libraryIds.Keys)
        {
            if (!cardIds.TryGetValue(category, out List<int>? categoryCardIds))
                continue;

            int libraryId = libraryIds[category];

            IEnumerable<Task> categoryTasks = categoryCardIds.Select(cardId =>
                AddCardToLibraryAsync(libraryId, cardId));

            addCardTasks.AddRange(categoryTasks);
        }

        await Task.WhenAll(addCardTasks);
    }

    private async Task AddCardToLibraryAsync(int libraryId, int cardId)
    {
        AddCardToLibraryPayload payload = new()
        {
            LibraryId = libraryId,
            ProductId = cardId,
            RequiresApproval = true
        };

        await _httpHelper.ExecuteRequestAsync<object>(HttpMethod.Post, "ConfigApi", _settings.Domain, "api/directmail/libraryproducts", _settings.ApiKey, payload);
    }

    private async Task<bool> UploadFileAsync(int directory, string fileName, byte[] content)
    {
        if (await _configService.DoesFileExistAsync(directory, fileName))
        {
            await _configService.DeleteFileAsync(directory, fileName);
        }

        HttpResponseMessage response = await _httpHelper.UploadFileAsync("ConfigApi", _settings.Domain, $"api/files/folders/{directory}", fileName, content, _settings.ApiKey);
        response.EnsureSuccessStatusCode();

        return true;
    }


    private async Task<(IEnumerable<BrandSetting> defaultBrandSettings, IEnumerable<Branch> branches, List<CardSetting> cardSettings)> GetCardCreationDataAsync(Instance instance)
    {
        Task<IEnumerable<BrandSetting>> defaultBrandSettingsTask = _onboardingDataService.GetSetupInstanceSettingsAsync(instance);
        Task<IEnumerable<Branch>> branchesTask = _onboardingDataService.GetSetupBranchesAsync(instance);
        Task<List<CardSetting>> cardSettingsTask = GetCardsToCreateAsync();

        await Task.WhenAll(defaultBrandSettingsTask, branchesTask, cardSettingsTask);

        return (
            await defaultBrandSettingsTask,
            await branchesTask,
            await cardSettingsTask
        );
    }

    private static List<BrandSetting> GetMergedBrandSettings(Branch branch, IEnumerable<BrandSetting> defaultSettings)
    {
        Dictionary<string, BrandSetting> mergedSettings = defaultSettings.ToDictionary(
            s => s.Setting,
            s => new BrandSetting { Setting = s.Setting, Value = s.Value });

        foreach (BrandSetting setting in branch.Settings)
        {
            mergedSettings[setting.Setting] = setting;
        }

        return [.. mergedSettings.Values];
    }

    private async Task<Dictionary<DirectMailCategory, int>> GetDirectMailLibrariesAsync(Instance instance, List<CardSetting> cards)
    {
        IEnumerable<DirectMailCategory> categories = cards.Select(c => c.Category).Distinct();
        var libraryTasks = categories.Select(async category =>
        {
            string categoryName = $"{instance.Subdomain} - {category}";
            int id = await GetOrCreateDirectMailLibraryAsync(categoryName, instance);
            return new KeyValuePair<DirectMailCategory, int>(category, id);
        });

        var results = await Task.WhenAll(libraryTasks);
        return results.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private async Task<int> CreateCardAsync(string instance, List<BrandSetting> brandSettings, CardSetting cardSetting, Image logo)
    {
        string cleanSubdomain = instance.Replace("-", "");
        string templateContent = await RenderCardTemplateAsync(cardSetting, logo, cleanSubdomain, brandSettings);

        DirectMailCard card = CreateDirectMailCard(instance, cardSetting, cleanSubdomain, templateContent);
        Log.Information("Card Creation {@Card}", card);

        int cardId = await _httpHelper.ExecuteRequestAsync<int>(HttpMethod.Post, "ConfigAPI", _settings.Domain, "api/directmail/products/", _settings.ApiKey, card);
        return cardId;
    }

    private async Task<int> CreateCardAsync(string instance, Branch branch, List<BrandSetting> brandSettings, CardSetting cardSetting, Image logo)
    {
        string cleanSubdomain = instance.Replace("-", "");
        string templateContent = await RenderCardTemplateAsync(cardSetting, branch, logo, cleanSubdomain, brandSettings);

        DirectMailCard card = CreateDirectMailCard(instance, branch, cardSetting, cleanSubdomain, templateContent);
        Log.Information("Card Creation {@Card}", card);

        int cardId = await _httpHelper.ExecuteRequestAsync<int>(HttpMethod.Post, "ConfigAPI", _settings.Domain, "api/directmail/products/", _settings.ApiKey, card);
        return cardId;
    }

    private static async Task<string> RenderCardTemplateAsync(CardSetting cardSetting, Image logo, string cleanSubdomain, List<BrandSetting> brandSettings)
    {
        string website = brandSettings.Where(bs => bs.Setting == "bannerWebsite").First().Value;
        string company = brandSettings.Where(bs => bs.Setting == "registeredName").First().Value;

        string templateBase = await cardSetting.GetContentAsync();
        object context = new { Website = website, Company = company, Instance = cleanSubdomain, Settings = brandSettings, Logo = logo };
        Scriban.Template template = Scriban.Template.Parse(templateBase);

        string result = template.Render(context);
        return result;
    }

    private static async Task<string> RenderCardTemplateAsync(CardSetting cardSetting, Branch branch, Image logo, string cleanSubdomain, List<BrandSetting> brandSettings)
    {
        string templateBase = await cardSetting.GetContentAsync();
        object context = new { Branch = branch, Instance = cleanSubdomain, Settings = brandSettings, Logo = logo };
        Scriban.Template template = Scriban.Template.Parse(templateBase);

        string result = template.Render(context);
        return result;
    }

    private static DirectMailCard CreateDirectMailCard(string instance, Branch branch, CardSetting cardSetting, string cleanSubdomain, string templateContent)
    {
        return new DirectMailCard
        {
            Title = $"{instance} - {cardSetting.Title} - {branch.Name}",
            GlobalCode = FormatGlobalCode(cardSetting.GlobalCodeSuffix, cleanSubdomain, branch.Name),
            Status = "Live",
            TypeId = cardSetting.TypeId,
            PriceModelId = cardSetting.PriceModelId,
            CategoryId = 1,
            Keywords = cardSetting.Keywords,
            PrintProviderId = cardSetting.PrintProviderId,
            StockIds = cardSetting.GetStockIds(),
            DeliveryMethodIds = cardSetting.GetDeliveryMethodIds(),
            PaperTypeIds = cardSetting.GetPaperTypeIdsIds(),
            LaminationIds = cardSetting.GetLaminationIds(),
            EnvelopeTypeIds = [],
            IsDuplex = true,
            PageCount = 2,
            MinOrderQuantity = 1,
            MaxOrderQuantity = 100000,
            DataType = cardSetting.DataType,
            IsQuickOrder = false,
            Template = Convert.ToBase64String(Encoding.UTF8.GetBytes(templateContent))
        };
    }

    private static DirectMailCard CreateDirectMailCard(string instance, CardSetting cardSetting, string cleanSubdomain, string templateContent)
    {
        return new DirectMailCard
        {
            Title = $"{instance} - {cardSetting.Title}",
            GlobalCode = FormatGlobalCode(cardSetting.GlobalCodeSuffix, cleanSubdomain, ""),
            Status = "Live",
            TypeId = cardSetting.TypeId,
            PriceModelId = cardSetting.PriceModelId,
            CategoryId = 1,
            Keywords = cardSetting.Keywords,
            PrintProviderId = cardSetting.PrintProviderId,
            StockIds = cardSetting.GetStockIds(),
            DeliveryMethodIds = cardSetting.GetDeliveryMethodIds(),
            PaperTypeIds = cardSetting.GetPaperTypeIdsIds(),
            LaminationIds = cardSetting.GetLaminationIds(),
            EnvelopeTypeIds = [],
            IsDuplex = true,
            PageCount = 2,
            MinOrderQuantity = 1,
            MaxOrderQuantity = 100000,
            DataType = "Contact",
            IsQuickOrder = false,
            Template = Convert.ToBase64String(Encoding.UTF8.GetBytes(templateContent))
        };
    }

    private static string FormatGlobalCode(string prefix, string instance, string branch)
    {
        prefix = prefix.ToUpper().Replace(" ", "");
        string fullInstance = instance.ToUpper().Replace(" ", "");
        string fullBranch = branch.ToUpper().Replace(" ", "");

        string instanceShort = fullInstance[..Math.Min(6, fullInstance.Length)];
        string branchShort = fullBranch[..Math.Min(6, fullBranch.Length)];

        string baseCode = prefix + instanceShort + branchShort;
        int remainingLength = 20 - baseCode.Length;

        string hash = GetShortHash(fullInstance + fullBranch, remainingLength);

        return (baseCode + hash).Substring(0, 20);
    }

    private async Task<List<CardSetting>> GetCardsToCreateAsync()
    {
        string cardsPath = Path.Combine(_templateSettings.Directory, "Cards", "cards.json");

        if (!File.Exists(cardsPath))
            throw new FileNotFoundException($"Cards config file not found at: {cardsPath}");

        string content = await File.ReadAllTextAsync(cardsPath);

        if (string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("Cards config file is empty");

        return JsonSerializer.Deserialize<List<CardSetting>>(content)
            ?? throw new InvalidOperationException("Unable to deserialize card settings from config file");
    }

    private async Task<int?> GetDirectMailLibraryAsync(string name)
    {
        List<DirectMailLibrary>? libraries = await _httpHelper.ExecuteRequestAsync<List<DirectMailLibrary>>(HttpMethod.Get, "ConfigAPI", _settings.Domain, "api/directmail/libraries", _settings.ApiKey);
        return libraries?.FirstOrDefault(l => l.Name == name)?.Id;
    }

    private async Task<int> CreateDirectMailLibraryAsync(string name, Instance instance)
    {
        CreateDirectMailLibraryPayload payload = new() 
        { 
            Name = name
        };
        CreateDirectoryResponse directory = await _httpHelper.ExecuteRequestAsync<CreateDirectoryResponse>(HttpMethod.Post, "ConfigAPI", _settings.Domain, "api/directmail/libraries", _settings.ApiKey, payload);
        directory.InstanceIds = new List<int>() { instance.Id };

        await _httpHelper.ExecuteRequestAsync<object>(HttpMethod.Put, "ConfigAPI", _settings.Domain, "api/directmail/libraries", _settings.ApiKey, directory);

        return directory.Id;
    }

    private async Task<int> GetOrCreateDirectMailLibraryAsync(string name, Instance instance)
    {
        int? library = await GetDirectMailLibraryAsync(name);
        return library ?? await CreateDirectMailLibraryAsync(name, instance);
    }

    private async Task<List<ImageDirectory>?> GetFoldersAsync()
    {
        return await _httpHelper.ExecuteRequestAsync<List<ImageDirectory>>(HttpMethod.Get, "ConfigAPI", _settings.Domain, "api/folders", _settings.ApiKey);
    }

    private static string GetShortHash(string input, int length)
    {
        using (var md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hashBytes.Length && sb.Length < length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }

            return sb.ToString().Substring(0, Math.Min(length, sb.Length));
        }
    }
}