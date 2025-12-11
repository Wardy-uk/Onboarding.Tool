namespace Onboarding.Tool.Model.BuildYourMarket;

public class PostCodeDistrict
{
    public required string OutwardCode { get; init; }

    public required string Description { get; init; }

    public required List<string> Sectors { get; init; }

    public required bool AllSectors { get; init; }
}
