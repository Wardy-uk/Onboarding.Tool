using Dapper;
using System.Data.Common;
using Onboarding.Tool.Data;
using Microsoft.EntityFrameworkCore;
using Onboarding.Tool.Model.BriefYourMarket.Brands;
using Onboarding.Tool.Model.Dashboard.Branches;
using Onboarding.Tool.Model.Dashboard.Users;
using Onboarding.Tool.Model.BuildYourMarket;
using PortalAccount = Onboarding.Tool.Model.BuildYourMarket.PortalAccount;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;
using Branch = Onboarding.Tool.Model.BriefYourMarket.Branches.Branch;
using EfBranch = Onboarding.Tool.Model.Data.Branch;
using Image = Onboarding.Tool.Model.BriefYourMarket.Images.Image;
using EfInstance = Onboarding.Tool.Model.Data.Instance;
using EfPortalAccount = Onboarding.Tool.Model.Data.PortalAccount;
using EfImage = Onboarding.Tool.Model.Data.Image;
using Onboarding.Tool.Model.Dashboard;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Model.Dashboard.Settings;
using Onboarding.Tool.Server.Helpers.Dashboard;
using Onboarding.Tool.Model.Dashboard.Setups;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using System.Xml.Linq;
using ImageType = Onboarding.Tool.Model.Enums.BriefYourMarket.Images.ImageType;
using iText.Layout;
using Serilog;
using iText.Layout.Properties;

namespace Onboarding.Tool.Server.Services.Onboarding;

public class OnboardingDataService : IOnboardingDataService
{
    private readonly IOnboardingSqlConnection _onboardingSqlConnection;
    private readonly DashboardSettingsHelper _dashboardSettingsHelper;
    private readonly AppDbContext _context;

    public OnboardingDataService(
        IOnboardingSqlConnection onboardingSqlConnection,
        DashboardSettingsHelper dashboardSettingsHelper,
        AppDbContext context)
    {
        _onboardingSqlConnection = onboardingSqlConnection;
        _dashboardSettingsHelper = dashboardSettingsHelper;
        _context = context;
    }

    public async Task<IEnumerable<BrandSetting>> GetSetupInstanceSettingsAsync(Instance instance)
    {
        using DbConnection connection = _onboardingSqlConnection.CreateOnboardingSqlConnection();
        const string sql = """
            SELECT [InstanceSettings].[Setting],
                   [InstanceSettings].[Value]
            FROM [InstanceSettings] (NOLOCK)
            WHERE [InstanceId] = @InstanceId
            """;

        return await connection.QueryAsync<BrandSetting>(sql, new
        {
            InstanceId = instance.Id
        });
    }

    public async Task<IEnumerable<PortalAccount>> GetSetupInstancePortalAccountsAsync(Instance instance)
    {
        using DbConnection connection = _onboardingSqlConnection.CreateOnboardingSqlConnection();
        const string sql = """
            SELECT [PortalName] [Portal]
            FROM [PortalAccount] (NOLOCK)
            WHERE [InstanceId] = @InstanceId
            """;

        return await connection.QueryAsync<PortalAccount>(sql, new
        {
            InstanceId = instance.Id
        });
    }

    public async Task<IEnumerable<Image>> GetSetupImagesAsync(Instance instance)
    {
        using DbConnection connection = _onboardingSqlConnection.CreateOnboardingSqlConnection();
        const string sql = """
            WITH [RankedImages] AS (
                SELECT *, 
                       ROW_NUMBER() OVER (PARTITION BY Type ORDER BY [Id] DESC) AS rn
                FROM [Image] (NOLOCK)
                WHERE [InstanceId] = @InstanceId
                AND [Type] IN (0, 1, 2, 3, 4)
            )
            SELECT [Name],
                   [Data] AS [Data],
                   [Type],
                   [CardWidthOverride] AS [LogoWidthOverride],
                   [CardHeightOverride] AS [LogoHeightOverride],
                   [CardXOverride] AS [LogoXOverride],
                   [CardYOverride] AS [LogoYOverride]
            FROM [RankedImages]
            WHERE rn = 1;
            """;

        return await connection.QueryAsync<Image>(sql, new
        {
            InstanceId = instance.Id
        });
    }

    public async Task<IEnumerable<string>> GetSetupUserEmailsAsync(Instance instance)
    {
        using DbConnection connection = _onboardingSqlConnection.CreateOnboardingSqlConnection();
        const string sql = """
            SELECT [Username]
            FROM [Users] (NOLOCK)
            WHERE [InstanceId] = @InstanceId
            """;

        return await connection.QueryAsync<string>(sql, new
        {
            InstanceId = instance.Id
        });
    }

    public async Task<IEnumerable<Branch>> GetSetupBranchesAsync(Instance instance)
    {
        using DbConnection connection = _onboardingSqlConnection.CreateOnboardingSqlConnection();
        const string sql = """
            SELECT [Branches].[Id],
                   [Branches].[IsDefault] [Default],
                   [Branches].[Name],
                   ISNULL([Address].[Address1], '') [Address1],
                   ISNULL([Address].[Address2], '') [Address2],
                   ISNULL([Address].[Address3], '') [Address3],
                   ISNULL([Address].[Town], '') [Town],
                   ISNULL([Address].[PostCode1], '') [PostCode1],
                   ISNULL([Address].[PostCode2], '') [PostCode2],
                   [Branches].[SalesEmail],
                   [Branches].[SalesPhone] [SalesPhoneNumber],
                   [Branches].[LettingsEmail],
                   [Branches].[LettingsPhone] [LettingsPhoneNumber]
            FROM [Branches] (NOLOCK)
            LEFT JOIN [Address] (NOLOCK) ON [Branches].[AddressId] = [Address].[Id]
            WHERE [InstanceId] = @InstanceId
            """;

        IEnumerable<Branch> branches = await connection.QueryAsync<Branch>(sql, new
        {
            InstanceId = instance.Id
        });

        foreach(Branch branch in branches)
        {
            branch.Settings = await GetSetupBranchSettingsAsync(branch);
        }

        return branches;
    }

    public async Task<IEnumerable<BrandSetting>> GetSetupBranchSettingsAsync(Branch branch)
    {
        using DbConnection connection = _onboardingSqlConnection.CreateOnboardingSqlConnection();
        const string sql = """
            SELECT [BranchSetting].[Setting],
                   [BranchSetting].[Value]
            FROM [BranchSetting] (NOLOCK)
            WHERE [BranchId] = @BranchId
            """;

        return await connection.QueryAsync<BrandSetting>(sql, new
        {
            BranchId = branch.Id
        });
    }

    public async Task UpdateBranchDefaultStatusAsync(SaveBranch saveBranch, Instance instance)
    {
        if (saveBranch.IsDefault)
        {
            using DbConnection connection = _onboardingSqlConnection.CreateOnboardingSqlConnection();
            const string sql = """
                UPDATE [Branches] SET [IsDefault] = 0 WHERE [InstanceId] = @InstanceId AND [Id] != @BranchId
                """;

            await connection.ExecuteAsync(sql, new { InstanceId = instance.Id, BranchId = saveBranch.Id });
        }
    }

    public async Task<bool> DoesUserExistAsync(ImportUser user, Instance instance)
    {
        using DbConnection connection = _onboardingSqlConnection.CreateOnboardingSqlConnection();
        const string sql = """
            SELECT COUNT(*) FROM [Users] (NOLOCK) WHERE [InstanceId] = @InstanceId AND [Username] = @Username 
            """;

        int result = await connection.ExecuteScalarAsync<int>(sql, new { InstanceId = instance.Id, Username = user.Email });

        return result > 0;
    }

    public async Task<EfInstance?> GetOrCreateInstanceAsync(int instanceId)
    {
        EfInstance? instance = await _context.Set<EfInstance>().FirstOrDefaultAsync(i => i.Id == instanceId);

        if (instance != null)
            return null;

        instance = new EfInstance { Id = instanceId };
        _context.Instances.Add(instance);
            
        await _context.SaveChangesAsync();

        return instance;
    }

    public async Task<OverviewDto> GetInstanceOverviewAsync(Instance instance)
    {   
        List<EfBranch> branches = await _context.Set<EfBranch>().Where(b => b.InstanceId == instance.Id).ToListAsync();
        bool hasDefaultBranch = branches.Any(b => b.IsDefault);
        int branchCount = branches.Count;
        int users = await _context.Set<Users>().Where(u => u.InstanceId == instance.Id).CountAsync();

        IEnumerable<InstanceSetting> settings = await _context.Set<InstanceSetting>().Where(iss => iss.InstanceId == instance.Id).ToListAsync();
        bool hasRequiredSettings = _dashboardSettingsHelper.BrandSettings
            .Where(cfg => cfg.Required)
            .All(cfg => settings.Any(s => s.Setting == cfg.Key));

        List<EfImage> images = await _context.Set<EfImage>().Where(i => i.InstanceId == instance.Id &&
                    (i.Type == ImageType.Logo || i.Type == ImageType.Splash)).ToListAsync();

        return new()
        {
            Branches = !hasDefaultBranch ? 0 : branchCount,
            Users = users,
            RequiredBrandSettings = hasRequiredSettings,
            RequiredImages = images.Count >= 2
        };
    }

    public async Task<List<PostCodeDistrict>> GetBranchDistrictsByNameAsync(Instance instance, string name)
    {
        List<PostCodeDistrict> postCodeDistricts = new();

        EfBranch? branch = await _context.Branches
            .Include(b => b.Districts)
                .ThenInclude(d => d.Sectors)
            .Where(b => b.Name == name)
            .FirstOrDefaultAsync();

        if (branch == null)
            return postCodeDistricts;

        postCodeDistricts = branch.Districts
        .Select(d => new PostCodeDistrict
        {
            OutwardCode = d.District.ToUpper(),
            Description = d.District.ToUpper(),
            Sectors = d.Sectors?
                .Where(s => int.TryParse(s.Sector, out _))
                .Select(s => s.Sector)
                .ToList() ?? new List<string>(),

            AllSectors = d.AllSectors
        })
        .ToList();


        return postCodeDistricts;
    }

    public async Task<List<BranchDto>> GetBranchOverviewAsync(Instance instance)
    {
        List<EfBranch> branches = await _context.Set<EfBranch>().Include(b => b.Address).Where(b => b.InstanceId == instance.Id).ToListAsync();

        return branches.Select((b) => new BranchDto
        {
            Id = b.Id,
            Name = b.Name,
            IsDefault = b.IsDefault,
            SalesEmail = b.SalesEmail,
            SalesPhone = b.SalesPhone,
            LettingsEmail = b.LettingsEmail,
            LettingsPhone = b.LettingsPhone,
            Address = b.Address ?? null
        }).ToList();
    }

    public async Task<BuildOverview> GetBuildOverviewAsync(Instance instance)
    {
        List<EfBranch> branches = await _context.Set<EfBranch>()
            .Include(b => b.Districts)
                .ThenInclude(d => d.Sectors)
            .Where(b => b.InstanceId == instance.Id).ToListAsync();

        List<EfPortalAccount> portalAccounts = await _context.Set<EfPortalAccount>().Where(p => p.InstanceId == instance.Id).ToListAsync();

        return new BuildOverview()
        {
            Branches = branches.Select(b => new BuildBranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Districts = b.Districts.Select(d => new BuildDistrictDto
                {
                    DistrictId = d.Id,
                    District = d.District,
                    AllSectors = d.AllSectors,
                    Sectors = d.Sectors?.Select(s => s.Sector).ToList() ?? []
                }).ToList()
            }).ToList(),
            PortalAccounts = portalAccounts
        };
    }

    public async Task<List<Users>> GetUsersAsync(Instance instance)
    {
        List<Users> users = await _context.Set<Users>().Where(u => u.InstanceId == instance.Id).ToListAsync();
        return users;
    }

    public List<BrandSettingConfig> GetBrandSettingConfig()
    {
        return _dashboardSettingsHelper.BrandSettings;
    }

    public async Task<SetupStateDto> GetSetupStateAsync(Instance instance)
    {
        EfInstance efInstance = await _context.Set<EfInstance>().Where(i => i.Id == instance.Id).FirstAsync();
        List<InstanceSetupStep> instanceSetupSteps = await _context.Set<InstanceSetupStep>().Where(iss => iss.InstanceId == instance.Id).ToListAsync();

        return new SetupStateDto
        {
            TemplateSteps = new()
            {
                TemplatesConfirmed = efInstance.TemplatesConfirmed,
                LetterheadConfirmed = efInstance.LetterheadConfirmed,
                DirectMailConfirmed = efInstance.DirectMailConfirmed
            },
            AdditionalSteps = instanceSetupSteps.Select(iss => new SetupAdditionalState
            {
                SetupStep = iss.SetupStep,
                State = iss.Status
            }).ToList()
        };
    }

    public async Task<CardPreviewInfo> GetCardPreviewInfoAsync(Instance instance)
    {
        EfImage? logo = await _context.Set<EfImage>()
            .Where(i => i.InstanceId == instance.Id &&
                       (i.Type == ImageType.PrintLogoAlternate || i.Type == ImageType.PrintLogo))
            .OrderBy(i => i.Type == ImageType.PrintLogoAlternate ? 0 : 1)
            .FirstOrDefaultAsync();

        if (logo == null)
            throw new InvalidOperationException("No card logo found.");

        Image image = new()
        {
            Name = logo.Name,
            Data = logo.Data,
            Type = logo.Type,
            LogoHeightOverride = logo.CardHeightOverride,
            LogoWidthOverride = logo.CardWidthOverride,
            LogoXOverride = logo.CardXOverride,
            LogoYOverride = logo.CardYOverride
        };

        return new()
        {
            Height = (int)image.CardLogoHeight,
            Width = (int)image.CardLogoWidth,
            X = (int)image.CardLogoX,
            Y = (int)image.CardLogoY
        };
    }

    public async Task<byte[]> GetPreviewPdfAsync(Instance instance, int? width = null, int? height = null, int? x = null, int? y = null)
    {
        EfImage? logo = await _context.Set<EfImage>()
            .Where(i => i.InstanceId == instance.Id &&
                    (i.Type == ImageType.PrintLogoAlternate || i.Type == ImageType.PrintLogo))
            .OrderBy(i => i.Type == ImageType.PrintLogoAlternate ? 0 : 1)
            .FirstOrDefaultAsync();

        if (logo == null)
            return [];

        Image image = new()
        {
            Name = logo.Name,
            Data = logo.Data,
            Type = logo.Type,
            LogoHeightOverride = logo.CardHeightOverride,
            LogoWidthOverride = logo.CardWidthOverride,
            LogoXOverride = logo.CardXOverride,
            LogoYOverride = logo.CardYOverride
        };

        string xmlString = """
            <canvas name="Page 1" width="220" height="158" mediabox="0,0,220,158" cropbox="5,5,210,148" bleed="0" resourceFilePath="" backgroundColour="" backgroundColourCMYK="" backgroundImagePath="/global/images/DirectMail/FreshontheMarket/ZestPropertyFOTM/PageBackgroundCompiled1T637375089627109959.png"> <segment id="1" pdfReferences="23|13|0" left="0" top="0" width="220" height="158" crop="false" editable="true" merged="false" logo="false" deleted="false"> <velocityImage left="0" top="0" width="220" height="158" changed="false" logo="false">$!{Product.MainImage.RelativeUrl}</velocityImage> </segment> <segment id="3" pdfReferences="367|358|0" left="0" top="120.4" width="220" height="37.6" crop="false" editable="true" merged="false" logo="false" deleted="false"> <image left="0" top="0" width="220" height="37.6" changed="false" logo="false">/global/images/DirectMail/{{ instance }}/colourblockprimary.tif</image> </segment> <segment id="7" pdfReferences="0|358|1094514051,0|358|3099197" left="15" top="130.5" width="90" height="12" crop="false" editable="true" merged="false" logo="false" deleted="false"> </segment> <segment id="4" pdfReferences="376|370|0" left="{{ logo.card_logo_x }}" top="{{ logo.card_logo_y }}" width="{{ logo.card_logo_width }}" height="{{ logo.card_logo_height }}" crop="false" editable="true" merged="false" logo="false" deleted="false"> <image left="0" top="0" width="{{ logo.card_logo_width }}" height="{{ logo.card_logo_height }}" changed="false" logo="false">data:image/tiff;base64,{{ logo }}</image> </segment> </canvas>
            """;

        xmlString = xmlString.Replace("{{ logo.card_logo_x }}",
            (x ?? image.CardLogoX).ToString());

        xmlString = xmlString.Replace("{{ logo.card_logo_y }}",
            (y ?? image.CardLogoY).ToString());

        xmlString = xmlString.Replace("{{ logo.card_logo_width }}",
            (width ?? image.CardLogoWidth).ToString());

        xmlString = xmlString.Replace("{{ logo.card_logo_height }}",
            (height ?? image.CardLogoHeight).ToString());

        xmlString = xmlString.Replace("{{ instance }}", "bym2013");
        xmlString = xmlString.Replace("{{ logo }}", Convert.ToBase64String(image.Data));

        return RenderPdfPreview(xmlString);
    }

    private static byte[] RenderPdfPreview(string xmlString)
    {
        const float MM_TO_PT = 72f / 25.4f;

        var xml = XDocument.Parse(xmlString);
        var canvasElement = xml.Root;

        byte[] pdfBytes;

        using (var ms = new MemoryStream())
        using (var writer = new PdfWriter(ms))
        using (var pdf = new PdfDocument(writer))
        {
            float widthPt = float.Parse(canvasElement.Attribute("width")?.Value ?? "210") * MM_TO_PT;
            float heightPt = float.Parse(canvasElement.Attribute("height")?.Value ?? "297") * MM_TO_PT;

            var pageSize = new PageSize(widthPt, heightPt);
            pdf.AddNewPage(pageSize);

            var doc = new Document(pdf, pageSize);
            doc.SetMargins(0, 0, 0, 0);

            int processedSegments = 0;

            foreach (var segment in canvasElement.Elements("segment"))
            {
                try
                {
                    bool wasProcessed = ProcessSegment(segment, doc, heightPt, MM_TO_PT);
                    if (wasProcessed) processedSegments++;
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to process segment {segment.Attribute("id")?.Value}: {ex.Message}");
                }
            }

            if (processedSegments == 0)
            {
                var testParagraph = new Paragraph("TEST: PDF Generated Successfully")
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
                    .SetFontSize(12)
                    .SetFixedPosition(10, heightPt - 30, 200);
                doc.Add(testParagraph);
            }

            doc.Close();
            pdfBytes = ms.ToArray();
        }

        return pdfBytes;
    }

    private static bool ProcessSegment(XElement segment, Document doc, float pageHeightPt, float mmToPt)
    {
        float segLeft = float.Parse(segment.Attribute("left")?.Value ?? "0") * mmToPt;
        float segTop = float.Parse(segment.Attribute("top")?.Value ?? "0") * mmToPt;
        float segWidth = float.Parse(segment.Attribute("width")?.Value ?? "0") * mmToPt;
        float segHeight = float.Parse(segment.Attribute("height")?.Value ?? "0") * mmToPt;

        bool processed = false;

        var textElement = segment.Element("text");
        if (textElement != null)
        {
            ProcessTextElement(textElement, doc, segLeft, segTop, segWidth, segHeight, pageHeightPt, mmToPt);
            processed = true;
        }

        var imageElement = segment.Element("image") ?? segment.Element("velocityImage");
        if (imageElement != null)
        {
            bool imageProcessed = ProcessImageElement(imageElement, doc, segLeft, segTop, segWidth, segHeight, pageHeightPt);
            if (!imageProcessed)
            {
                CreateImagePlaceholder(doc, segLeft, segTop, segWidth, segHeight, pageHeightPt, imageElement.Value);
            }
            processed = true;
        }

        return processed;
    }

    private static void ProcessTextElement(XElement textElement, Document doc, float segLeft, float segTop, float segWidth, float segHeight, float pageHeightPt, float mmToPt)
    {
        string content = textElement.Value?.Trim();
        if (string.IsNullOrEmpty(content)) return;

        float fontSize = float.Parse(textElement.Attribute("size")?.Value ?? "12") * mmToPt;
        string fontName = textElement.Attribute("font")?.Value ?? "Helvetica";
        string rgbValue = textElement.Attribute("rgb")?.Value ?? "0,0,0";
        string alignment = textElement.Attribute("align")?.Value ?? "left";

        var rgbParts = rgbValue.Split(',');
        Color textColor = ColorConstants.BLACK;
        if (rgbParts.Length == 3 &&
            int.TryParse(rgbParts[0], out int r) &&
            int.TryParse(rgbParts[1], out int g) &&
            int.TryParse(rgbParts[2], out int b))
        {
            textColor = new DeviceRgb(r / 255f, g / 255f, b / 255f);
        }

        PdfFont font = fontName.ToLower() switch
        {
            var n when n.Contains("bold") => PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD),
            var n when n.Contains("italic") => PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE),
            var n when n.Contains("times") => PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN),
            var n when n.Contains("courier") => PdfFontFactory.CreateFont(StandardFonts.COURIER),
            _ => PdfFontFactory.CreateFont(StandardFonts.HELVETICA)
        };

        float pdfY = pageHeightPt - segTop - fontSize;

        var paragraph = new Paragraph(content)
            .SetFont(font)
            .SetFontSize(fontSize)
            .SetFontColor(textColor)
            .SetMargin(0)
            .SetPadding(0)
            .SetTextAlignment(alignment.ToLower() switch
            {
                "center" => TextAlignment.CENTER,
                "right" => TextAlignment.RIGHT,
                "justify" => TextAlignment.JUSTIFIED,
                _ => TextAlignment.LEFT
            });

        paragraph.SetFixedPosition(segLeft, pdfY, segWidth > 0 ? segWidth : 1000);

        doc.Add(paragraph);
    }

    private static bool ProcessImageElement(XElement imageElement, Document doc, float segLeft, float segTop, float segWidth, float segHeight, float pageHeightPt)
    {
        string imageValue = imageElement.Value?.Trim();
        if (string.IsNullOrEmpty(imageValue) || imageValue.StartsWith("$!{"))
        {
            return false;
        }

        byte[] imageBytes = null;

        try
        {
            if (imageValue.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
            {
                var base64Data = imageValue.Substring(imageValue.IndexOf(',') + 1);
                imageBytes = Convert.FromBase64String(base64Data);
            }
            else if (File.Exists(imageValue))
            {
                imageBytes = File.ReadAllBytes(imageValue);
            }
            else
            {
                return false;
            }

            var imgData = ImageDataFactory.Create(imageBytes);
            var img = new iText.Layout.Element.Image(imgData);

            img.ScaleAbsolute(segWidth, segHeight);
            float pdfY = pageHeightPt - segTop - segHeight;
            img.SetFixedPosition(segLeft, pdfY);

            doc.Add(img);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    private static void CreateImagePlaceholder(Document doc, float segLeft, float segTop, float segWidth, float segHeight, float pageHeightPt, string imagePath)
    {
        float pdfY = pageHeightPt - segTop - segHeight;

        var placeholderDiv = new Div()
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetBorder(new iText.Layout.Borders.SolidBorder(ColorConstants.GRAY, 1))
            .SetWidth(segWidth)
            .SetHeight(segHeight)
            .SetFixedPosition(segLeft, pdfY, segWidth);

        string fileName = System.IO.Path.GetFileName(imagePath ?? "unknown");
        var placeholderText = new Paragraph($"[{fileName}]")
            .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA))
            .SetFontSize(Math.Min(10, segHeight / 3))
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMargin(2);

        placeholderDiv.Add(placeholderText);
        doc.Add(placeholderDiv);
    }
}
