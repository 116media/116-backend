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
        var aggregate = TestAggregate.Create(Guid.NewGuid());

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
        var aggregate = TestAggregate.Create(Guid.NewGuid());
        var domainEvent = new TestDomainEvent { Message = "Test event" };

        // Act
        aggregate.AddDomainEvent(domainEvent);

        // Assert
        aggregate.DomainEvents.Should().ContainSingle();
        aggregate.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void AddDomainEvent_WithMultipleEvents_ShouldAddAllInOrder()
    {
        // Arrange
        var aggregate = TestAggregate.Create(Guid.NewGuid());
        var event1 = new TestDomainEvent { Message = "First event" };
        var event2 = new TestDomainEvent { Message = "Second event" };
        var event3 = new TestDomainEvent { Message = "Third event" };

        // Act
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);
        aggregate.AddDomainEvent(event3);

        // Assert
        aggregate.DomainEvents.Should().HaveCount(3);
        aggregate.DomainEvents.Should().HaveElementAt(0, event1);
        aggregate.DomainEvents.Should().HaveElementAt(1, event2);
        aggregate.DomainEvents.Should().HaveElementAt(2, event3);
    }

    [Fact]
    public void ClearDomainEvents_WithEvents_ShouldReturnEventsAndClearList()
    {
        // Arrange
        var aggregate = TestAggregate.Create(Guid.NewGuid());
        var event1 = new TestDomainEvent { Message = "Event 1" };
        var event2 = new TestDomainEvent { Message = "Event 2" };
        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);

        // Act
        IDomainEvent[] clearedEvents = aggregate.ClearDomainEvents();

        // Assert
        clearedEvents.Should().HaveCount(2);
        clearedEvents.Should().HaveElementAt(0, event1);
        clearedEvents.Should().HaveElementAt(1, event2);
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WithNoEvents_ShouldReturnEmptyArray()
    {
        // Arrange
        var aggregate = TestAggregate.Create(Guid.NewGuid());

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
        var aggregate = TestAggregate.Create(Guid.NewGuid());
        aggregate.AddDomainEvent(new TestDomainEvent { Message = "Event" });

        // Act
        IDomainEvent[] firstClear = aggregate.ClearDomainEvents();
        IDomainEvent[] secondClear = aggregate.ClearDomainEvents();

        // Assert
        firstClear.Should().ContainSingle();
        secondClear.Should().BeEmpty();
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldReturnReadOnlyList()
    {
        // Arrange
        var aggregate = TestAggregate.Create(Guid.NewGuid());
        aggregate.AddDomainEvent(new TestDomainEvent { Message = "Event" });

        // Act
        IReadOnlyList<IDomainEvent> events = aggregate.DomainEvents;

        // Assert
        events.Should().BeAssignableTo<IReadOnlyList<IDomainEvent>>();
        events.GetType().Name.Should().Contain("ReadOnly");
    }

    [Fact]
    public void AddDomainEvent_AfterClear_ShouldStartFresh()
    {
        // Arrange
        var aggregate = TestAggregate.Create(Guid.NewGuid());
        aggregate.AddDomainEvent(new TestDomainEvent { Message = "Old event" });
        aggregate.ClearDomainEvents();

        // Act
        var newEvent = new TestDomainEvent { Message = "New event" };
        aggregate.AddDomainEvent(newEvent);

        // Assert
        aggregate.DomainEvents.Should().ContainSingle();
        aggregate.DomainEvents.Should().HaveElementAt(0, newEvent);
    }
}
