using _116.Mailer.Application.Shared.Repositories;
using _116.Mailer.Contracts.Application.DTOs;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Domain.Entities;
using _116.Mailer.Infrastructure.Services;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="MessageDispatcher" />: the message class decides whether an
/// address's opt-in state may suppress delivery, and every permitted recipient is enqueued
/// in their own locale.
/// </summary>
public class MessageDispatcherTests
{
    private const string Address = "fan@example.com";

    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<INewsletterRepository> _newsletter = new();

    private MessageDispatcher Dispatcher =>
        new(_emailService.Object, _newsletter.Object, NullLogger<MessageDispatcher>.Instance);

    /// <summary>
    /// A message whose class is supplied per test, so one record covers every policy branch.
    /// </summary>
    private sealed record ClassifiedMessage(EnumMessageClass Class, params MessageRecipient[] To) : Message
    {
        public override EnumMessageClass Class { get; } = Class;

        public override string TemplateName => "AnyTemplate";

        public override IReadOnlyList<MessageRecipient> Recipients => To;

        public override IReadOnlyDictionary<string, string> Tokens =>
            new Dictionary<string, string> { ["userName"] = "Fally" };
    }

    private static MessageRecipient Recipient(string address = Address, string locale = "en")
    {
        return new MessageRecipient(UserId: null, Address: address, DisplayName: "Fally", Locale: locale);
    }

    private void SubscriberIs(NewsletterSubscriberEntity? subscriber)
    {
        _newsletter.Setup(r => r.GetByEmailAsync(Address, It.IsAny<CancellationToken>())).ReturnsAsync(subscriber);
    }

    private static NewsletterSubscriberEntity Pending()
    {
        return NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), Address);
    }

    private static NewsletterSubscriberEntity Subscribed()
    {
        NewsletterSubscriberEntity subscriber = Pending();
        subscriber.Confirm(DateTime.UtcNow);

        return subscriber;
    }

    private static NewsletterSubscriberEntity Unsubscribed()
    {
        NewsletterSubscriberEntity subscriber = Subscribed();
        subscriber.Unsubscribe(DateTime.UtcNow);

        return subscriber;
    }

    private void VerifyEnqueued(Times times)
    {
        _emailService.Verify(
            e =>
                e.EnqueueAsync(
                    It.IsAny<string>(),
                    It.IsAny<EmailRecipientDto>(),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()
                ),
            times
        );
    }

    [Theory]
    [InlineData(EnumMessageClass.Transactional)]
    [InlineData(EnumMessageClass.Operational)]
    public async Task DispatchAsync_AlwaysSentClasses_ShouldEnqueueEvenForAnUnsubscribedAddress(
        EnumMessageClass messageClass
    )
    {
        SubscriberIs(Unsubscribed());

        await Dispatcher.DispatchAsync(new ClassifiedMessage(messageClass, Recipient()), CancellationToken.None);

        VerifyEnqueued(Times.Once());
    }

    [Theory]
    [InlineData(EnumMessageClass.Transactional)]
    [InlineData(EnumMessageClass.Operational)]
    public async Task DispatchAsync_AlwaysSentClasses_ShouldNotEvenConsultTheOptInState(EnumMessageClass messageClass)
    {
        await Dispatcher.DispatchAsync(new ClassifiedMessage(messageClass, Recipient()), CancellationToken.None);

        _newsletter.Verify(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DispatchAsync_Notification_ShouldEnqueueForAnAddressThatNeverSubscribed()
    {
        SubscriberIs(null);

        await Dispatcher.DispatchAsync(
            new ClassifiedMessage(EnumMessageClass.Notification, Recipient()),
            CancellationToken.None
        );

        VerifyEnqueued(Times.Once());
    }

    [Fact]
    public async Task DispatchAsync_Notification_ShouldBeSuppressedForAnUnsubscribedAddress()
    {
        SubscriberIs(Unsubscribed());

        await Dispatcher.DispatchAsync(
            new ClassifiedMessage(EnumMessageClass.Notification, Recipient()),
            CancellationToken.None
        );

        VerifyEnqueued(Times.Never());
    }

    [Fact]
    public async Task DispatchAsync_Subscription_ShouldEnqueueOnlyForAConfirmedSubscription()
    {
        SubscriberIs(Subscribed());

        await Dispatcher.DispatchAsync(
            new ClassifiedMessage(EnumMessageClass.Subscription, Recipient()),
            CancellationToken.None
        );

        VerifyEnqueued(Times.Once());
    }

    [Fact]
    public async Task DispatchAsync_Subscription_ShouldBeSuppressedWhileTheSubscriptionIsUnconfirmed()
    {
        SubscriberIs(Pending());

        await Dispatcher.DispatchAsync(
            new ClassifiedMessage(EnumMessageClass.Subscription, Recipient()),
            CancellationToken.None
        );

        VerifyEnqueued(Times.Never());
    }

    [Fact]
    public async Task DispatchAsync_Subscription_ShouldBeSuppressedForAnAddressThatNeverSubscribed()
    {
        SubscriberIs(null);

        await Dispatcher.DispatchAsync(
            new ClassifiedMessage(EnumMessageClass.Subscription, Recipient()),
            CancellationToken.None
        );

        VerifyEnqueued(Times.Never());
    }

    [Fact]
    public async Task DispatchAsync_ShouldEnqueueEachRecipientInTheirOwnLocale()
    {
        var message = new ClassifiedMessage(
            EnumMessageClass.Transactional,
            Recipient("en@example.com", "en"),
            Recipient("fr@example.com", "fr")
        );

        await Dispatcher.DispatchAsync(message, CancellationToken.None);

        _emailService.Verify(
            e =>
                e.EnqueueAsync(
                    "AnyTemplate",
                    It.Is<EmailRecipientDto>(r => r.Address == "en@example.com" && r.Locale == "en"),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _emailService.Verify(
            e =>
                e.EnqueueAsync(
                    "AnyTemplate",
                    It.Is<EmailRecipientDto>(r => r.Address == "fr@example.com" && r.Locale == "fr"),
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task DispatchAsync_WithNoRecipients_ShouldEnqueueNothing()
    {
        await Dispatcher.DispatchAsync(new ClassifiedMessage(EnumMessageClass.Transactional), CancellationToken.None);

        _emailService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DispatchAsync_ShouldCarryTheMessageTemplateAndTokensThrough()
    {
        await Dispatcher.DispatchAsync(
            new ClassifiedMessage(EnumMessageClass.Transactional, Recipient()),
            CancellationToken.None
        );

        _emailService.Verify(
            e =>
                e.EnqueueAsync(
                    "AnyTemplate",
                    It.IsAny<EmailRecipientDto>(),
                    It.Is<IReadOnlyDictionary<string, string>>(t => t["userName"] == "Fally"),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }
}
