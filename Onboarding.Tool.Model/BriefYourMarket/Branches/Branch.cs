using Onboarding.Tool.Model.BriefYourMarket.Brands;

namespace Onboarding.Tool.Model.BriefYourMarket.Branches;

public class Branch
{
    public required int Id { get; init; }

    public required bool Default { get; init; }

    public required string Name { get; init; }

    public required string Address1 { get; init; }

    public string Address2 { get; init; } = string.Empty;

    public string Address3 { get; init; } = string.Empty;

    public required string Town { get; init; }

    public required string PostCode1 { get; init; }

    public required string PostCode2 { get; init; }

    public string SalesEmail { get; init; } = string.Empty;

    public string SalesPhoneNumber { get; init; } = string.Empty;

    public string LettingsEmail { get; init; } = string.Empty;

    public string LettingsPhoneNumber { get; init; } = string.Empty;

    public IEnumerable<BrandSetting> Settings { get; set; } = [];

    public string DomesticNumber
    {
        get
        {
            return SalesPhoneNumber.Replace("+44", "0");
        }
    }

    public string FormattedAddress
    {
        get
        {
            List<string> parts = new()
            {
                Address1,
                Address2,
                Address3,
                Town,
                $"{PostCode1} {PostCode2}"
            };

            return string.Join(", ", parts);
        }
    }
}
