using _116.Shared.Domain;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Domain;

/// <summary>
/// Unit tests for <see cref="Aggregate{TId}"/>.
/// </summary>
public class AggregateTests
{
    private class TestAggregate : Aggregate<Guid>
    {
        public static TestAggregate Create(Guid id)
        {
            return new TestAggregate { Id = id };
        }
    }

    private class TestDomainEvent : IDomainEvent
    {
        public string Message { get; init; } = string.Empty;
    }

    [Fact]
    public void DomainEvents_InitiallyEmpty_ShouldReturnEmptyList()
    {
        // Arrange
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid());

        // Act
        IReadOnlyList<IDomainEvent> events = aggregate.DomainEvents;

        // Assert
        events.Should().NotBeNull();
        events.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_WithValidEvent_ShouldAddToList()
    {
        // Arrange
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid());
        var domainEvent = new TestDomainEvent { Message = "Test event" };

        // Act
        aggregate.AddDomainEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void AddDomainEvent_WithMultipleEvents_ShouldAddAllInOrder()
    {
        // Arrange
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid());
        var event1 = new TestDomainEvent { Message = "First event" };
        var event2 = new TestDomainEvent { Message = "Second event" };
        var event3 = new TestDomainEvent { Message = "Third event" };

        // Act
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);
        aggregate.AddDomainEvent(event3);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(3);
        aggregate.DomainEvents[0].Should().Be(event1);
        aggregate.DomainEvents[1].Should().Be(event2);
        aggregate.DomainEvents[2].Should().Be(event3);
    }

    [Fact]
    public void ClearDomainEvents_WithEvents_ShouldReturnEventsAndClearList()
    {
        // Arrange
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid());
        var event1 = new TestDomainEvent { Message = "Event 1" };
        var event2 = new TestDomainEvent { Message = "Event 2" };
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);

        // Act
        IDomainEvent[] clearedEvents = aggregate.ClearDomainEvents();

        // Assert
        clearedEvents.Should().HaveCount(2);
        clearedEvents[0].Should().Be(event1);
        clearedEvents[1].Should().Be(event2);
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WithNoEvents_ShouldReturnEmptyArray()
    {
        // Arrange
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid());

        // Act
        IDomainEvent[] clearedEvents = aggregate.ClearDomainEvents();

        // Assert
        clearedEvents.Should().NotBeNull();
        clearedEvents.Should().BeEmpty();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_CalledMultipleTimes_ShouldReturnEmptyAfterFirst()
    {
        // Arrange
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid());
        aggregate.AddDomainEvent(new TestDomainEvent { Message = "Event" });

        // Act
        IDomainEvent[] firstClear = aggregate.ClearDomainEvents();
        IDomainEvent[] secondClear = aggregate.ClearDomainEvents();

        // Assert
        firstClear.Should().HaveCount(1);
        secondClear.Should().BeEmpty();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldReturnReadOnlyList()
    {
        // Arrange
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid());
        aggregate.AddDomainEvent(new TestDomainEvent { Message = "Event" });

        // Act
        IReadOnlyList<IDomainEvent> events = aggregate.DomainEvents;

        // Assert
        events.Should().BeAssignableTo<IReadOnlyList<IDomainEvent>>();
        // Verify it's truly readonly by checking type
        events.GetType().Name.Should().Contain("ReadOnly");
    }

    [Fact]
    public void AddDomainEvent_AfterClear_ShouldStartFresh()
    {
        // Arrange
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid());
        aggregate.AddDomainEvent(new TestDomainEvent { Message = "Old event" });
        aggregate.ClearDomainEvents();

        // Act
        var newEvent = new TestDomainEvent { Message = "New event" };
        aggregate.AddDomainEvent(newEvent);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents[0].Should().Be(newEvent);
    }
}
