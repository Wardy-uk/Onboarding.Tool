namespace Onboarding.Tool.Model.Dashboard.Branches;

public class ImportBranch
{
    public required bool IsDefault { get; init; }

    public required string Name { get; init; }

    public required string SalesEmail { get; init; }

    public required string SalesPhone { get; init; }

    public required string LettingsEmail { get; init; }

    public required string LettingsPhone { get; init; }

    public required string Address1 { get; init; }

    public string? Address2 { get; init; }

    public string? Address3 { get; init; }

    public required string Town { get; init; }

    public required string PostCode1 { get; set; }

    public required string PostCode2 { get; set; }
}
