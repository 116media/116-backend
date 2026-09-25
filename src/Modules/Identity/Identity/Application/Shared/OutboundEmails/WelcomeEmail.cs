using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Identity.Application.Shared.OutboundEmails;

/// <summary>
/// An account finished verification: the welcome that every verification path produces.
/// </summary>
/// <param name="User">The newly verified account.</param>
public record WelcomeEmail(EmailRecipient User) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => IdentityEmailTemplates.Welcome;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [User];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string> { ["userName"] = User.DisplayName ?? string.Empty };
}
