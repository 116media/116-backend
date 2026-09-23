using _116.Mailer.Contracts.Application.Messages;

namespace _116.Identity.Application.Shared.Messages;

/// <summary>
/// An account's address was replaced: the alert to the address that was removed, so a
/// hijacked account is visible to its real owner.
/// </summary>
/// <param name="FormerAddress">The address the account used before the change.</param>
/// <param name="NewEmailMasked">The new address with its local part masked.</param>
/// <param name="ChangedAt">When the address was replaced.</param>
public record EmailChangedAlertMessage(MessageRecipient FormerAddress, string NewEmailMasked, DateTimeOffset ChangedAt)
    : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => IdentityMessageTemplates.EmailChangedAlertOld;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [FormerAddress];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = FormerAddress.DisplayName ?? string.Empty,
            ["newEmailMasked"] = NewEmailMasked,
            ["changeTime"] = ChangedAt.ToString("u"),
        };
}
