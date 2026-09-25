using _116.Content.Application.Commerce.OutboundEmails;
using _116.Content.Application.Editorial.OutboundEmails;
using _116.Content.Application.Interactions.OutboundEmails;
using _116.Identity.Application.Shared.OutboundEmails;
using _116.Identity.Domain.Enums;
using _116.Mailer.Application.Newsletter.OutboundEmails;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Application.Templates;
using _116.Mailer.Application.Templates.Messages;
using _116.Mailer.Contracts.Application.OutboundEmails;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.OutboundEmails;

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
    /// One representative instance of every shipped message record, with the class it must
    /// declare. A new record that is not listed here fails
    /// <see cref="Catalogue_ShouldCoverEveryShippedMessageRecord" />.
    /// </summary>
    public static TheoryData<OutboundEmail, EnumEmailClass> AllMessages()
    {
        return new TheoryData<OutboundEmail, EnumEmailClass>
        {
            { new WelcomeEmail(User()), EnumEmailClass.Transactional },
            {
                new OtpIssuedEmail(User(), IdentityEmailTemplates.EmailVerificationOtp, "123456", 60),
                EnumEmailClass.Transactional
            },
            { new PasswordChangedEmail(User(), EnumPasswordChangeOrigin.Reset, At), EnumEmailClass.Transactional },
            { new SignedOutAllDevicesEmail(User(), ByAdmin: true, At), EnumEmailClass.Transactional },
            { new EmailChangedAlertEmail(User(), "f***@example.com", At), EnumEmailClass.Transactional },
            { new EmailChangedConfirmationEmail(User(), At), EnumEmailClass.Transactional },
            { new RoleChangedEmail(User(), "Admin", "granted"), EnumEmailClass.Transactional },
            { new RefreshTokenReplayEmail(User(), At), EnumEmailClass.Transactional },
            {
                new OrderInvoiceEmail(User(), "AB12CD34", "150.00", "BankTransfer", "Musique"),
                EnumEmailClass.Transactional
            },
            {
                new PaymentReceiptEmail(User(), "AB12CD34", "150.00", "https://res.example/r.png", At),
                EnumEmailClass.Transactional
            },
            { new PaymentRejectedEmail(User(), "AB12CD34", "Reference missing"), EnumEmailClass.Transactional },
            { new OrderCancelledEmail(User(), "AB12CD34"), EnumEmailClass.Transactional },
            { new PromotionRemovedEmail(User(), "Eloko Oyo", "Policy violation", At), EnumEmailClass.Transactional },
            {
                new CommissionedContentPublishedEmail(User(), "Eloko Oyo", "https://app.example/a/eloko-oyo"),
                EnumEmailClass.Transactional
            },
            {
                new CommissionedContentRejectedEmail(User(), "Eloko Oyo", "Policy violation"),
                EnumEmailClass.Transactional
            },
            { new ShootScheduledEmail(User(), "Eloko Oyo", At.UtcDateTime), EnumEmailClass.Transactional },
            {
                new ArtistVerifiedEmail(User(), "Fally Ipupa", "https://app.example/artists/fally"),
                EnumEmailClass.Notification
            },
            {
                new RevisionDecidedEmail(User(), "Eloko Oyo", "accepted", "https://app.example/l/eloko-oyo"),
                EnumEmailClass.Notification
            },
            {
                new SubmissionDecidedEmail(User(), "Eloko Oyo", "approved", "Great transcription."),
                EnumEmailClass.Notification
            },
            {
                new CommentReplyEmail(User(), "Aline", "Eloko Oyo review", "Agreed", "https://app.example/a/x"),
                EnumEmailClass.Notification
            },
            { new NewsletterConfirmEmail(User(), "https://app.example/confirm/x"), EnumEmailClass.Transactional },
            { new NewsletterWelcomeEmail(User(), "https://app.example/unsubscribe/x"), EnumEmailClass.Subscription },
        };
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void EveryMessage_ShouldDeclareItsPolicyClass(OutboundEmail message, EnumEmailClass expected)
    {
        message.Class.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void EveryMessage_ShouldResolveAtLeastOneRecipient(OutboundEmail message, EnumEmailClass _)
    {
        message.Recipients.Should().NotBeEmpty();
        message.Recipients.Should().AllSatisfy(recipient => recipient.Address.Should().NotBeNullOrWhiteSpace());
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void EveryMessage_ShouldNameATemplateThatRendersInEveryCulture(OutboundEmail message, EnumEmailClass _)
    {
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

        IEnumerable<Type> covered = AllMessages().Select(row => row.Data.Item1.GetType());

        shipped.Except(covered).Should().BeEmpty("every shipped OutboundEmail record must be listed in AllMessages()");
    }
}
