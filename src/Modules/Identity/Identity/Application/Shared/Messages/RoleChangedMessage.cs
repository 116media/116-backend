using _116.Mailer.Contracts.Application.Messages;

namespace _116.Identity.Application.Shared.Messages;

/// <summary>
/// A role was granted to or revoked from an account; the action token carries which.
/// </summary>
/// <param name="User">The account whose roles changed.</param>
/// <param name="RoleName">The role that was granted or revoked.</param>
/// <param name="Action">Either "granted" or "revoked".</param>
public record RoleChangedMessage(MessageRecipient User, string RoleName, string Action) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => IdentityMessageTemplates.RoleChanged;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [User];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = User.DisplayName ?? string.Empty,
            ["roleName"] = RoleName,
            ["action"] = Action,
        };
}
