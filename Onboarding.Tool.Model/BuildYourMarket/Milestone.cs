namespace Onboarding.Tool.Model.BuildYourMarket;

public class Milestone
{
    public required int Id { get; init; }

    public required int Length { get; init; }

    public required string MilestoneType { get; init; }

    public required string MilestoneContext { get; init; }
}
