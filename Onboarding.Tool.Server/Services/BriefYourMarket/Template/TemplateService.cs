using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Onboarding.Tool.Data;
using Onboarding.Tool.Model.BriefYourMarket.Branches;
using Onboarding.Tool.Model.BriefYourMarket.Brands;
using Onboarding.Tool.Model.BriefYourMarket.Images;
using Onboarding.Tool.Model.Enums;
using Onboarding.Tool.Model.Enums.BriefYourMarket.Images;
using Onboarding.Tool.Model.Enums.BriefYourMarket.Instance;
using Onboarding.Tool.Model.Exceptions;
using Microsoft.EntityFrameworkCore;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using EfInstance = Onboarding.Tool.Model.Data.Instance;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Template;

public class TemplateService : ITemplateService
{
    private readonly IOnboardingDataService _onboardingDataService;
    private readonly InstanceTemplateSettings _settings;
    private readonly AppDbContext _context;
    private const string BaseTargetPath = "Customisations/UK";
    private const string TemplateDefinitionsPath = "Template Definitions";
    private static readonly Dictionary<string, string> _additionalSettingsToCopy = new()
    {
        { "PortalLead.MarketAppraisalLink", "contactURL"},
        { "PortalLead.CallbackLink", "contactURL" },
        { "theme.companyName", "registeredName" },
        { "bannerAddress", "bannerWebsite" },
        { "mainTitlesColour", "theme.colourPrimary" },
        { "fontColour", "mainTextColour" },
        { "categoryBackground", "theme.colourPrimary" },
        { "linkColour", "theme.colourPrimary" },
        { "surveyBtnColour", "theme.colourPrimary" },
        { "surveyBtnHoverColour", "theme.colourSecondary" },
        { "surveyBorder", "theme.colourSecondary" },
        { "micrositeBorder", "theme.colourSecondary" },
        { "micrositeBtnLink", "bannerWebsite" },
        { "micrositeBtnColour", "theme.colourPrimary" },
        { "micrositeBtnHoverColour", "theme.colourSecondary" },
        { "micrositeIcon", "theme.colourSecondary" },
        { "micrositeIconHover", "theme.colourSecondary" },
    };
    private static readonly Dictionary<string, string> _branchConfigAsSettings = new()
    {
        { "ContactForms.NotificationEmailAddress", "email" },
        { "theme.companyEmail", "email" },
        { "companyEmail", "email" },
        { "FromAddress", "email" },
        { "ReplyToAddress", "email" },
        { "theme.companyPhone", "phone" },
        { "companyPhone", "phone" },
        { "footerText2", "address" },
        { "footerHTMLText2", "address-split" },
        { "footerText1", "name" },
        { "footerHTMLText1", "name" }
    };

    public TemplateService(
        IOnboardingDataService onboardingDataService,
        IOptions<InstanceTemplateSettings> settings,
        AppDbContext context)
    {
        _onboardingDataService = onboardingDataService;
        _settings = settings.Value;
        _context = context;
    }

    public async Task<GitPush> CreateTemplateGitPushAsync(Instance instance)
    {
        string branchName = $"refs/heads/onboarding-automation/{instance.Subdomain}-{DateTime.Now:ddMMMyyyyHH-mm}";
        string masterCommitId = "0000000000000000000000000000000000000000";

        IEnumerable<BrandSetting> brandSettings = await _onboardingDataService.GetSetupInstanceSettingsAsync(instance);
        IEnumerable<Image> images = await _onboardingDataService.GetSetupImagesAsync(instance);
        if (!images.Any())
        {
            throw new InvalidOperationException($"No images found for instance '{instance.Subdomain}'");
        }

        List<GitChange> gitChanges = [];

        try
        {
            Image logoImage = images.Any(i => i.Type == ImageType.logoAlternate)
                ? GetImageByType(images, ImageType.logoAlternate)
                : GetImageByType(images, ImageType.Logo);

            gitChanges.AddRange(await GetDefaultTemplateStructureAsync(instance));
            gitChanges.AddRange(await GetDefaultTemplateImagesAsync(instance));
            gitChanges.AddRange(CreateInstanceImagesChanges(instance, images));
            gitChanges.Add(await CreateSolutionFileChange(instance));
            gitChanges.Add(await CreateMicrositeFileChange(instance, brandSettings));
            gitChanges.Add(await CreateTemplateSetFileChange(instance));
            gitChanges.Add(await CreateProjectFileChange(instance, logoImage.Name, GetImageByType(images, ImageType.Splash).Name));
            gitChanges.Add(await CreateBrandSettingsChange(instance, images, brandSettings));

            GitRefUpdate gitRef = new()
            {
                Name = branchName,
                OldObjectId = masterCommitId
            };

            GitCommitRef gitCommit = new()
            {
                Comment = $"Created new template set for instance: {instance.Subdomain}",
                Changes = gitChanges
            };

            return new GitPush
            {
                RefUpdates = [gitRef],
                Commits = [gitCommit]
            };
        }
        catch (Exception ex)
        {
            throw new TemplateCreationException($"Failed to create template for instance '{instance.Subdomain}'", ex);
        }
    }

    public async Task<bool> UpdateInstanceTemplateStatusAsync(Instance instance, TemplateConfirmationState expextedState, TemplateConfirmationState newState)
    {
        EfInstance efInstance = await _context.Instances.Where(i => i.Id == instance.Id).FirstAsync();
        if (efInstance.TemplatesConfirmed != expextedState)
            return false;

        efInstance.TemplatesConfirmed = newState;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateInstanceDirectMailStatusAsync(Instance instance, TemplateConfirmationState expextedState, TemplateConfirmationState newState)
    {
        EfInstance efInstance = await _context.Instances.Where(i => i.Id == instance.Id).FirstAsync();
        if (efInstance.DirectMailConfirmed != expextedState)
            return false;

        efInstance.DirectMailConfirmed = newState;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<TemplateConfirmationState> GetInstanceDirectMailStatusAsync(Instance instance)
    {
        EfInstance efInstance = await _context.Instances.Where(i => i.Id == instance.Id).FirstAsync();

        return efInstance.DirectMailConfirmed;
    }

    public async Task<bool> UpdateInstanceLetterStatusAsync(Instance instance, TemplateConfirmationState expextedState, TemplateConfirmationState newState)
    {
        EfInstance efInstance = await _context.Instances.Where(i => i.Id == instance.Id).FirstAsync();
        if (efInstance.LetterheadConfirmed != expextedState)
            return false;

        efInstance.LetterheadConfirmed = newState;
        await _context.SaveChangesAsync();

        return true;
    }

    private static Image GetImageByType(IEnumerable<Image> images, ImageType type)
    {
        return images.FirstOrDefault(i => i.Type == type) ??
               throw new InvalidOperationException($"Image of type {type} not found");
    }

    private async Task<List<GitChange>> GetDefaultTemplateStructureAsync(Instance instance)
    {
        string directory = Path.Combine(_settings.Directory, "Template Definitions");
        string targetPath = $"{BaseTargetPath}/{instance.Domain}/{TemplateDefinitionsPath}";

        string[] files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
        List<GitChange> changes = [];

        foreach (string filePath in files)
        {
            string content = await File.ReadAllTextAsync(filePath);
            string relativePath = Path.GetRelativePath(directory, filePath).Replace('\\', '/');
            string fullTargetPath = $"{targetPath}/{relativePath}";

            changes.Add(CreateTextFileChange(fullTargetPath, content));
        }

        return changes;
    }

    private async Task<List<GitChange>> GetDefaultTemplateImagesAsync(Instance instance)
    {
        string directory = Path.Combine(_settings.Directory, "Images");
        string targetPath = $"{BaseTargetPath}/{instance.Domain}/{TemplateDefinitionsPath}/Images";

        string[] files = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
        List<GitChange> changes = [];

        foreach (string filePath in files)
        {
            byte[] content = await File.ReadAllBytesAsync(filePath);
            string relativePath = Path.GetRelativePath(directory, filePath).Replace('\\', '/');
            string fullTargetPath = $"{targetPath}/{relativePath}";

            changes.Add(CreateBinaryFileChange(fullTargetPath, content));
        }

        return changes;
    }

    private static List<GitChange> CreateInstanceImagesChanges(Instance instance, IEnumerable<Image> images)
    {
        string targetPath = $"{BaseTargetPath}/{instance.Domain}/{TemplateDefinitionsPath}/Images/Templates";

        return [.. images.Select(image =>
            CreateBinaryFileChange($"{targetPath}/{image.Name}", image.Data)
        )];
    }

    private async Task<GitChange> CreateSolutionFileChange(Instance instance)
    {
        string sourcePath = Path.Combine(_settings.Directory, "Solution Files", "Solution Template.sln");
        string targetPath = $"{BaseTargetPath}/{instance.Domain}/{instance.Subdomain}.sln";

        string content = await File.ReadAllTextAsync(sourcePath);
        return CreateTextFileChange(targetPath, content);
    }

    private async Task<GitChange> CreateMicrositeFileChange(Instance instance, IEnumerable<BrandSetting> brandSettings)
    {
        string sourcePath = Path.Combine(_settings.Directory, "Solution Files", "ResponsiveMicrosite03.master");
        string targetPath = $"{BaseTargetPath}/{instance.Domain}/{TemplateDefinitionsPath}/ResponsiveMicrosite03.master";

        string gaCode = brandSettings.Where(bs => bs.Setting == "GoogleAnalytics").FirstOrDefault()?.Value ?? "-";
        string website = brandSettings.Where(bs => bs.Setting == "bannerWebsite").FirstOrDefault()?.Value ?? "#";

        object templateContext = new
        {
            google_code = gaCode,
            customer_website = website,
            instance.Subdomain
        };

        string content = await RenderTemplateFileAsync(sourcePath, templateContext);
        return CreateTextFileChange(targetPath, content);
    }

    private async Task<GitChange> CreateTemplateSetFileChange(Instance instance)
    {
        string sourcePath = Path.Combine(_settings.Directory, "Solution Files", "TemplateSet.xml");
        string targetPath = $"{BaseTargetPath}/{instance.Domain}/{TemplateDefinitionsPath}/TemplateSet.xml";

        object templateContext = new { domain = instance.Domain };
        string content = await RenderTemplateFileAsync(sourcePath, templateContext);

        return CreateTextFileChange(targetPath, content);
    }

    private async Task<GitChange> CreateProjectFileChange(Instance instance, string logoName, string splashName)
    {
        string sourcePath = Path.Combine(_settings.Directory, "Solution Files", "Template Definitions.csproj");
        string targetPath = $"{BaseTargetPath}/{instance.Domain}/{TemplateDefinitionsPath}/Template Definitions.csproj";

        string aspxFiles = GetAspxFilesForInstanceType(InstanceTypes.Estate);
        object templateContext = new
        {
            aspx_pages = aspxFiles,
            image_logo = logoName,
            image_splash = splashName
        };

        string content = await RenderTemplateFileAsync(sourcePath, templateContext);
        return CreateTextFileChange(targetPath, content);
    }

    private static string GetAspxFilesForInstanceType(InstanceTypes instanceType) => instanceType switch
    {
        InstanceTypes.Estate => """
            <Content Include="Documents\Site\RequestAValuation.aspx" />
            <Content Include="Documents\Site\ValuationRequestComplete.aspx" />
            <Content Include="Documents\Site\ValuationRequestFailed.aspx" />
            <Content Include="Documents\Site\GetAQuoteComplete.aspx" />
            <Content Include="Documents\Site\GetAQuoteFailed.aspx" />
            <Content Include="Documents\Site\ContactUsComplete.aspx" />
            <Content Include="Documents\Site\ContactUsFailed.aspx" />
            """,

        InstanceTypes.Insurance => """
            <Content Include="Documents\Site\GetAQuote.aspx" />
            <Content Include="Documents\Site\ValuationRequestComplete.aspx" />
            <Content Include="Documents\Site\ValuationRequestFailed.aspx" />
            <Content Include="Documents\Site\GetAQuoteComplete.aspx" />
            <Content Include="Documents\Site\GetAQuoteFailed.aspx" />
            <Content Include="Documents\Site\ContactUsComplete.aspx" />
            <Content Include="Documents\Site\ContactUsFailed.aspx" />
            """,

        _ => throw new ArgumentOutOfRangeException(nameof(instanceType))
    };

    private async Task<GitChange> CreateBrandSettingsChange(Instance instance, IEnumerable<Image> images, IEnumerable<BrandSetting> brandSettings)
    {
        string targetPath = $"{BaseTargetPath}/{instance.Domain}/{TemplateDefinitionsPath}/BrandSettings.xml";
        IEnumerable<Branch> branches = await _onboardingDataService.GetSetupBranchesAsync(instance);
        IEnumerable<BrandSetting> otherBrandSettings = GetAdditionalBrandSettings(instance, brandSettings, images);

        List<BrandSetting> completeBrandSettings = brandSettings.ToList();
        completeBrandSettings.AddRange(otherBrandSettings);

        string xml = GenerateBrandSettingsXml(otherBrandSettings, branches);
        return CreateTextFileChange(targetPath, xml);
    }

    private static string GenerateBrandSettingsXml(IEnumerable<BrandSetting> defaultSettings, IEnumerable<Branch> currentBranches)
    {
        if (defaultSettings == null || !defaultSettings.Any())
        {
            throw new InvalidOperationException("No brand settings available to generate XML");
        }

        var (settings, branches) = ApplyAdditionalBrandSettings(defaultSettings, currentBranches);

        IEnumerable<XElement> settingElements = settings.OrderBy(s => s.Setting).Select(defaultSetting =>
        {
            XElement settingElement = new("Setting",
                new XAttribute("Name", defaultSetting.Setting),
                new XElement("DefaultValue", defaultSetting.Value ?? string.Empty)
            );

            foreach (Branch branch in branches)
            {
                BrandSetting? branchOverride = branch.Settings.FirstOrDefault(s => s.Setting == defaultSetting.Setting);

                if (branchOverride != null)
                {
                    settingElement.Add(new XElement("Value",
                        new XAttribute("Brand", branch.Name),
                        branchOverride.Value ?? string.Empty));
                }
            }

            return settingElement;
        });

        XElement root = new("BrandSettings", settingElements);
        XDocument document = new(
            new XDeclaration("1.0", "utf-8", null),
            root
        );

        using MemoryStream ms = new();
        using (XmlWriter xmlWriter = XmlWriter.Create(ms, new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = Environment.NewLine,
            NewLineHandling = NewLineHandling.Replace,
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        }))
        {
            document.Save(xmlWriter);
        }

        string xmlString = Encoding.UTF8.GetString(ms.ToArray());
        return xmlString;
    }

    private static async Task<string> RenderTemplateFileAsync(string filePath, object context)
    {
        string templateContent = await File.ReadAllTextAsync(filePath);
        Scriban.Template template = Scriban.Template.Parse(templateContent);
        return template.Render(context);
    }

    private static GitChange CreateTextFileChange(string path, string content) => new()
    {
        ChangeType = VersionControlChangeType.Add,
        Item = new GitItem { Path = path },
        NewContent = new ItemContent
        {
            Content = content,
            ContentType = ItemContentType.RawText
        }
    };

    private static GitChange CreateBinaryFileChange(string path, byte[] content) => new()
    {
        ChangeType = VersionControlChangeType.Add,
        Item = new GitItem { Path = path },
        NewContent = new ItemContent
        {
            Content = Convert.ToBase64String(content),
            ContentType = ItemContentType.Base64Encoded
        }
    };

    private static List<BrandSetting> GetAdditionalBrandSettings(Instance instance, IEnumerable<BrandSetting> brandSettings, IEnumerable<Image> image)
    {
        List<BrandSetting> currentSettings = brandSettings.ToList();
        BrandSetting primaryColour = currentSettings.Where(cs => cs.Setting == "theme.colourPrimary").First();

        currentSettings.Add(CreateBrandSetting("fontFamily", "Arial"));
        currentSettings.Add(CreateBrandSetting("fontSize", "12px"));
        currentSettings.Add(CreateBrandSetting("theme.valuationLink", $"https://{instance.Domain}/ContactForm.aspx?ContactFormId=1"));

        if (!brandSettings.Where(s => s.Setting == "theme.digivalLink").Any())
            currentSettings.Add(CreateBrandSetting("theme.digivalLink", $"https://{instance.Domain}/Valuation"));

        currentSettings.Add(CreateBrandSetting("ezineURL", "http://ezines.briefyourmarket.com/campaign/[idhere]"));
        currentSettings.Add(CreateBrandSetting("aspxLink", $"https://{instance.Domain}/ContactForm.aspx?ContactFormId=1"));
        currentSettings.Add(CreateBrandSetting("aspxLinkText", "Request Valuation"));
        currentSettings.Add(CreateBrandSetting("categoryUpdateRequestText", "Get only what you need"));

        Image logo = image.Any(i => i.Type == ImageType.logoAlternate)
                        ? GetImageByType(image, ImageType.logoAlternate)
                        : GetImageByType(image, ImageType.Logo);

        Image splash = image.Where(i => i.Type == ImageType.Splash).First();

        currentSettings.Add(CreateBrandSetting("banner", $"https://{instance.Domain}/Images/Templates/{logo.Name}"));
        currentSettings.Add(CreateBrandSetting("surveyHeader", $"https://{instance.Domain}/Images/Templates/{logo.Name}"));
        currentSettings.Add(CreateBrandSetting("micrositeLogo", $"https://{instance.Domain}/Images/Templates/{logo.Name}"));
        currentSettings.Add(CreateBrandSetting("logoWidth", logo.Width.ToString()));
        currentSettings.Add(CreateBrandSetting("splashImage", $"https://{instance.Domain}/Images/Templates/{splash.Name}"));
        currentSettings.Add(CreateBrandSetting("splashImageShow", "2"));
        currentSettings.Add(CreateBrandSetting("splashWidth", splash.Width.ToString()));
        currentSettings.Add(CreateBrandSetting("splashHeight", splash.Height.ToString()));
        currentSettings.Add(CreateBrandSetting("pointerImage", $"https://{instance.Domain}/global/images/Templates/BYMLogos/chevron2018v2.png"));
        currentSettings.Add(CreateBrandSetting("survey1Background", $"url(https://{instance.Domain}/Images/Templates/Survey/bgImage.jpg)"));

        currentSettings.Add(CreateBrandSetting("mainBackground", "#ffffff"));
        currentSettings.Add(CreateBrandSetting("backgroundColour", "#f2f2f2"));
        currentSettings.Add(CreateBrandSetting("backgroundTextColour", "#333333"));
        currentSettings.Add(CreateBrandSetting("categoryHeaderColour", "#ffffff"));
        currentSettings.Add(CreateBrandSetting("categoryTextColour", "#ffffff"));
        currentSettings.Add(CreateBrandSetting("categoryUpdateTextColour", "#ffffff"));
        currentSettings.Add(CreateBrandSetting("categoryTickColour", "#ffffff"));
        currentSettings.Add(CreateBrandSetting("surveyBtnText", "#ffffff"));
        currentSettings.Add(CreateBrandSetting("surveyBtnTextHover", "#ffffff"));
        currentSettings.Add(CreateBrandSetting("survey1BgColour", "(243,246,243,0.9)"));
        currentSettings.Add(CreateBrandSetting("survey2Background", "#ffffff"));
        currentSettings.Add(CreateBrandSetting("survey2BgColour", "#222222"));
        currentSettings.Add(CreateBrandSetting("survey3Background", "#ffffff"));
        currentSettings.Add(CreateBrandSetting("survey3BoxShadow", "4px 4px 10px rgba(51, 51, 51, .7)"));
        currentSettings.Add(CreateBrandSetting("micrositeBtnText", "#ffffff"));
        currentSettings.Add(CreateBrandSetting("micrositeBtnTextHover", "#ffffff"));

        currentSettings.Add(CreateBrandSetting("articleMainStyles", $"<![CDATA[color: {primaryColour.Value}; font-family: Arial]]>"));
        currentSettings.Add(CreateBrandSetting("articleTitleStyles", $"<![CDATA[font-family:Arial, sans-serif;color:{primaryColour.Value};font-weight:bold;padding-top:10px;font-size:16px]]>"));
        currentSettings.Add(CreateBrandSetting("articleImageStyles", "<![CDATA[vertical-align:top]]>"));
        currentSettings.Add(CreateBrandSetting("articleTeaserStyles", "<![CDATA[font-family:Arial;font-size:12px;vertical-align:top]]>"));
        currentSettings.Add(CreateBrandSetting("articleMoreLinkContainerStyles", "<![CDATA[font-family:Arial]]>"));
        currentSettings.Add(CreateBrandSetting("articleMoreLinkStyles", $"<![CDATA[text-decoration:none;color:{primaryColour.Value}]]>"));
        currentSettings.Add(CreateBrandSetting("articleTitleLinkStyles", $"<![CDATA[font-family:Arial;color:{primaryColour.Value};font-weight:bold;font-size:16px;text-decoration:none]]>"));

        List<string> socialPlatforms = new()
        {
            "Twitter",
            "Facebook",
            "Linkedin",
            "Youtube",
            "Instagram"
        };


        foreach (string platform in socialPlatforms)
        {
            string settingKey = $"{platform}URL";
            BrandSetting? linkSetting = currentSettings.FirstOrDefault(cs => cs.Setting == settingKey);

            if (linkSetting == null)
            {
                currentSettings.Add(CreateBrandSetting(settingKey, "#"));
                currentSettings.Add(CreateBrandSetting($"{platform}Show", "1"));
            }
            else
            {
                currentSettings.Add(CreateBrandSetting($"{platform}Show", "2"));
            }
        }

        currentSettings.Add(CreateBrandSetting("YomdelButtonImg", "#"));
        currentSettings.Add(CreateBrandSetting("YomdelChatLink", "#"));

        return currentSettings;
    }

    private static BrandSetting CreateBrandSetting(string name, string value)
    {
        return new()
        {
            Setting = name,
            Value = value
        };
    }

    private static (IEnumerable<BrandSetting> settings, IEnumerable<Branch> branches) ApplyAdditionalBrandSettings(IEnumerable<BrandSetting> defaultSettings, IEnumerable<Branch> currentBranches)
    {
        List<BrandSetting> settings = defaultSettings.ToList();
        List<Branch> branches = currentBranches.ToList();

        foreach (var kvp in _additionalSettingsToCopy)
        {
            BrandSetting? globalSetting = settings.FirstOrDefault(s => s.Setting == kvp.Value);
            if (globalSetting != null)
                settings.Add(CreateBrandSetting(kvp.Key, globalSetting.Value));

            foreach (Branch branch in branches)
            {
                List<BrandSetting> branchSettings = branch.Settings?.ToList() ?? new List<BrandSetting>();
                BrandSetting? branchSettingToCopy = branchSettings.FirstOrDefault(s => s.Setting == kvp.Value);
                if (branchSettingToCopy != null)
                {
                    branchSettings.Add(CreateBrandSetting(kvp.Key, branchSettingToCopy.Value));
                    branch.Settings = branchSettings;
                }
            }
        }

        Branch defaultBranch = branches.Where(b => b.Default).First();

        foreach (var kvp in _branchConfigAsSettings)
        {
            string value = GetValueFromBranch(defaultBranch, defaultSettings, kvp.Value, true);

            settings.Add(CreateBrandSetting(kvp.Key, value));
            
            foreach (Branch branch in branches)
            {
                List<BrandSetting> branchSettings = branch.Settings?.ToList() ?? new List<BrandSetting>();
                string branchSpecificValue = GetValueFromBranch(branch, defaultSettings, kvp.Value, false);

                branchSettings.Add(CreateBrandSetting(kvp.Key, branchSpecificValue));
                branch.Settings = branchSettings;
            }
        }

        return (settings, branches);
    }

    private static string GetValueFromBranch(Branch branch, IEnumerable<BrandSetting> defaultSettings, string type, bool isDefault)
    {
        string value = "";

        if (type == "email")
            value = branch.SalesEmail;
        else if (type == "phone")
            value = branch.DomesticNumber;
        else if (type == "address")
        {
            List<string> parts = new()
            {
                branch.Address1,
                branch.Address2,
                branch.Address3,
                branch.Town,
                $"{branch.PostCode1} {branch.PostCode2}"
            };

            value = string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }
        else if (type == "address-split")
        {
            List<string> parts = new()
            {
                branch.Address1,
                branch.Address2,
                branch.Address3,
                branch.Town,
                $"{branch.PostCode1} {branch.PostCode2}"
            };

            value = string.Join(",<br/>", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }
        else if (type == "name")
        {
            string companyName = string.Empty;

            if (!isDefault)
            {
                companyName = branch.Settings.Where(s => s.Setting == "companyName").FirstOrDefault()?.Value ?? "";

                if (string.IsNullOrEmpty(companyName))
                    companyName = defaultSettings.Where(s => s.Setting == "companyName").FirstOrDefault()?.Value ?? "";

                value = string.Format("{0} {1}", companyName, branch.Name);
            }
            else
            {
                value = defaultSettings.Where(s => s.Setting == "companyName").FirstOrDefault()?.Value ?? "";
            }
        }

        return value;
    }
}
