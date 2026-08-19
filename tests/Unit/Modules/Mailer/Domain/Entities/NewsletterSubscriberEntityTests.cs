using _116.Mailer.Domain.Entities;
using _116.Mailer.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Mailer.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="NewsletterSubscriberEntity" /> covering the double
/// opt-in lifecycle and its idempotent link semantics.
/// </summary>
public class NewsletterSubscriberEntityTests
{
    private static readonly DateTime Now = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Subscribe_ShouldStartPendingWithBothTokensAndLowercasedEmail()
    {
        var subscriber = NewsletterSubscriberEntity.Subscribe(id: Guid.NewGuid(), email: "  Fan@Example.COM ");

        subscriber.Email.Should().Be("fan@example.com");
        subscriber.Status.Should().Be(EnumNewsletterStatus.PendingConfirmation);
        subscriber.ConfirmationToken.Should().NotBeNullOrWhiteSpace();
        subscriber.UnsubscribeToken.Should().NotBeNullOrWhiteSpace();
        subscriber.ConfirmationToken.Should().NotBe(subscriber.UnsubscribeToken);
    }

    [Fact]
    public void Confirm_Pending_ShouldSubscribe()
    {
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");

        bool changed = subscriber.Confirm(now: Now);

        changed.Should().BeTrue();
        subscriber.Status.Should().Be(EnumNewsletterStatus.Subscribed);
        subscriber.ConfirmedAt.Should().Be(Now);
    }

    [Fact]
    public void Confirm_Twice_ShouldBeANoOp()
    {
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        subscriber.Confirm(now: Now);

        bool changed = subscriber.Confirm(now: Now.AddDays(1));

        changed.Should().BeFalse();
        subscriber.ConfirmedAt.Should().Be(Now);
    }

    [Fact]
    public void Confirm_AfterUnsubscribe_ShouldNotResubscribe()
    {
        // A stale confirmation link must never override an explicit opt-out.
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        subscriber.Confirm(now: Now);
        subscriber.Unsubscribe(now: Now);

        bool changed = subscriber.Confirm(now: Now.AddDays(1));

        changed.Should().BeFalse();
        subscriber.Status.Should().Be(EnumNewsletterStatus.Unsubscribed);
    }

    [Fact]
    public void Unsubscribe_Subscribed_ShouldOptOut()
    {
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        subscriber.Confirm(now: Now);

        bool changed = subscriber.Unsubscribe(now: Now.AddDays(2));

        changed.Should().BeTrue();
        subscriber.Status.Should().Be(EnumNewsletterStatus.Unsubscribed);
        subscriber.UnsubscribedAt.Should().Be(Now.AddDays(2));
    }

    [Fact]
    public void Unsubscribe_Twice_ShouldBeANoOp()
    {
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        subscriber.Confirm(now: Now);
        subscriber.Unsubscribe(now: Now);

        bool changed = subscriber.Unsubscribe(now: Now.AddDays(1));

        changed.Should().BeFalse();
        subscriber.UnsubscribedAt.Should().Be(Now);
    }

    [Fact]
    public void ReissueConfirmation_AfterUnsubscribe_ShouldRotateTokenAndGoPending()
    {
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        string firstToken = subscriber.ConfirmationToken;
        subscriber.Confirm(now: Now);
        subscriber.Unsubscribe(now: Now);

        subscriber.ReissueConfirmation();

        subscriber.Status.Should().Be(EnumNewsletterStatus.PendingConfirmation);
        subscriber.ConfirmationToken.Should().NotBe(firstToken);
        subscriber.ConfirmedAt.Should().BeNull();
        subscriber.UnsubscribedAt.Should().BeNull();
    }

    [Fact]
    public void ReissueConfirmation_WhileSubscribed_ShouldBeANoOp()
    {
        var subscriber = NewsletterSubscriberEntity.Subscribe(Guid.NewGuid(), "fan@example.com");
        subscriber.Confirm(now: Now);
        string token = subscriber.ConfirmationToken;

        subscriber.ReissueConfirmation();

        subscriber.Status.Should().Be(EnumNewsletterStatus.Subscribed);
        subscriber.ConfirmationToken.Should().Be(token);
    }
}
