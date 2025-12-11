using Microsoft.Extensions.Options;
using Onboarding.Tool.Model.BriefYourMarket.Images;
using Onboarding.Tool.Model.BriefYourMarket.Instances.ApiModels;
using Onboarding.Tool.Model.Enums.BriefYourMarket.Images;
using Onboarding.Tool.Server.Helpers.Files;
using System.Text.Json;
using InstanceModel = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Instances.Letter;

public class InstanceLetterService : IInstanceLetterService
{
    private readonly IHttpHelper _httpHelper;
    private readonly IOnboardingDataService _onboardingDataService;
    private readonly BriefYourMarketInstanceHelper _briefYourMarketInstanceHelper;
    private readonly InstanceSettings _instanceSettings;

    public InstanceLetterService(
        IHttpHelper httpHelper,
        IOnboardingDataService onboardingDataService,
        IOptions<InstanceSettings> instanceSettings, 
        BriefYourMarketInstanceHelper briefYourMarketInstanceHelper)
    {
        _httpHelper = httpHelper;
        _onboardingDataService = onboardingDataService;
        _instanceSettings = instanceSettings.Value;
        _briefYourMarketInstanceHelper = briefYourMarketInstanceHelper;
    }

    public async Task<LetterHeadResult> CreateDefaultLetterHeadAsync(InstanceModel instance)
    {
        Image? logo = await GetLogoImageAsync(instance, ImageType.PrintLogo) ?? throw new InvalidOperationException("The instance does not have a logo uploaded.");
        Image? altLogo = await GetLogoImageAsync(instance, ImageType.PrintLogoAlternate);

        byte[] resizedLogo = await ImageHelper.ResizeImageAsync(logo.Data, 125);
        byte[] letter = PdfHelper.GenerateLetterheadPdf(resizedLogo);

        byte[] altLetter = [];
        if (altLogo != null)
        {
            byte[] resizedAltLogo = await ImageHelper.ResizeImageAsync(altLogo.Data, 125);
            altLetter = PdfHelper.GenerateLetterheadPdf(resizedAltLogo);
        }

        try
        {
            if (altLetter.Length > 0)
            {
                await UploadLetterHeadAsync(instance, altLetter, "default_alt.pdf");
            }

            return await UploadLetterHeadAsync(instance, letter, "default.pdf");
        }
        catch
        {
            throw;
        }
    }

    private async Task<Image?> GetLogoImageAsync(InstanceModel instance, ImageType type)
    {
        IEnumerable<Image> images = await _onboardingDataService.GetSetupImagesAsync(instance);
        Image? logoImage = images.Where(i => i.Type == type).FirstOrDefault();

        return logoImage;
    }

    private async Task<LetterHeadResult> UploadLetterHeadAsync(InstanceModel instance, byte[] pdf, string name)
    {
        HttpResponseMessage uploadResponse = await _httpHelper.UploadFileAsync("InstanceApi", _briefYourMarketInstanceHelper.GetFormattedInstanceString(instance.Subdomain), "api/letterheads", name, pdf, _instanceSettings.ApiKey);
        if (!uploadResponse.IsSuccessStatusCode)
            throw new InvalidOperationException($"Failed to upload letterhead. Result: ({uploadResponse.StatusCode}) {uploadResponse.ReasonPhrase}.");

        string responseText = await uploadResponse.Content.ReadAsStringAsync();
        List<LetterHeadResult>? letterHeadResult = JsonSerializer.Deserialize<List<LetterHeadResult>>(responseText) ?? throw new InvalidOperationException("Failed to upload letter, no response given from API.");
        return letterHeadResult.First();
    }
}
