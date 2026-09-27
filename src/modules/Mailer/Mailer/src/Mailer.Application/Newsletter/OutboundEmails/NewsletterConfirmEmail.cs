using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Mailer.Application.Newsletter.OutboundEmails;

/// <summary>
/// The double opt-in request. Transactional rather than <see cref="EnumEmailClass.Subscription" />
/// because it is the mail that asks for the subscription: gating it on one would mean it could
/// never be sent.
/// </summary>
/// <param name="Subscriber">The address that asked to subscribe.</param>
/// <param name="ConfirmUrl">The link that confirms the subscription.</param>
public record NewsletterConfirmEmail(EmailRecipient Subscriber, string ConfirmUrl) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Transactional;

    /// <inheritdoc />
    public override string TemplateName => NewsletterEmailTemplates.NewsletterConfirm;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Subscriber];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string> { ["confirmUrl"] = ConfirmUrl };
}
