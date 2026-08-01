using _116.Shared.Application.Services;
using _116.Shared.Domain;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Services;

/// <summary>
/// Unit tests for <see cref="DomainEventPublisher"/>: handler fan-out in
/// registration order, compiled-delegate dispatch, swallow-and-log on handler
/// failure, isolation of a handler that cannot be constructed, the debug trace
/// for events without handlers, and the dispatch depth cap for reentrant
/// publications.
/// </summary>
public class DomainEventPublisherTests
{
    public record TestDomainEvent(Guid AggregateId) : IDomainEvent;

    public record OtherDomainEvent(Guid AggregateId) : IDomainEvent;

    private sealed class DelegatingTestHandler(Func<TestDomainEvent, CancellationToken, Task> onHandle)
        : IDomainEventHandler<TestDomainEvent>
    {
        public Task Handle(TestDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            return onHandle(domainEvent, cancellationToken);
        }
    }

    private interface IUnregisteredDependency;

    private sealed class UnconstructableTestHandler(IUnregisteredDependency dependency)
        : IDomainEventHandler<TestDomainEvent>
    {
        public Task Handle(TestDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(dependency);
        }
    }

    private static DomainEventPublisher CreatePublisher(
        Action<ServiceCollection> configureServices,
        Mock<ILogger<DomainEventPublisher>>? loggerMock = null
    )
    {
        var services = new ServiceCollection();
        configureServices(services);

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        ILogger<DomainEventPublisher> logger = (loggerMock ?? new Mock<ILogger<DomainEventPublisher>>()).Object;

        return new DomainEventPublisher(serviceProvider, logger, new DomainEventHandlerRegistry(services));
    }

    [Fact]
    public async Task Publish_WithSingleHandler_InvokesHandlerWithEventAndToken()
    {
        // Arrange
        var handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        DomainEventPublisher publisher = CreatePublisher(services =>
            services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => handlerMock.Object)
        );

        var domainEvent = new TestDomainEvent(Guid.NewGuid());
        using var tokenSource = new CancellationTokenSource();

        // Act
        await publisher.Publish(domainEvent, tokenSource.Token);

        // Assert
        handlerMock.Verify(h => h.Handle(domainEvent, tokenSource.Token), Times.Once);
    }

    [Fact]
    public async Task Publish_WithMultipleHandlers_InvokesAllInRegistrationOrder()
    {
        // Arrange
        var invocationOrder = new List<string>();
        DomainEventPublisher publisher = CreatePublisher(services =>
        {
            services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => new DelegatingTestHandler(
                (_, _) =>
                {
                    invocationOrder.Add("first");
                    return Task.CompletedTask;
                }
            ));
            services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => new DelegatingTestHandler(
                (_, _) =>
                {
                    invocationOrder.Add("second");
                    return Task.CompletedTask;
                }
            ));
        });

        // Act
        await publisher.Publish(new TestDomainEvent(Guid.NewGuid()));

        // Assert
        invocationOrder.Should().Equal("first", "second");
    }

    [Fact]
    public async Task Publish_WithNoRegisteredHandlers_CompletesWithoutError()
    {
        // Arrange
        DomainEventPublisher publisher = CreatePublisher(_ => { });

        // Act
        Exception? exception = await Record.ExceptionAsync(() =>
            publisher.Publish(new TestDomainEvent(Guid.NewGuid()))
        );

        // Assert
        exception.Should().BeNull();
    }

    [Fact]
    public async Task Publish_WhenHandlerThrows_SwallowsLogsErrorAndRunsRemainingHandlers()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DomainEventPublisher>>();
        bool secondHandlerRan = false;

        DomainEventPublisher publisher = CreatePublisher(
            services =>
            {
                services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => new DelegatingTestHandler(
                    (_, _) => throw new InvalidOperationException("Handler failure")
                ));
                services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => new DelegatingTestHandler(
                    (_, _) =>
                    {
                        secondHandlerRan = true;
                        return Task.CompletedTask;
                    }
                ));
            },
            loggerMock
        );

        // Act
        Exception? exception = await Record.ExceptionAsync(() =>
            publisher.Publish(new TestDomainEvent(Guid.NewGuid()))
        );

        // Assert
        exception.Should().BeNull();
        secondHandlerRan.Should().BeTrue();
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

    [Fact]
    public async Task Publish_DispatchesOnlyToHandlersOfTheEventType()
    {
        // Arrange
        var testEventHandlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        var otherEventHandlerMock = new Mock<IDomainEventHandler<OtherDomainEvent>>();

        DomainEventPublisher publisher = CreatePublisher(services =>
        {
            services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => testEventHandlerMock.Object);
            services.AddScoped<IDomainEventHandler<OtherDomainEvent>>(_ => otherEventHandlerMock.Object);
        });

        var domainEvent = new OtherDomainEvent(Guid.NewGuid());

        // Act
        await publisher.Publish(domainEvent);

        // Assert
        otherEventHandlerMock.Verify(h => h.Handle(domainEvent, It.IsAny<CancellationToken>()), Times.Once);
        testEventHandlerMock.Verify(
            h => h.Handle(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Publish_RepeatedPublishesOfSameEventType_InvokeHandlerEachTime()
    {
        // Arrange
        var handlerMock = new Mock<IDomainEventHandler<TestDomainEvent>>();
        DomainEventPublisher publisher = CreatePublisher(services =>
            services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => handlerMock.Object)
        );

        // Act
        await publisher.Publish(new TestDomainEvent(Guid.NewGuid()));
        await publisher.Publish(new TestDomainEvent(Guid.NewGuid()));
        await publisher.Publish(new TestDomainEvent(Guid.NewGuid()));

        // Assert
        handlerMock.Verify(h => h.Handle(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Publish_ReentrantPublishBeyondDepthCap_DropsEventAndLogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DomainEventPublisher>>();
        int invocationCount = 0;

        DomainEventPublisher publisher = null!;
        publisher = CreatePublisher(
            services =>
                services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => new DelegatingTestHandler(
                    async (domainEvent, cancellationToken) =>
                    {
                        invocationCount++;
                        await publisher.Publish(domainEvent, cancellationToken);
                    }
                )),
            loggerMock
        );

        // Act
        await publisher.Publish(new TestDomainEvent(Guid.NewGuid()));

        // Assert
        invocationCount.Should().Be(DomainEventPublisher.MaxDispatchDepth);
        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("depth")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Publish_AfterDroppedReentrantPublish_DispatchesTopLevelEventsAgain()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DomainEventPublisher>>();
        int invocationCount = 0;

        DomainEventPublisher publisher = null!;
        publisher = CreatePublisher(
            services =>
                services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => new DelegatingTestHandler(
                    async (domainEvent, cancellationToken) =>
                    {
                        invocationCount++;
                        await publisher.Publish(domainEvent, cancellationToken);
                    }
                )),
            loggerMock
        );

        // Act
        await publisher.Publish(new TestDomainEvent(Guid.NewGuid()));
        await publisher.Publish(new TestDomainEvent(Guid.NewGuid()));

        // Assert
        invocationCount.Should().Be(DomainEventPublisher.MaxDispatchDepth * 2);
    }

    [Fact]
    public async Task Publish_WhenOneHandlerCannotBeConstructed_StillRunsItsSiblings()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DomainEventPublisher>>();
        bool siblingRan = false;

        DomainEventPublisher publisher = CreatePublisher(
            services =>
            {
                services.AddScoped<IDomainEventHandler<TestDomainEvent>, UnconstructableTestHandler>();
                services.AddScoped<IDomainEventHandler<TestDomainEvent>>(_ => new DelegatingTestHandler(
                    (_, _) =>
                    {
                        siblingRan = true;
                        return Task.CompletedTask;
                    }
                ));
            },
            loggerMock
        );

        // Act
        Exception? exception = await Record.ExceptionAsync(() =>
            publisher.Publish(new TestDomainEvent(Guid.NewGuid()))
        );

        // Assert
        exception.Should().BeNull();
        siblingRan.Should().BeTrue();
        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, _) =>
                            v.ToString()!.Contains(nameof(UnconstructableTestHandler))
                            && v.ToString()!.Contains(nameof(TestDomainEvent))
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Publish_WithNoRegisteredHandlers_LogsDebugNamingTheEventType()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<DomainEventPublisher>>();
        DomainEventPublisher publisher = CreatePublisher(_ => { }, loggerMock);

        // Act
        await publisher.Publish(new TestDomainEvent(Guid.NewGuid()));

        // Assert
        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(nameof(TestDomainEvent))),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}
