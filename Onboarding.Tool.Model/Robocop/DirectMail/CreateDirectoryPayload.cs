namespace Onboarding.Tool.Model.Robocop.DirectMail;

public class CreateDirectoryPayload
{
    public required string Name { get; set; }

    public string CleanedName
    {
        get
        {
            return Name.Replace("-", "");
        }
    }

    public int? ParentDirectoryId { get; set; }
}
