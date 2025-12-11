using Onboarding.Tool.Model.Converters;
using Onboarding.Tool.Model.Enums.BriefYourMarket.DirectMail;
using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.BriefYourMarket.Cards;

public class CardSetting
{
    [JsonConverter(typeof(JsonStringEnumConverterWithDefaults))]
    public required DirectMailCategory Category { get; init; }

    public required string Title { get; init; }

    public required string GlobalCodeSuffix { get; init; }

    public required int TypeId { get; init; }

    public required int PriceModelId { get; init; }

    public required string Keywords { get; init; }

    public required int PrintProviderId { get; init; }

    public required string StockIds { get; init; }

    public required string DeliveryMethodIds { get; init; }

    public required string PaperTypeIds { get; init; }

    public required string LaminationIds { get; init; }

    public required string Path { get; init; }

    public required string DataType { get; init; }

    public async Task<string> GetContentAsync()
    {
        string content;

        try
        {
            content = await File.ReadAllTextAsync(Path);
        }
        catch
        {
            throw;
        }

        return content;
    }

    public List<int> GetStockIds()
    {
        List<int> stockIds = StockIds
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.Parse(id.Trim()))
            .ToList();

        return stockIds;
    }

    public List<int> GetDeliveryMethodIds()
    {
        List<int> deliveryMethodIds = DeliveryMethodIds
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.Parse(id.Trim()))
            .ToList();

        return deliveryMethodIds;
    }

    public List<int> GetPaperTypeIdsIds()
    {
        List<int> paperTypeIds = PaperTypeIds
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.Parse(id.Trim()))
            .ToList();

        return paperTypeIds;
    }

    public List<int> GetLaminationIds()
    {
        List<int> laminationIds = LaminationIds
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.Parse(id.Trim()))
            .ToList();

        return laminationIds;
    }
}
