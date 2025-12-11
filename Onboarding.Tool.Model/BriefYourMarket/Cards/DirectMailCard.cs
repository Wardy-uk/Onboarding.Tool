namespace Onboarding.Tool.Model.BriefYourMarket.Cards;

public class DirectMailCard
{
    public required string Title { get; init; }

    public required string GlobalCode { get; init; }

    public required string Status { get; init; }

    public required int TypeId { get; init; }

    public required int PriceModelId { get; init; }

    public required int CategoryId { get; init; }

    public required string Keywords { get; init; }

    public required int PrintProviderId { get; init; }

    public required List<int> StockIds { get; init; }

    public required List<int> DeliveryMethodIds { get; init; }

    public required List<int> PaperTypeIds { get; init; }

    public required List<int> LaminationIds { get; init; }

    public List<int> EnvelopeTypeIds { get; init; } = new();

    public string ThumbnailImage { get; init; } = string.Empty;

    public string PreviewImage { get; init; } = string.Empty;

    public string ReverseThumbnailImage { get; init; } = string.Empty;

    public string ReversePreviewImage { get; init; } = string.Empty;

    public required bool IsDuplex { get; init; }

    public required int PageCount { get; init; }

    public required int MinOrderQuantity { get; init; }

    public required int MaxOrderQuantity { get; init; }

    public required string DataType { get; init; }

    public required bool IsQuickOrder { get; init; }

    public required string Template { get; init; }
}
