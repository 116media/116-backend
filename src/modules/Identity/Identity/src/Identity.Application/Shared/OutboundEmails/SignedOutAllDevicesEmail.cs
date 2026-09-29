using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Identity.Application.Shared.OutboundEmails;

/// <summary>
/// Every session on an account was ended. Whether an admin did it selects the template, since
/// a forced sign-out reads differently from one the user triggered.
/// </summary>
/// <param name="User">The account that was signed out.</param>
/// <param name="ByAdmin">Whether an administrator forced the sign-out.</param>
/// <param name="SignedOutAt">When the sessions were ended.</param>
public record SignedOutAllDevicesEmail(EmailRecipient User, bool ByAdmin, DateTimeOffset SignedOutAt) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName =>
        ByAdmin ? IdentityEmailTemplates.AccountForceLoggedOut : IdentityEmailTemplates.SignedOutAllDevices;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [User];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = User.DisplayName ?? string.Empty,
            ["time"] = SignedOutAt.ToString("u"),
        };
}
