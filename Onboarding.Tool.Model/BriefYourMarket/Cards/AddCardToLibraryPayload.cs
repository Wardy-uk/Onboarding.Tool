namespace Onboarding.Tool.Model.BriefYourMarket.Cards;

public class AddCardToLibraryPayload
{
    public required int LibraryId { get; init; }

    public required int ProductId { get; init; }

    public required bool RequiresApproval { get; init; }
}
