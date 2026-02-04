using _116.Shared.Application.Services;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.interceptors;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure.Interceptors;

/// <summary>
/// Unit tests for <see cref="DispatchDomainEventsInterceptor"/>.
/// </summary>
public class DispatchDomainEventsInterceptorTests
{
    private class TestAggregate : Aggregate<Guid>, IAggregate
    {
        public string Name { get; set; } = string.Empty;

        public static TestAggregate Create(Guid id, string name)
        {
            return new TestAggregate { Id = id, Name = name };
        }
    }

    private class TestDomainEvent : IDomainEvent
    {
        public string Message { get; init; } = string.Empty;
    }

    private class TestDbContext : DbContext
    {
        public DbSet<TestAggregate> Aggregates { get; set; } = null!;

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options) { }
    }

    private (TestDbContext context, Mock<IDomainEventPublisher> publisherMock) CreateTestContext()
    {
        var publisherMock = new Mock<IDomainEventPublisher>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped(_ => publisherMock.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var interceptor = new DispatchDomainEventsInterceptor(serviceScopeFactory);

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        var context = new TestDbContext(options);
        return (context, publisherMock);
    }

    [Fact]
    public void SavingChanges_WithDomainEvents_ShouldPublishEvents()
    {
        // Arrange
        var (context, publisherMock) = CreateTestContext();
        var aggregate = TestAggregate.Create(Guid.NewGuid(), "Test");
        var domainEvent = new TestDomainEvent { Message = "Test event" };
        aggregate.AddDomainEvent(domainEvent);

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        publisherMock.Verify(
            p => p.Publish(It.Is<IDomainEvent>(e => e == domainEvent), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public void SavingChanges_WithDomainEvents_ShouldClearEventsAfterPublish()
    {
        // Arrange
        var (context, publisherMock) = CreateTestContext();
        var aggregate = TestAggregate.Create(Guid.NewGuid(), "Test");
        aggregate.AddDomainEvent(new TestDomainEvent { Message = "Event" });

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SavingChanges_WithMultipleDomainEvents_ShouldPublishAllEvents()
    {
        // Arrange
        var (context, publisherMock) = CreateTestContext();
        var aggregate = TestAggregate.Create(Guid.NewGuid(), "Test");
        var event1 = new TestDomainEvent { Message = "Event 1" };
        var event2 = new TestDomainEvent { Message = "Event 2" };
        var event3 = new TestDomainEvent { Message = "Event 3" };

        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);
        aggregate.AddDomainEvent(event3);

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        publisherMock.Verify(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        publisherMock.Verify(p => p.Publish(event1, It.IsAny<CancellationToken>()), Times.Once);
        publisherMock.Verify(p => p.Publish(event2, It.IsAny<CancellationToken>()), Times.Once);
        publisherMock.Verify(p => p.Publish(event3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void SavingChanges_WithMultipleAggregates_ShouldPublishAllEvents()
    {
        // Arrange
        var (context, publisherMock) = CreateTestContext();
        var aggregate1 = TestAggregate.Create(Guid.NewGuid(), "Aggregate 1");
        var aggregate2 = TestAggregate.Create(Guid.NewGuid(), "Aggregate 2");

        var event1 = new TestDomainEvent { Message = "Event from aggregate 1" };
        var event2 = new TestDomainEvent { Message = "Event from aggregate 2" };

        aggregate1.AddDomainEvent(event1);
        aggregate2.AddDomainEvent(event2);

        // Act
        context.Aggregates.AddRange(aggregate1, aggregate2);
        context.SaveChanges();

        // Assert
        publisherMock.Verify(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        publisherMock.Verify(p => p.Publish(event1, It.IsAny<CancellationToken>()), Times.Once);
        publisherMock.Verify(p => p.Publish(event2, It.IsAny<CancellationToken>()), Times.Once);
        aggregate1.DomainEvents.Should().BeEmpty();
        aggregate2.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SavingChanges_WithNoDomainEvents_ShouldNotPublish()
    {
        // Arrange
        var (context, publisherMock) = CreateTestContext();
        var aggregate = TestAggregate.Create(Guid.NewGuid(), "Test");

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        publisherMock.Verify(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SavingChangesAsync_WithDomainEvents_ShouldPublishEvents()
    {
        // Arrange
        var (context, publisherMock) = CreateTestContext();
        var aggregate = TestAggregate.Create(Guid.NewGuid(), "Test");
        var domainEvent = new TestDomainEvent { Message = "Test event" };
        aggregate.AddDomainEvent(domainEvent);

        // Act
        context.Aggregates.Add(aggregate);
        await context.SaveChangesAsync();

        // Assert
        publisherMock.Verify(
            p => p.Publish(It.Is<IDomainEvent>(e => e == domainEvent), It.IsAny<CancellationToken>()),
            Times.Once
        );
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SavingChangesAsync_WithMultipleDomainEvents_ShouldPublishAllEvents()
    {
        // Arrange
        var (context, publisherMock) = CreateTestContext();
        var aggregate = TestAggregate.Create(Guid.NewGuid(), "Test");
        var event1 = new TestDomainEvent { Message = "Event 1" };
        var event2 = new TestDomainEvent { Message = "Event 2" };

        aggregate.AddDomainEvent(event1);
        aggregate.AddDomainEvent(event2);

        // Act
        context.Aggregates.Add(aggregate);
        await context.SaveChangesAsync();

        // Assert
        publisherMock.Verify(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        publisherMock.Verify(p => p.Publish(event1, It.IsAny<CancellationToken>()), Times.Once);
        publisherMock.Verify(p => p.Publish(event2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void SavingChanges_WithModifiedAggregate_ShouldPublishNewEvents()
    {
        // Arrange
        var (context, publisherMock) = CreateTestContext();
        var aggregate = TestAggregate.Create(Guid.NewGuid(), "Test");
        context.Aggregates.Add(aggregate);
        context.SaveChanges();
        publisherMock.Reset();

        // Add event after initial save
        var newEvent = new TestDomainEvent { Message = "New event" };
        aggregate.AddDomainEvent(newEvent);
        aggregate.Name = "Modified";

        // Act
        context.SaveChanges();

        // Assert
        publisherMock.Verify(p => p.Publish(newEvent, It.IsAny<CancellationToken>()), Times.Once);
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SavingChangesAsync_WithNoDomainEvents_ShouldNotPublish()
    {
        // Arrange
        var (context, publisherMock) = CreateTestContext();
        var aggregate = TestAggregate.Create(Guid.NewGuid(), "Test");

        // Act
        context.Aggregates.Add(aggregate);
        await context.SaveChangesAsync();

        // Assert
        publisherMock.Verify(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
