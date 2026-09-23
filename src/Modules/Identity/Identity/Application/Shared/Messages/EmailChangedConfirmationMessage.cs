using _116.Mailer.Contracts.Application.Messages;

namespace _116.Identity.Application.Shared.Messages;

/// <summary>
/// An account's address was replaced: the confirmation to the address that now owns it.
/// </summary>
/// <param name="NewAddress">The address the account now uses.</param>
/// <param name="ChangedAt">When the address was replaced.</param>
public record EmailChangedConfirmationMessage(MessageRecipient NewAddress, DateTimeOffset ChangedAt) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => IdentityMessageTemplates.EmailChangedConfirmNew;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [NewAddress];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = NewAddress.DisplayName ?? string.Empty,
            ["changeTime"] = ChangedAt.ToString("u"),
        };
}
