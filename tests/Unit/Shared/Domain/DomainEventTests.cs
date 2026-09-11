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
    public void OccurredOn_AtConstruction_ShouldBeUnstampedUntilTheInterceptorCollectsIt()
    {
        // Arrange & Act
        IDomainEvent domainEvent = new TestDomainEvent();

        // Assert
        domainEvent.OccurredOn.Should().Be(default);
    }

    [Fact]
    public void OccurredOn_WhenStamped_ShouldSurviveTheCopyAndKeepThePayload()
    {
        // Arrange
        var raised = new TestDomainEvent { Message = "payload" };
        var stampedAt = new DateTime(2026, 9, 11, 10, 30, 0, DateTimeKind.Utc);

        // Act
        TestDomainEvent stamped = raised with
        {
            OccurredOn = stampedAt,
        };

        // Assert
        stamped.OccurredOn.Should().Be(stampedAt);
        stamped.EventId.Should().Be(raised.EventId);
        stamped.Message.Should().Be("payload");
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

    #region Equality

    [Fact]
    public void GetHashCode_ForTwoEventsOfOneType_ShouldMatch()
    {
        // Arrange
        IDomainEvent first = new TestDomainEvent();
        IDomainEvent second = new TestDomainEvent();

        // Assert
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact]
    public void Equals_ForTwoEventsOfOneType_ShouldIgnoreTheStampedIdentity()
    {
        // Arrange
        var first = new TestDomainEvent();
        var second = new TestDomainEvent();

        // Assert
        first.EventId.Should().NotBe(second.EventId);
        first.Should().Be(second);
    }

    [Fact]
    public void Equals_AgainstNull_ShouldBeFalse()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();

        // Assert
        domainEvent.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_AcrossEventTypes_ShouldBeFalse()
    {
        // Arrange
        DomainEvent first = new TestDomainEvent();
        DomainEvent second = new AnotherDomainEvent();

        // Assert
        first.Equals(second).Should().BeFalse();
        first.GetHashCode().Should().NotBe(second.GetHashCode());
    }

    #endregion
}
