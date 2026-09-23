using _116.Mailer.Contracts.Application.Messages;

namespace _116.Identity.Application.Shared.Messages;

/// <summary>
/// An account finished verification: the welcome that every verification path produces.
/// </summary>
/// <param name="User">The newly verified account.</param>
public record WelcomeMessage(MessageRecipient User) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => IdentityMessageTemplates.Welcome;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [User];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string> { ["userName"] = User.DisplayName ?? string.Empty };
}
