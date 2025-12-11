namespace Onboarding.Tool.Model.Robocop.DirectMail;

public class CreateDirectoryResponse
{
    public required List<int> InstanceIds { get; set; }

    public required int Id { get; init; }

    public required string Name { get; init; }
}
