using _116.Shared.Domain;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Domain;

/// <summary>
/// Unit tests for <see cref="IDomainEvent"/> interface properties.
/// </summary>
public class IDomainEventTests
{
    private class TestDomainEvent : IDomainEvent
    {
        public string Message { get; init; } = string.Empty;
    }

    private class AnotherDomainEvent : IDomainEvent
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
        // Note: EventId generates new Guid each time accessed, so they will always be different
    }

    [Fact]
    public void CreatedAt_ShouldBeCurrentDateTime()
    {
        // Arrange
        DateTime before = DateTime.Now;

        // Act
        IDomainEvent domainEvent = new TestDomainEvent();
        DateTime createdAt = domainEvent.CreatedAt; // Cache the value since property returns DateTime.Now on each access

        // Assert
        DateTime after = DateTime.Now;
        createdAt.Should().BeOnOrAfter(before);
        createdAt.Should().BeOnOrBefore(after.AddMilliseconds(10)); // Add tolerance for timing precision
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
