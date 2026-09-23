using _116.Mailer.Contracts.Application.Messages;

namespace _116.Identity.Application.Shared.Messages;

/// <summary>
/// A refresh token was replayed and every session was revoked: the security alert telling the
/// account holder why they were signed out.
/// </summary>
/// <param name="User">The account whose sessions were revoked.</param>
/// <param name="DetectedAt">When the replay was detected.</param>
public record RefreshTokenReplayMessage(MessageRecipient User, DateTimeOffset DetectedAt) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => IdentityMessageTemplates.RefreshTokenReplayAlert;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [User];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = User.DisplayName ?? string.Empty,
            ["time"] = DetectedAt.ToString("u"),
        };
}
