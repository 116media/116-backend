using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Identity.Application.Shared.OutboundEmails;

/// <summary>
/// A role was granted to or revoked from an account; the action token carries which.
/// </summary>
/// <param name="User">The account whose roles changed.</param>
/// <param name="RoleName">The role that was granted or revoked.</param>
/// <param name="Action">Either "granted" or "revoked".</param>
public record RoleChangedEmail(EmailRecipient User, string RoleName, string Action) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => IdentityEmailTemplates.RoleChanged;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [User];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = User.DisplayName ?? string.Empty,
            ["roleName"] = RoleName,
            ["action"] = Action,
        };
}
