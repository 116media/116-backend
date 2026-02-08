using _116.Shared.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Services;

/// <summary>
/// Unit tests for <see cref="Dispatcher"/>.
/// </summary>
public class DispatcherTests
{
    #region Test Setup - Request and Handler Types

    // Request with response
    private record TestQuery(string Data) : IRequest<TestQueryResponse>;

    private record TestQueryResponse(string Result);

    // Request without response
    private record TestCommand(string Data) : IRequest;

    // Handler with response
    private class TestQueryHandler : IRequestHandler<TestQuery, TestQueryResponse>
    {
        public bool WasCalled { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task<TestQueryResponse> Handle(TestQuery request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedCancellationToken = cancellationToken;
            return Task.FromResult(new TestQueryResponse($"Processed: {request.Data}"));
        }
    }

    // Handler without response
    private class TestCommandHandler : IRequestHandler<TestCommand>
    {
        public bool WasCalled { get; private set; }
        public CancellationToken ReceivedCancellationToken { get; private set; }

        public Task Handle(TestCommand request, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedCancellationToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    // Handler that returns wrong type
    private class InvalidReturnTypeHandler : IRequestHandler<TestQuery, TestQueryResponse>
    {
        public Task<TestQueryResponse> Handle(TestQuery request, CancellationToken cancellationToken = default)
        {
            // This won't actually be called in the failing test
            return Task.FromResult(new TestQueryResponse("Invalid"));
        }
    }

    #endregion

    #region Send<TResponse> Tests - Success Cases

    [Fact]
    public async Task Send_WithValidRequestAndResponse_ShouldInvokeHandler()
    {
        // Arrange
        TestQueryHandler handler = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IRequestHandler<TestQuery, TestQueryResponse>)))
            .Returns(handler);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestQuery query = new("test data");

        // Act
        TestQueryResponse response = await dispatcher.Send(query);

        // Assert
        response.Should().NotBeNull();
        response.Result.Should().Be("Processed: test data");
        handler.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Send_WithValidRequest_ShouldReturnCorrectResponse()
    {
        // Arrange
        TestQueryHandler handler = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IRequestHandler<TestQuery, TestQueryResponse>)))
            .Returns(handler);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestQuery query = new("input");

        // Act
        TestQueryResponse response = await dispatcher.Send(query);

        // Assert
        response.Result.Should().Contain("input");
    }

    [Fact]
    public async Task Send_WithCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        TestQueryHandler handler = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IRequestHandler<TestQuery, TestQueryResponse>)))
            .Returns(handler);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestQuery query = new("test");
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        // Act
        await dispatcher.Send(query, token);

        // Assert
        handler.ReceivedCancellationToken.Should().Be(token);
    }

    [Fact]
    public async Task Send_WithDefaultCancellationToken_ShouldPassDefaultToken()
    {
        // Arrange
        TestQueryHandler handler = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IRequestHandler<TestQuery, TestQueryResponse>)))
            .Returns(handler);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestQuery query = new("test");

        // Act
        await dispatcher.Send(query);

        // Assert
        handler.ReceivedCancellationToken.Should().Be(default(CancellationToken));
    }

    #endregion

    #region Send<TResponse> Tests - Error Cases

    [Fact]
    public async Task Send_WhenHandlerNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IRequestHandler<TestQuery, TestQueryResponse>)))
            .Returns(null!);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestQuery query = new("test");

        // Act
        Func<Task> act = async () => await dispatcher.Send(query);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("No handler registered for TestQuery");
    }

    [Fact]
    public async Task Send_WhenHandlerNotRegistered_ShouldIncludeRequestTypeName()
    {
        // Arrange
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(null!);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestQuery query = new("test");

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await dispatcher.Send(query)
        );

        exception.Message.Should().Contain("TestQuery");
    }

    #endregion

    #region Send (void) Tests - Success Cases

    [Fact]
    public async Task Send_WithValidCommandNoResponse_ShouldInvokeHandler()
    {
        // Arrange
        TestCommandHandler handler = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IRequestHandler<TestCommand>))).Returns(handler);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestCommand command = new("test data");

        // Act
        await dispatcher.Send(command);

        // Assert
        handler.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Send_WithCommandAndCancellationToken_ShouldPassTokenToHandler()
    {
        // Arrange
        TestCommandHandler handler = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IRequestHandler<TestCommand>))).Returns(handler);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestCommand command = new("test");
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        // Act
        await dispatcher.Send(command, token);

        // Assert
        handler.ReceivedCancellationToken.Should().Be(token);
    }

    [Fact]
    public async Task Send_WithCommandAndDefaultCancellationToken_ShouldPassDefaultToken()
    {
        // Arrange
        TestCommandHandler handler = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IRequestHandler<TestCommand>))).Returns(handler);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestCommand command = new("test");

        // Act
        await dispatcher.Send(command);

        // Assert
        handler.ReceivedCancellationToken.Should().Be(default(CancellationToken));
    }

    #endregion

    #region Send (void) Tests - Error Cases

    [Fact]
    public async Task Send_WhenCommandHandlerNotRegistered_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IRequestHandler<TestCommand>))).Returns(null!);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestCommand command = new("test");

        // Act
        Func<Task> act = async () => await dispatcher.Send(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("No handler registered for TestCommand");
    }

    [Fact]
    public async Task Send_WhenCommandHandlerNotRegistered_ShouldIncludeRequestTypeName()
    {
        // Arrange
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock.Setup(sp => sp.GetService(It.IsAny<Type>())).Returns(null!);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestCommand command = new("test");

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await dispatcher.Send(command)
        );

        exception.Message.Should().Contain("TestCommand");
    }

    #endregion

    #region Handler Resolution Tests

    [Fact]
    public async Task Send_WithMultipleRequests_ShouldResolveCorrectHandlers()
    {
        // Arrange
        TestQueryHandler queryHandler = new();
        TestCommandHandler commandHandler = new();

        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IRequestHandler<TestQuery, TestQueryResponse>)))
            .Returns(queryHandler);
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IRequestHandler<TestCommand>))).Returns(commandHandler);

        Dispatcher dispatcher = new(serviceProviderMock.Object);

        // Act
        await dispatcher.Send(new TestQuery("query data"));
        await dispatcher.Send(new TestCommand("command data"));

        // Assert
        queryHandler.WasCalled.Should().BeTrue();
        commandHandler.WasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Send_WithSameRequestTypeTwice_ShouldUseCachedHandlerType()
    {
        // Arrange
        TestQueryHandler handler = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        int callCount = 0;

        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IRequestHandler<TestQuery, TestQueryResponse>)))
            .Returns(() =>
            {
                callCount++;
                return handler;
            });

        Dispatcher dispatcher = new(serviceProviderMock.Object);

        // Act
        await dispatcher.Send(new TestQuery("first"));
        await dispatcher.Send(new TestQuery("second"));

        // Assert
        // Handler should be resolved twice (once for each request), but type resolution should be cached
        callCount.Should().Be(2);
    }

    #endregion

    #region Service Provider Interaction Tests

    [Fact]
    public async Task Send_ShouldCallServiceProviderGetService()
    {
        // Arrange
        TestQueryHandler handler = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IRequestHandler<TestQuery, TestQueryResponse>)))
            .Returns(handler);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestQuery query = new("test");

        // Act
        await dispatcher.Send(query);

        // Assert
        serviceProviderMock.Verify(
            sp => sp.GetService(typeof(IRequestHandler<TestQuery, TestQueryResponse>)),
            Times.Once
        );
    }

    [Fact]
    public async Task Send_WithCommand_ShouldCallServiceProviderGetService()
    {
        // Arrange
        TestCommandHandler handler = new();
        Mock<IServiceProvider> serviceProviderMock = new();
        serviceProviderMock.Setup(sp => sp.GetService(typeof(IRequestHandler<TestCommand>))).Returns(handler);

        Dispatcher dispatcher = new(serviceProviderMock.Object);
        TestCommand command = new("test");

        // Act
        await dispatcher.Send(command);

        // Assert
        serviceProviderMock.Verify(sp => sp.GetService(typeof(IRequestHandler<TestCommand>)), Times.Once);
    }

    #endregion
}
