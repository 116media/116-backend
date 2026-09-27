using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Mailer.Application.Newsletter.OutboundEmails;

/// <summary>
/// The first mail a confirmed subscriber receives. Subscription-class, so it is suppressed
/// unless the address holds a confirmed subscription at dispatch time.
/// </summary>
/// <param name="Subscriber">The address that confirmed.</param>
/// <param name="UnsubscribeUrl">The one-click opt-out link.</param>
public record NewsletterWelcomeEmail(EmailRecipient Subscriber, string UnsubscribeUrl) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Subscription;

    /// <inheritdoc />
    public override string TemplateName => NewsletterEmailTemplates.NewsletterWelcome;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Subscriber];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string> { ["unsubscribeUrl"] = UnsubscribeUrl };
}
