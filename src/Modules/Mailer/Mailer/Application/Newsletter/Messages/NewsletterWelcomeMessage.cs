using _116.Mailer.Contracts.Application.Messages;

namespace _116.Mailer.Application.Newsletter.Messages;

/// <summary>
/// The first mail a confirmed subscriber receives. Subscription-class, so it is suppressed
/// unless the address holds a confirmed subscription at dispatch time.
/// </summary>
/// <param name="Subscriber">The address that confirmed.</param>
/// <param name="UnsubscribeUrl">The one-click opt-out link.</param>
public record NewsletterWelcomeMessage(MessageRecipient Subscriber, string UnsubscribeUrl) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Subscription;

    /// <inheritdoc />
    public override string TemplateName => NewsletterMessageTemplates.NewsletterWelcome;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Subscriber];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string> { ["unsubscribeUrl"] = UnsubscribeUrl };
}
