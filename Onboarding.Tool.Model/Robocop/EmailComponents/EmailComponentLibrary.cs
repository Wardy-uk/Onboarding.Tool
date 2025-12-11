namespace Onboarding.Tool.Model.Robocop.EmailComponents;

public class EmailComponentLibrary
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public required Dictionary<int, string> Instances { get; init; }

    public required bool IsGlobal { get; init; }

    public required string Category { get; init; }
}
