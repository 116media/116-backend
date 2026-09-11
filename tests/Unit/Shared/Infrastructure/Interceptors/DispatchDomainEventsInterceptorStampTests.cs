using _116.Shared.Application.Services;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.interceptors;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure.Interceptors;

/// <summary>
/// Unit tests for the occurrence stamp <see cref="DispatchDomainEventsInterceptor" /> applies:
/// the domain raises events without reading a clock, and the interceptor stamps each one from
/// the injected <see cref="TimeProvider" /> as it collects them.
/// </summary>
public class DispatchDomainEventsInterceptorStampTests
{
    private static readonly DateTime StartInstant = new(2026, 9, 11, 8, 0, 0, DateTimeKind.Utc);

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(StartInstant));

    private class TestAggregate : Aggregate<Guid>
    {
        public string Name { get; set; } = string.Empty;

        public static TestAggregate Create(Guid id, string name)
        {
            return new TestAggregate { Id = id, Name = name };
        }

        /// <summary>
        /// Records an event on this aggregate.
        /// </summary>
        /// <param name="domainEvent">The event to record.</param>
        public void Raise(IDomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }
    }

    private record TestDomainEvent(string Payload) : DomainEvent;

    private class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestAggregate> Aggregates { get; set; } = null!;
    }

    private sealed class RecordingPublisher : IDomainEventPublisher
    {
        public List<IDomainEvent> PublishedEvents { get; } = [];

        public Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            PublishedEvents.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Builds a context whose saves dispatch through the interceptor under test.
    /// </summary>
    /// <returns>The context and the publisher recording what it dispatched.</returns>
    private (TestDbContext Context, RecordingPublisher Publisher) CreateContext()
    {
        var publisher = new RecordingPublisher();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IDomainEventPublisher>(_ => publisher);

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        var interceptor = new DispatchDomainEventsInterceptor(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<DispatchDomainEventsInterceptor>.Instance,
            _time
        );

        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return (new TestDbContext(options), publisher);
    }

    [Fact]
    public void SaveChanges_ShouldStampOccurredOnFromTheInjectedClock()
    {
        // Arrange
        (TestDbContext context, RecordingPublisher publisher) = CreateContext();
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid(), "aggregate");
        aggregate.Raise(new TestDomainEvent("payload"));

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        publisher.PublishedEvents.Should().ContainSingle().Which.OccurredOn.Should().Be(StartInstant);
    }

    [Fact]
    public void SaveChanges_ShouldNotReadTheWallClock()
    {
        // Arrange
        (TestDbContext context, RecordingPublisher publisher) = CreateContext();
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid(), "aggregate");
        aggregate.Raise(new TestDomainEvent("payload"));
        _time.Advance(TimeSpan.FromDays(365));

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        publisher.PublishedEvents.Should().ContainSingle().Which.OccurredOn.Should().Be(StartInstant.AddDays(365));
    }

    [Fact]
    public void SaveChanges_ShouldPreserveTheEventIdAndPayloadThroughTheStamp()
    {
        // Arrange
        (TestDbContext context, RecordingPublisher publisher) = CreateContext();
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid(), "aggregate");
        var raised = new TestDomainEvent("payload");
        aggregate.Raise(raised);

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        IDomainEvent published = publisher.PublishedEvents.Should().ContainSingle().Subject;
        published.EventId.Should().Be(raised.EventId);
        published.Should().BeOfType<TestDomainEvent>().Which.Payload.Should().Be("payload");
    }

    [Fact]
    public void SaveChanges_WithAnEventThatAlreadyCarriesAStamp_ShouldLeaveItUntouched()
    {
        // Arrange
        (TestDbContext context, RecordingPublisher publisher) = CreateContext();
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid(), "aggregate");
        var replayedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        aggregate.Raise(new TestDomainEvent("payload") { OccurredOn = replayedAt });

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        publisher.PublishedEvents.Should().ContainSingle().Which.OccurredOn.Should().Be(replayedAt);
    }

    [Fact]
    public void SaveChanges_WithSeveralEvents_ShouldStampThemAllWithOneReadOfTheClock()
    {
        // Arrange
        (TestDbContext context, RecordingPublisher publisher) = CreateContext();
        TestAggregate aggregate = TestAggregate.Create(Guid.NewGuid(), "aggregate");
        aggregate.Raise(new TestDomainEvent("first"));
        aggregate.Raise(new TestDomainEvent("second"));

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        publisher.PublishedEvents.Should().HaveCount(2);
        publisher.PublishedEvents.Should().OnlyContain(published => published.OccurredOn == StartInstant);
    }
}
