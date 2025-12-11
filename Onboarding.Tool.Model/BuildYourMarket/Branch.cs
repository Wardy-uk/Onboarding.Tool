using System.Text.Json.Serialization;

namespace Onboarding.Tool.Model.BuildYourMarket;

public class Branch
{
    public required int BranchId { get; init; }

    public required string Name { get; init; }

    public int? Brand { get; init; }

    public int? CreditGroupId { get; init; }

    public required bool CustomDirty { get; init; }

    public required int EmailTemplateId { get; init; }

    public required int LetterTemplateId { get; init; }

    public required int PrintTemplateId { get; init; }

    public required string OfficePhone { get; init; }

    public required bool PersonalLandlordSalutation { get; init; }

    public string? PortalAccount { get; init; }

    public string? Region { get; init; }

    public required string RentAskingPriceChange { get; init; }

    public required bool Updating { get; init; }

    public string? RentAskingPriceMovementType { get; init; }

    public string? SignatureContactEmail { get; init; }

    public string? SignatureContactName { get; init; }

    public string? SignatureContactPosition { get; init; }

    public string? TenantTypes { get; init; }

    public bool? WholeOfUK { get; init; }

    public List<PostCodeDistrict>? PostCodeDistricts { get; init; }
}
