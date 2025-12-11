namespace Onboarding.Tool.Model.Robocop.Images;

public class ImageDirectory
{
    public required int Id { get; set; }

    public required string Path { get; set; }

    public int? ParentId { get; set; }
}
