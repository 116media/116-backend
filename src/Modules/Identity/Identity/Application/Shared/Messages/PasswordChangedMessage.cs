using _116.Identity.Domain.Enums;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Identity.Application.Shared.Messages;

/// <summary>
/// An account password was replaced. The origin selects the template and, with it, the
/// timestamp token name each template was written against.
/// </summary>
/// <param name="User">The account whose password changed.</param>
/// <param name="Origin">The flow that replaced the password.</param>
/// <param name="ChangedAt">When the password was replaced.</param>
public record PasswordChangedMessage(MessageRecipient User, EnumPasswordChangeOrigin Origin, DateTimeOffset ChangedAt)
    : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName =>
        Origin switch
        {
            EnumPasswordChangeOrigin.Reset => IdentityMessageTemplates.PasswordResetCompleted,
            EnumPasswordChangeOrigin.SetLocal => IdentityMessageTemplates.LocalPasswordAdded,
            _ => IdentityMessageTemplates.PasswordChanged,
        };

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [User];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        Origin switch
        {
            EnumPasswordChangeOrigin.Reset => new Dictionary<string, string>
            {
                ["userName"] = User.DisplayName ?? string.Empty,
                ["resetTime"] = ChangedAt.ToString("u"),
            },
            EnumPasswordChangeOrigin.SetLocal => new Dictionary<string, string>
            {
                ["userName"] = User.DisplayName ?? string.Empty,
            },
            _ => new Dictionary<string, string>
            {
                ["userName"] = User.DisplayName ?? string.Empty,
                ["changeTime"] = ChangedAt.ToString("u"),
            },
        };
}
