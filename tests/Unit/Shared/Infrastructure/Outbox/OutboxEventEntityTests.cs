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

    [Fact]
    public void MarkDispatched_ShouldRetireTheRowFromReplay()
    {
        // Arrange
        OutboxEventEntity row = OutboxEventEntity.Create(new TestArticleEvent(Guid.NewGuid(), "published"));
        DateTime before = DateTime.UtcNow;

        // Act
        row.MarkDispatched();

        // Assert
        row.DispatchedAt.Should().NotBeNull();
        row.DispatchedAt!.Value.Should().BeOnOrAfter(before);
        row.LastError.Should().BeNull();
    }

    [Fact]
    public void MarkDispatched_AfterAFailure_ShouldClearTheRecordedError()
    {
        // Arrange
        OutboxEventEntity row = OutboxEventEntity.Create(new TestArticleEvent(Guid.NewGuid(), "published"));
        row.MarkFailed("transient provider failure");

        // Act
        row.MarkDispatched();

        // Assert
        row.LastError.Should().BeNull();
        row.DispatchedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkFailed_ShouldCountTheAttemptAndKeepTheRowReplayable()
    {
        // Arrange
        OutboxEventEntity row = OutboxEventEntity.Create(new TestArticleEvent(Guid.NewGuid(), "published"));

        // Act
        row.MarkFailed("handler threw");

        // Assert
        row.AttemptCount.Should().Be(1);
        row.LastError.Should().Be("handler threw");
        row.DispatchedAt.Should().BeNull();
    }

    [Fact]
    public void MarkFailed_Repeatedly_ShouldAccumulateAttemptsSoReplayCanGiveUp()
    {
        // Arrange
        OutboxEventEntity row = OutboxEventEntity.Create(new TestArticleEvent(Guid.NewGuid(), "published"));

        // Act
        row.MarkFailed("first");
        row.MarkFailed("second");
        row.MarkFailed("third");

        // Assert
        row.AttemptCount.Should().Be(3);
        row.LastError.Should().Be("third");
    }
}
