using _116.Shared.Domain;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Domain;

/// <summary>
/// Unit tests for <see cref="IDomainEvent"/> interface properties.
/// </summary>
public class DomainEventTests
{
    private record TestDomainEvent : DomainEvent
    {
        public string Message { get; init; } = string.Empty;
    }

    private record AnotherDomainEvent : DomainEvent
    {
        public int Value { get; init; }
    }

    [Fact]
    public void EventId_ShouldBeUniqueGuid()
    {
        // Arrange & Act
        IDomainEvent event1 = new TestDomainEvent();
        IDomainEvent event2 = new TestDomainEvent();

        // Assert
        event1.EventId.Should().NotBe(Guid.Empty);
        event2.EventId.Should().NotBe(Guid.Empty);
        event1.EventId.Should().NotBe(event2.EventId);
    }

    [Fact]
    public void OccurredOn_ShouldBeStampedInUtcAtConstruction()
    {
        // Arrange
        DateTime before = DateTime.UtcNow;

        // Act
        IDomainEvent domainEvent = new TestDomainEvent();

        // Assert
        domainEvent.OccurredOn.Should().BeOnOrAfter(before);
        domainEvent.OccurredOn.Should().BeOnOrBefore(DateTime.UtcNow);
        domainEvent.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void EventId_ReadTwice_ShouldReturnTheSameValue()
    {
        // Arrange
        IDomainEvent domainEvent = new TestDomainEvent();

        // Assert
        domainEvent.EventId.Should().Be(domainEvent.EventId);
    }

    [Fact]
    public void OccurredOn_ReadTwice_ShouldReturnTheSameValue()
    {
        // Arrange
        IDomainEvent domainEvent = new TestDomainEvent();

        // Assert
        domainEvent.OccurredOn.Should().Be(domainEvent.OccurredOn);
    }

    [Fact]
    public void EventType_ShouldReturnAssemblyQualifiedName()
    {
        // Arrange & Act
        IDomainEvent domainEvent = new TestDomainEvent();

        // Assert
        domainEvent.EventType.Should().NotBeNullOrEmpty();
        domainEvent.EventType.Should().Contain(nameof(TestDomainEvent));
        domainEvent.EventType.Should().Contain(","); // Assembly qualified name contains comma
    }

    [Fact]
    public void EventType_ShouldBeDifferentForDifferentEventTypes()
    {
        // Arrange & Act
        IDomainEvent event1 = new TestDomainEvent();
        IDomainEvent event2 = new AnotherDomainEvent();

        // Assert
        event1.EventType.Should().NotBe(event2.EventType);
        event1.EventType.Should().Contain(nameof(TestDomainEvent));
        event2.EventType.Should().Contain(nameof(AnotherDomainEvent));
    }

    [Fact]
    public void EventType_ShouldBeConsistentForSameType()
    {
        // Arrange & Act
        IDomainEvent event1 = new TestDomainEvent { Message = "First" };
        IDomainEvent event2 = new TestDomainEvent { Message = "Second" };

        // Assert
        event1.EventType.Should().Be(event2.EventType);
    }

    [Fact]
    public void DomainEvent_WithCustomProperties_ShouldPreserveValues()
    {
        // Arrange
        const string expectedMessage = "Test message";

        // Act
        var domainEvent = new TestDomainEvent { Message = expectedMessage };

        // Assert
        domainEvent.Message.Should().Be(expectedMessage);
    }

    [Fact]
    public void DomainEvent_WithMultipleProperties_ShouldPreserveAllValues()
    {
        // Arrange
        const int expectedValue = 42;

        // Act
        var domainEvent = new AnotherDomainEvent { Value = expectedValue };

        // Assert
        domainEvent.Value.Should().Be(expectedValue);
    }
}
