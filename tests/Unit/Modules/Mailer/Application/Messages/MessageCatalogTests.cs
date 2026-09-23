using _116.Content.Application.Commerce.Messages;
using _116.Content.Application.Editorial.Messages;
using _116.Content.Application.Interactions.Messages;
using _116.Identity.Application.Shared.Messages;
using _116.Identity.Domain.Enums;
using _116.Mailer.Application.Newsletter.Messages;
using _116.Mailer.Application.Shared.Services;
using _116.Mailer.Application.Templates;
using _116.Mailer.Application.Templates.Messages;
using _116.Mailer.Contracts.Application.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Application.Messages;

/// <summary>
/// Unit tests over every <see cref="Message" /> record the modules ship: each declares a
/// policy class, resolves a recipient, and carries exactly the tokens its own template
/// declares — so a record and its resources can never drift apart unnoticed.
/// </summary>
public class MessageCatalogTests
{
    private static readonly DateTimeOffset At = new(2026, 8, 16, 12, 0, 0, TimeSpan.Zero);

    private static readonly EmailTemplateRenderer Renderer = new(
        LocalizerFactory.CreateMessage<EmailTemplateMessage>()
    );

    private static MessageRecipient User(string locale = "en")
    {
        return new MessageRecipient(Guid.NewGuid(), "fan@example.com", "Fally", locale);
    }

    /// <summary>
    /// One representative instance of every shipped message record, with the class it must
    /// declare. A new record that is not listed here fails
    /// <see cref="Catalogue_ShouldCoverEveryShippedMessageRecord" />.
    /// </summary>
    public static TheoryData<Message, EnumMessageClass> AllMessages()
    {
        return new TheoryData<Message, EnumMessageClass>
        {
            { new WelcomeMessage(User()), EnumMessageClass.Transactional },
            {
                new OtpIssuedMessage(User(), IdentityMessageTemplates.EmailVerificationOtp, "123456", 60),
                EnumMessageClass.Transactional
            },
            { new PasswordChangedMessage(User(), EnumPasswordChangeOrigin.Reset, At), EnumMessageClass.Transactional },
            { new SignedOutAllDevicesMessage(User(), ByAdmin: true, At), EnumMessageClass.Transactional },
            { new EmailChangedAlertMessage(User(), "f***@example.com", At), EnumMessageClass.Transactional },
            { new EmailChangedConfirmationMessage(User(), At), EnumMessageClass.Transactional },
            { new RoleChangedMessage(User(), "Admin", "granted"), EnumMessageClass.Transactional },
            { new RefreshTokenReplayMessage(User(), At), EnumMessageClass.Transactional },
            {
                new OrderInvoiceMessage(User(), "AB12CD34", "150.00", "BankTransfer", "Musique"),
                EnumMessageClass.Transactional
            },
            {
                new PaymentReceiptMessage(User(), "AB12CD34", "150.00", "https://res.example/r.png", At),
                EnumMessageClass.Transactional
            },
            { new PaymentRejectedMessage(User(), "AB12CD34", "Reference missing"), EnumMessageClass.Transactional },
            { new OrderCancelledMessage(User(), "AB12CD34"), EnumMessageClass.Transactional },
            {
                new PromotionRemovedMessage(User(), "Eloko Oyo", "Policy violation", At),
                EnumMessageClass.Transactional
            },
            {
                new CommissionedContentPublishedMessage(User(), "Eloko Oyo", "https://app.example/a/eloko-oyo"),
                EnumMessageClass.Transactional
            },
            {
                new CommissionedContentRejectedMessage(User(), "Eloko Oyo", "Policy violation"),
                EnumMessageClass.Transactional
            },
            { new ShootScheduledMessage(User(), "Eloko Oyo", At.UtcDateTime), EnumMessageClass.Transactional },
            {
                new ArtistVerifiedMessage(User(), "Fally Ipupa", "https://app.example/artists/fally"),
                EnumMessageClass.Notification
            },
            {
                new RevisionDecidedMessage(User(), "Eloko Oyo", "accepted", "https://app.example/l/eloko-oyo"),
                EnumMessageClass.Notification
            },
            {
                new SubmissionDecidedMessage(User(), "Eloko Oyo", "approved", "Great transcription."),
                EnumMessageClass.Notification
            },
            {
                new CommentReplyMessage(User(), "Aline", "Eloko Oyo review", "Agreed", "https://app.example/a/x"),
                EnumMessageClass.Notification
            },
            { new NewsletterConfirmMessage(User(), "https://app.example/confirm/x"), EnumMessageClass.Transactional },
            {
                new NewsletterWelcomeMessage(User(), "https://app.example/unsubscribe/x"),
                EnumMessageClass.Subscription
            },
        };
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void EveryMessage_ShouldDeclareItsPolicyClass(Message message, EnumMessageClass expected)
    {
        message.Class.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void EveryMessage_ShouldResolveAtLeastOneRecipient(Message message, EnumMessageClass _)
    {
        message.Recipients.Should().NotBeEmpty();
        message.Recipients.Should().AllSatisfy(recipient => recipient.Address.Should().NotBeNullOrWhiteSpace());
    }

    [Theory]
    [MemberData(nameof(AllMessages))]
    public void EveryMessage_ShouldNameATemplateThatRendersInEveryCulture(Message message, EnumMessageClass _)
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
        IEnumerable<Type> shipped = typeof(Message)
            .Assembly.GetTypes()
            .Concat(typeof(WelcomeMessage).Assembly.GetTypes())
            .Concat(typeof(OrderInvoiceMessage).Assembly.GetTypes())
            .Concat(typeof(NewsletterConfirmMessage).Assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(Message)) && !type.IsAbstract)
            .Distinct();

        IEnumerable<Type> covered = AllMessages().Select(row => row.Data.Item1.GetType());

        shipped.Except(covered).Should().BeEmpty("every shipped Message record must be listed in AllMessages()");
    }
}
