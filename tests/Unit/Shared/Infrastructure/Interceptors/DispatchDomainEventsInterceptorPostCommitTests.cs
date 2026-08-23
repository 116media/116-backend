using _116.Shared.Application.Services;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.interceptors;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Infrastructure.Interceptors;

/// <summary>
/// Unit tests for the post-commit behavior of <see cref="DispatchDomainEventsInterceptor"/>:
/// events dispatch only after a successful save, failed saves dispatch nothing
/// and leave no stale buffer, each event is published from a fresh
/// dependency injection scope in raise order, the caller's cancellation token
/// never reaches the handlers, and a dispatch that throws cannot fail the
/// committed save.
/// </summary>
public class DispatchDomainEventsInterceptorPostCommitTests
{
    private class TestAggregate : Aggregate<Guid>
    {
        public string Name { get; set; } = string.Empty;

        public static TestAggregate Create(Guid id, string name)
        {
            return new TestAggregate { Id = id, Name = name };
        }
    }

    private record TestDomainEvent(Guid AggregateId) : IDomainEvent;

    private class TestDbContext : DbContext
    {
        public DbSet<TestAggregate> Aggregates { get; set; } = null!;

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options) { }
    }

    private sealed class RecordingPublisher(Action<IDomainEvent>? onPublish) : IDomainEventPublisher
    {
        public List<IDomainEvent> PublishedEvents { get; } = [];

        public List<CancellationToken> ReceivedTokens { get; } = [];

        public Task Publish(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            PublishedEvents.Add(domainEvent);
            ReceivedTokens.Add(cancellationToken);
            onPublish?.Invoke(domainEvent);
            return Task.CompletedTask;
        }
    }

    private static (TestDbContext context, List<RecordingPublisher> scopedPublishers) CreateContext(
        string databaseName,
        Action<IDomainEvent>? onPublish = null
    )
    {
        var scopedPublishers = new List<RecordingPublisher>();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IDomainEventPublisher>(_ =>
        {
            var publisher = new RecordingPublisher(onPublish);
            scopedPublishers.Add(publisher);
            return publisher;
        });

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        var interceptor = new DispatchDomainEventsInterceptor(
            serviceScopeFactory,
            NullLogger<DispatchDomainEventsInterceptor>.Instance
        );

        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(interceptor)
            .Options;

        return (new TestDbContext(options), scopedPublishers);
    }

    private static TestDbContext CreateVerificationContext(string databaseName)
    {
        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public void SaveChanges_WithDomainEvents_PublishesOnlyAfterRowIsPersisted()
    {
        // Arrange
        string databaseName = Guid.NewGuid().ToString();
        var aggregateId = Guid.NewGuid();
        bool rowVisibleAtPublishTime = false;

        var (context, scopedPublishers) = CreateContext(
            databaseName,
            onPublish: _ =>
            {
                using TestDbContext verificationContext = CreateVerificationContext(databaseName);
                rowVisibleAtPublishTime = verificationContext.Aggregates.AsNoTracking().Any(a => a.Id == aggregateId);
            }
        );

        var aggregate = TestAggregate.Create(aggregateId, "Test");
        aggregate.AddDomainEvent(new TestDomainEvent(aggregateId));

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        scopedPublishers.SelectMany(p => p.PublishedEvents).Should().ContainSingle();
        rowVisibleAtPublishTime.Should().BeTrue();
    }

    [Fact]
    public void SaveChanges_WhenSaveFails_DispatchesNothingAndClearsEvents()
    {
        // Arrange
        string databaseName = Guid.NewGuid().ToString();
        var aggregateId = Guid.NewGuid();

        var (seedContext, _) = CreateContext(databaseName);
        seedContext.Aggregates.Add(TestAggregate.Create(aggregateId, "Existing"));
        seedContext.SaveChanges();

        var (context, scopedPublishers) = CreateContext(databaseName);
        var duplicate = TestAggregate.Create(aggregateId, "Duplicate");
        duplicate.AddDomainEvent(new TestDomainEvent(aggregateId));
        context.Aggregates.Add(duplicate);

        // Act
        Exception? exception = Record.Exception(() => context.SaveChanges());

        // Assert
        exception.Should().NotBeNull();
        scopedPublishers.SelectMany(p => p.PublishedEvents).Should().BeEmpty();
        duplicate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenSaveFails_DispatchesNothingAndClearsEvents()
    {
        // Arrange
        string databaseName = Guid.NewGuid().ToString();
        var aggregateId = Guid.NewGuid();

        var (seedContext, _) = CreateContext(databaseName);
        seedContext.Aggregates.Add(TestAggregate.Create(aggregateId, "Existing"));
        await seedContext.SaveChangesAsync();

        var (context, scopedPublishers) = CreateContext(databaseName);
        var duplicate = TestAggregate.Create(aggregateId, "Duplicate");
        duplicate.AddDomainEvent(new TestDomainEvent(aggregateId));
        context.Aggregates.Add(duplicate);

        // Act
        Exception? exception = await Record.ExceptionAsync(() => context.SaveChangesAsync());

        // Assert
        exception.Should().NotBeNull();
        scopedPublishers.SelectMany(p => p.PublishedEvents).Should().BeEmpty();
        duplicate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void SaveChanges_AfterFailedSave_DoesNotDispatchDiscardedEvents()
    {
        // Arrange
        string databaseName = Guid.NewGuid().ToString();
        var aggregateId = Guid.NewGuid();

        var (seedContext, _) = CreateContext(databaseName);
        seedContext.Aggregates.Add(TestAggregate.Create(aggregateId, "Existing"));
        seedContext.SaveChanges();

        var (context, scopedPublishers) = CreateContext(databaseName);
        var duplicate = TestAggregate.Create(aggregateId, "Duplicate");
        duplicate.AddDomainEvent(new TestDomainEvent(aggregateId));
        context.Aggregates.Add(duplicate);
        Record.Exception(() => context.SaveChanges()).Should().NotBeNull();

        // Act
        context.Entry(duplicate).State = EntityState.Detached;
        context.Aggregates.Add(TestAggregate.Create(Guid.NewGuid(), "Fresh"));
        context.SaveChanges();

        // Assert
        scopedPublishers.SelectMany(p => p.PublishedEvents).Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenCanceled_DiscardsTheCollectedEventsSoALaterSaveDispatchesNothing()
    {
        // Arrange
        string databaseName = Guid.NewGuid().ToString();
        var (context, scopedPublishers) = CreateContext(databaseName);

        var canceledAggregate = TestAggregate.Create(Guid.NewGuid(), "Canceled");
        canceledAggregate.AddDomainEvent(new TestDomainEvent(canceledAggregate.Id));
        context.Aggregates.Add(canceledAggregate);

        using var tokenSource = new CancellationTokenSource();
        await tokenSource.CancelAsync();

        // Act
        Exception? exception = await Record.ExceptionAsync(() => context.SaveChangesAsync(tokenSource.Token));

        context.ChangeTracker.Clear();
        context.Aggregates.Add(TestAggregate.Create(Guid.NewGuid(), "Fresh"));
        await context.SaveChangesAsync();

        // Assert
        exception.Should().BeAssignableTo<OperationCanceledException>();
        scopedPublishers.SelectMany(p => p.PublishedEvents).Should().BeEmpty();
    }

    [Fact]
    public void SaveChanges_WithMultipleEvents_DispatchesEachEventInAFreshScopeInRaiseOrder()
    {
        // Arrange
        string databaseName = Guid.NewGuid().ToString();
        var (context, scopedPublishers) = CreateContext(databaseName);

        var aggregate = TestAggregate.Create(Guid.NewGuid(), "Test");
        var firstEvent = new TestDomainEvent(aggregate.Id);
        var secondEvent = new TestDomainEvent(aggregate.Id);
        aggregate.AddDomainEvent(firstEvent);
        aggregate.AddDomainEvent(secondEvent);

        // Act
        context.Aggregates.Add(aggregate);
        context.SaveChanges();

        // Assert
        scopedPublishers.Should().HaveCount(2);
        scopedPublishers[0].PublishedEvents.Should().ContainSingle().Which.Should().BeSameAs(firstEvent);
        scopedPublishers[1].PublishedEvents.Should().ContainSingle().Which.Should().BeSameAs(secondEvent);
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleAggregates_DispatchesEventsInCollectionOrder()
    {
        // Arrange
        string databaseName = Guid.NewGuid().ToString();
        var (context, scopedPublishers) = CreateContext(databaseName);

        var firstAggregate = TestAggregate.Create(Guid.NewGuid(), "First");
        var secondAggregate = TestAggregate.Create(Guid.NewGuid(), "Second");
        var firstEvent = new TestDomainEvent(firstAggregate.Id);
        var secondEvent = new TestDomainEvent(secondAggregate.Id);
        firstAggregate.AddDomainEvent(firstEvent);
        secondAggregate.AddDomainEvent(secondEvent);

        // Act
        context.Aggregates.AddRange(firstAggregate, secondAggregate);
        await context.SaveChangesAsync();

        // Assert
        scopedPublishers.Should().HaveCount(2);
        scopedPublishers
            .SelectMany(p => p.PublishedEvents)
            .Should()
            .ContainInOrder(firstEvent, secondEvent)
            .And.HaveCount(2);
    }

    [Fact]
    public async Task SaveChangesAsync_WithCancelableCallerToken_DispatchesWithATokenTheCallerCannotCancel()
    {
        // Arrange
        string databaseName = Guid.NewGuid().ToString();
        var (context, scopedPublishers) = CreateContext(databaseName);

        var aggregate = TestAggregate.Create(Guid.NewGuid(), "Test");
        aggregate.AddDomainEvent(new TestDomainEvent(aggregate.Id));
        context.Aggregates.Add(aggregate);

        using var requestTokenSource = new CancellationTokenSource();

        // Act
        await context.SaveChangesAsync(requestTokenSource.Token);
        await requestTokenSource.CancelAsync();

        // Assert
        CancellationToken dispatchToken = scopedPublishers
            .SelectMany(p => p.ReceivedTokens)
            .Should()
            .ContainSingle()
            .Subject;
        dispatchToken.Should().NotBe(requestTokenSource.Token);
        dispatchToken.CanBeCanceled.Should().BeFalse();
        dispatchToken.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenPublisherResolutionThrows_LogsAndLeavesTheSaveSuccessful()
    {
        // Arrange
        string databaseName = Guid.NewGuid().ToString();
        var loggerMock = new Mock<ILogger<DispatchDomainEventsInterceptor>>();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IDomainEventPublisher>(_ =>
            throw new InvalidOperationException("Publisher resolution failure")
        );

        ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
        var interceptor = new DispatchDomainEventsInterceptor(
            serviceProvider.GetRequiredService<IServiceScopeFactory>(),
            loggerMock.Object
        );

        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName)
            .AddInterceptors(interceptor)
            .Options;

        await using var context = new TestDbContext(options);
        var aggregateId = Guid.NewGuid();
        var aggregate = TestAggregate.Create(aggregateId, "Test");
        aggregate.AddDomainEvent(new TestDomainEvent(aggregateId));
        context.Aggregates.Add(aggregate);

        // Act
        Exception? exception = await Record.ExceptionAsync(() => context.SaveChangesAsync());

        // Assert
        exception.Should().BeNull();

        await using TestDbContext verificationContext = CreateVerificationContext(databaseName);
        verificationContext.Aggregates.AsNoTracking().Any(a => a.Id == aggregateId).Should().BeTrue();

        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(nameof(TestDomainEvent))),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
