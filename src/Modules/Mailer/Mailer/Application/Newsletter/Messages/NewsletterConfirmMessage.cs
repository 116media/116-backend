using _116.Mailer.Contracts.Application.Messages;

namespace _116.Mailer.Application.Newsletter.Messages;

/// <summary>
/// The double opt-in request. Transactional rather than <see cref="EnumMessageClass.Subscription" />
/// because it is the mail that asks for the subscription: gating it on one would mean it could
/// never be sent.
/// </summary>
/// <param name="Subscriber">The address that asked to subscribe.</param>
/// <param name="ConfirmUrl">The link that confirms the subscription.</param>
public record NewsletterConfirmMessage(MessageRecipient Subscriber, string ConfirmUrl) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => NewsletterMessageTemplates.NewsletterConfirm;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Subscriber];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string> { ["confirmUrl"] = ConfirmUrl };
}
