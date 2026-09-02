using _116.Shared.Domain;
using _116.Shared.Infrastructure.Outbox;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure.Outbox;

/// <summary>
/// Unit tests for <see cref="OutboxEventEntity" />: the row carries an event durably and hands
/// back the same event — same identity included — when replay rebuilds it.
/// </summary>
public class OutboxEventEntityTests
{
    /// <summary>
    /// A domain event with a payload, used to prove the round trip keeps both payload and stamp.
    /// </summary>
    /// <param name="ArticleId">The article the event refers to.</param>
    /// <param name="Reason">Why the event was raised.</param>
    private sealed record TestArticleEvent(Guid ArticleId, string Reason) : DomainEvent;

    [Fact]
    public void Create_ShouldCarryTheEventIdentityOntoTheRow()
    {
        // Arrange
        var domainEvent = new TestArticleEvent(Guid.NewGuid(), "published");

        // Act
        OutboxEventEntity row = OutboxEventEntity.Create(domainEvent);

        // Assert
        row.Id.Should().Be(domainEvent.EventId);
        row.OccurredOn.Should().Be(domainEvent.OccurredOn);
        row.EventType.Should().Be(domainEvent.EventType);
        row.DispatchedAt.Should().BeNull();
    }

    [Fact]
    public void ToDomainEvent_ShouldRebuildTheEventWithTheIdentityItWasRaisedWith()
    {
        // Arrange
        // A rebuilt event that minted a fresh id would never match its own processed-event row,
        // so every non-idempotent handler would run again on replay.
        var domainEvent = new TestArticleEvent(Guid.NewGuid(), "published");
        OutboxEventEntity row = OutboxEventEntity.Create(domainEvent);

        // Act
        IDomainEvent? rebuilt = row.ToDomainEvent();

        // Assert
        rebuilt.Should().NotBeNull();
        rebuilt!.EventId.Should().Be(domainEvent.EventId);
        rebuilt.OccurredOn.Should().Be(domainEvent.OccurredOn);
        rebuilt.Should().BeOfType<TestArticleEvent>();
        ((TestArticleEvent)rebuilt).ArticleId.Should().Be(domainEvent.ArticleId);
        ((TestArticleEvent)rebuilt).Reason.Should().Be(domainEvent.Reason);
    }

    [Fact]
    public void ToDomainEvent_RebuiltTwice_ShouldYieldTheSameIdentityBothTimes()
    {
        // Arrange
        var domainEvent = new TestArticleEvent(Guid.NewGuid(), "published");
        OutboxEventEntity row = OutboxEventEntity.Create(domainEvent);

        // Act
        IDomainEvent? first = row.ToDomainEvent();
        IDomainEvent? second = row.ToDomainEvent();

        // Assert
        first!.EventId.Should().Be(second!.EventId);
    }
}
