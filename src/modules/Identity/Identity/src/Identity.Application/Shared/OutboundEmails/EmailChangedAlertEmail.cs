using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Identity.Application.Shared.OutboundEmails;

/// <summary>
/// An account's address was replaced: the alert to the address that was removed, so a
/// hijacked account is visible to its real owner.
/// </summary>
/// <param name="FormerAddress">The address the account used before the change.</param>
/// <param name="NewEmailMasked">The new address with its local part masked.</param>
/// <param name="ChangedAt">When the address was replaced.</param>
public record EmailChangedAlertEmail(EmailRecipient FormerAddress, string NewEmailMasked, DateTimeOffset ChangedAt)
    : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => IdentityEmailTemplates.EmailChangedAlertOld;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [FormerAddress];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = FormerAddress.DisplayName ?? string.Empty,
            ["newEmailMasked"] = NewEmailMasked,
            ["changeTime"] = ChangedAt.ToString("u"),
        };
}
