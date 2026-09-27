using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Identity.Application.Shared.OutboundEmails;

/// <summary>
/// An account's address was replaced: the confirmation to the address that now owns it.
/// </summary>
/// <param name="NewAddress">The address the account now uses.</param>
/// <param name="ChangedAt">When the address was replaced.</param>
public record EmailChangedConfirmationEmail(EmailRecipient NewAddress, DateTimeOffset ChangedAt) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => IdentityEmailTemplates.EmailChangedConfirmNew;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [NewAddress];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = NewAddress.DisplayName ?? string.Empty,
            ["changeTime"] = ChangedAt.ToString("u"),
        };
}
