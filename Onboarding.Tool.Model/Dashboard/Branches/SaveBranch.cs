using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Model.Dashboard.Branches;

public class SaveBranch
{
    public int Id { get; init; }

    public required bool IsDefault { get; init; }

    public required string Name { get; init; }

    public required string SalesEmail { get; init; }

    public required string SalesPhone { get; init; }

    public required string LettingsEmail { get; init; }

    public required string LettingsPhone { get; init; }

    public Address? Address { get; init; }
}
