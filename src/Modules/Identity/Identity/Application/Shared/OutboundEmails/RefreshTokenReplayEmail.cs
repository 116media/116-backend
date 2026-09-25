using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Identity.Application.Shared.OutboundEmails;

/// <summary>
/// A refresh token was replayed and every session was revoked: the security alert telling the
/// account holder why they were signed out.
/// </summary>
/// <param name="User">The account whose sessions were revoked.</param>
/// <param name="DetectedAt">When the replay was detected.</param>
public record RefreshTokenReplayEmail(EmailRecipient User, DateTimeOffset DetectedAt) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => IdentityEmailTemplates.RefreshTokenReplayAlert;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [User];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = User.DisplayName ?? string.Empty,
            ["time"] = DetectedAt.ToString("u"),
        };
}
