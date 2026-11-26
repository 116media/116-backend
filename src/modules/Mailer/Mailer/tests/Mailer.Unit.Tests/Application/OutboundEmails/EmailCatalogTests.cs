using _116.Content.Application.Commerce.OutboundEmails;
using _116.Content.Application.Editorial.OutboundEmails;
using _116.Content.Application.Interactions.OutboundEmails;
using _116.Identity.Application.Shared.OutboundEmails;
using _116.Identity.Domain.Enums;
using _116.Mailer.Application.Newsletter.OutboundEmails;
using _116.Mailer.Application.Shared.Templates.Emails;
using _116.Mailer.Application.Shared.Templates.Emails.Messages;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Mailer.Unit.Tests.Application.OutboundEmails;

/// <summary>
/// Unit tests over every <see cref="OutboundEmail" /> record the modules ship: each declares a
/// policy class, resolves a recipient, and carries exactly the tokens its own template
/// declares — so a record and its resources can never drift apart unnoticed.
/// </summary>
public class EmailCatalogTests
{
    private static readonly DateTimeOffset At = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    private static readonly EmailTemplateRenderer Renderer = new(
        LocalizerFactory.CreateMessage<EmailTemplateMessage>()
    );

    private static EmailRecipient User(string locale = "en")
    {
        return new EmailRecipient(Guid.NewGuid(), "fan@example.com", "Fally", locale);
    }

    /// <summary>
    /// One representative instance of every shipped message record, keyed by the record's name,
    /// with the class it must declare. A new record that is not listed here fails
    /// <see cref="Catalogue_ShouldCoverEveryShippedMessageRecord" />.
    /// </summary>
    private static readonly Dictionary<string, (OutboundEmail Message, EnumEmailClass Class)> Catalogue = new()
    {
        [nameof(WelcomeEmail)] = (new WelcomeEmail(User()), EnumEmailClass.Transactional),
        [nameof(OtpIssuedEmail)] = (
            new OtpIssuedEmail(User(), IdentityEmailTemplates.EmailVerificationOtp, "123456", 60),
            EnumEmailClass.Transactional
        ),
        [nameof(PasswordChangedEmail)] = (
            new PasswordChangedEmail(User(), EnumPasswordChangeOrigin.Reset, At),
            EnumEmailClass.Transactional
        ),
        [nameof(SignedOutAllDevicesEmail)] = (
            new SignedOutAllDevicesEmail(User(), ByAdmin: true, At),
            EnumEmailClass.Transactional
        ),
        [nameof(EmailChangedAlertEmail)] = (
            new EmailChangedAlertEmail(User(), "f***@example.com", At),
            EnumEmailClass.Transactional
        ),
        [nameof(EmailChangedConfirmationEmail)] = (
            new EmailChangedConfirmationEmail(User(), At),
            EnumEmailClass.Transactional
        ),
        [nameof(RoleChangedEmail)] = (new RoleChangedEmail(User(), "Admin", "granted"), EnumEmailClass.Transactional),
        [nameof(RefreshTokenReplayEmail)] = (new RefreshTokenReplayEmail(User(), At), EnumEmailClass.Transactional),
        [nameof(OrderInvoiceEmail)] = (
            new OrderInvoiceEmail(User(), "AB12CD34", "150.00", "BankTransfer", "Musique"),
            EnumEmailClass.Transactional
        ),
        [nameof(PaymentReceiptEmail)] = (
            new PaymentReceiptEmail(User(), "AB12CD34", "150.00", "https://res.example/r.png", At),
            EnumEmailClass.Transactional
        ),
        [nameof(PaymentRejectedEmail)] = (
            new PaymentRejectedEmail(User(), "AB12CD34", "Reference missing"),
            EnumEmailClass.Transactional
        ),
        [nameof(OrderCancelledEmail)] = (new OrderCancelledEmail(User(), "AB12CD34"), EnumEmailClass.Transactional),
        [nameof(PromotionRemovedEmail)] = (
            new PromotionRemovedEmail(User(), "Eloko Oyo", "Policy violation", At),
            EnumEmailClass.Transactional
        ),
        [nameof(CommissionedContentPublishedEmail)] = (
            new CommissionedContentPublishedEmail(User(), "Eloko Oyo", "https://app.example/a/eloko-oyo"),
            EnumEmailClass.Transactional
        ),
        [nameof(CommissionedContentRejectedEmail)] = (
            new CommissionedContentRejectedEmail(User(), "Eloko Oyo", "Policy violation"),
            EnumEmailClass.Transactional
        ),
        [nameof(ShootScheduledEmail)] = (
            new ShootScheduledEmail(User(), "Eloko Oyo", At.UtcDateTime),
            EnumEmailClass.Transactional
        ),
        [nameof(ArtistVerifiedEmail)] = (
            new ArtistVerifiedEmail(User(), "Fally Ipupa", "https://app.example/artists/fally"),
            EnumEmailClass.Notification
        ),
        [nameof(RevisionDecidedEmail)] = (
            new RevisionDecidedEmail(User(), "Eloko Oyo", "accepted", "https://app.example/l/eloko-oyo"),
            EnumEmailClass.Notification
        ),
        [nameof(SubmissionDecidedEmail)] = (
            new SubmissionDecidedEmail(User(), "Eloko Oyo", "approved", "Great transcription."),
            EnumEmailClass.Notification
        ),
        [nameof(CommentReplyEmail)] = (
            new CommentReplyEmail(User(), "Aline", "Eloko Oyo review", "Agreed", "https://app.example/a/x"),
            EnumEmailClass.Notification
        ),
        [nameof(NewsletterConfirmEmail)] = (
            new NewsletterConfirmEmail(User(), "https://app.example/confirm/x"),
            EnumEmailClass.Transactional
        ),
        [nameof(NewsletterWelcomeEmail)] = (
            new NewsletterWelcomeEmail(User(), "https://app.example/unsubscribe/x"),
            EnumEmailClass.Subscription
        ),
    };

    /// <summary>
    /// The catalogue keys as theory rows. Keying by name rather than by the record itself keeps the
    /// rows serializable, so the runner enumerates one case per message.
    /// </summary>
    /// <returns>The message record names.</returns>
    public static TheoryData<string> AllMessages() => new(Catalogue.Keys.OrderBy(name => name, StringComparer.Ordinal));

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void EveryMessage_ShouldDeclareItsPolicyClass(string messageName)
    {
        (OutboundEmail message, EnumEmailClass expected) = Catalogue[messageName];

        message.Class.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void EveryMessage_ShouldResolveAtLeastOneRecipient(string messageName)
    {
        OutboundEmail message = Catalogue[messageName].Message;

        message.Recipients.Should().NotBeEmpty();
        message.Recipients.Should().AllSatisfy(recipient => recipient.Address.Should().NotBeNullOrWhiteSpace());
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void EveryMessage_ShouldNameATemplateThatRendersInEveryCulture(string messageName)
    {
        OutboundEmail message = Catalogue[messageName].Message;

        foreach (string culture in (string[])["en", "fr"])
        {
            RenderedEmail rendered = Renderer.Render(message.TemplateName, message.Tokens, culture);

            rendered.Subject.Should().NotBeNullOrWhiteSpace();
            rendered.HtmlBody.Should().NotContain("{{");
            rendered.TextBody.Should().NotContain("{{");
        }
    }

    [Fact]
    public void Catalogue_ShouldCoverEveryShippedMessageRecord()
    {
        IEnumerable<Type> shipped = typeof(OutboundEmail)
            .Assembly.GetTypes()
            .Concat(typeof(WelcomeEmail).Assembly.GetTypes())
            .Concat(typeof(OrderInvoiceEmail).Assembly.GetTypes())
            .Concat(typeof(NewsletterConfirmEmail).Assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(OutboundEmail)) && !type.IsAbstract)
            .Distinct();

        IEnumerable<Type> covered = Catalogue.Values.Select(entry => entry.Message.GetType());

        shipped.Except(covered).Should().BeEmpty("every shipped OutboundEmail record must be listed in AllMessages()");
    }
}
