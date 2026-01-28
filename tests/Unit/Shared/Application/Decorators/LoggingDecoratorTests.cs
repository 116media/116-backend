using _116.Shared.Application.Decorators;
using _116.Shared.Contracts.Application.CQRS;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Decorators;

/// <summary>
/// Unit tests for <see cref="LoggingDecorator{TRequest, TResponse}"/>.
/// </summary>
public class LoggingDecoratorTests
{
    #region Test Setup

    public record TestRequest(string Value) : IRequest<TestResponse>;

    public record TestResponse(string Result);

    #endregion

    #region Success Cases - Basic Logging

    [Fact]
    public async Task Handle_ShouldCallHandler()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        TestRequest request = new("test");

        // Act
        TestResponse response = await decorator.Handle(request);

        // Assert
        response.Should().NotBeNull();
        response.Result.Should().Be("Success");
        handlerMock.Verify(h => h.Handle(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnHandlerResult()
    {
        // Arrange
        TestResponse expectedResponse = new("Expected");
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        TestResponse response = await decorator.Handle(new TestRequest("test"));

        // Assert
        response.Should().Be(expectedResponse);
    }

    #endregion

    #region Logging Behavior Tests

    [Fact]
    public async Task Handle_ShouldLogStart()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        await decorator.Handle(new TestRequest("test"));

        // Assert
        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[START]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldLogEnd()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        await decorator.Handle(new TestRequest("test"));

        // Assert
        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[END]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldLogRequestTypeName()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        await decorator.Handle(new TestRequest("test"));

        // Assert
        loggerMock.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestRequest")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeast(2)
        ); // At least START and END logs
    }

    [Fact]
    public async Task Handle_ShouldLogResponseTypeName()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        await decorator.Handle(new TestRequest("test"));

        // Assert
        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("TestResponse")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        ); // In END log
    }

    #endregion

    #region Performance Logging Tests

    [Fact]
    public async Task Handle_WithFastRequest_ShouldNotLogPerformanceWarning()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success")); // Returns immediately

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        await decorator.Handle(new TestRequest("test"));

        // Assert
        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_WithSlowRequest_ShouldLogPerformanceWarning()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(3100); // Delay more than 3 seconds
                return new TestResponse("Success");
            });

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        await decorator.Handle(new TestRequest("test"));

        // Assert
        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[PERFORMANCE]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WithSlowRequest_ShouldLogElapsedSeconds()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(3100);
                return new TestResponse("Success");
            });

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        await decorator.Handle(new TestRequest("test"));

        // Assert
        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("seconds")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassToHandler()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestResponse("Success"));

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        // Act
        await decorator.Handle(new TestRequest("test"), token);

        // Assert
        handlerMock.Verify(h => h.Handle(It.IsAny<TestRequest>(), token), Times.Once);
    }

    #endregion

    #region Execution Order Tests

    [Fact]
    public async Task Handle_ShouldLogInCorrectOrder()
    {
        // Arrange
        List<string> logSequence = [];

        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                logSequence.Add("Handler");
                return Task.FromResult(new TestResponse("Success"));
            });

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        loggerMock
            .Setup(x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[START]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                )
            )
            .Callback(() => logSequence.Add("Start"));

        loggerMock
            .Setup(x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[END]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                )
            )
            .Callback(() => logSequence.Add("End"));

        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        await decorator.Handle(new TestRequest("test"));

        // Assert
        logSequence.Should().HaveCount(3);
        logSequence[0].Should().Be("Start");
        logSequence[1].Should().Be("Handler");
        logSequence[2].Should().Be("End");
    }

    [Fact]
    public async Task Handle_ShouldLogEndEvenIfHandlerThrows()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler error"));

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        try
        {
            await decorator.Handle(new TestRequest("test"));
        }
        catch (InvalidOperationException)
        {
            // Expected exception
        }

        // Assert - START should be logged, but END won't be because exception interrupts flow
        loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[START]")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    #region Exception Propagation Tests

    [Fact]
    public async Task Handle_WhenHandlerThrows_ShouldPropagateException()
    {
        // Arrange
        Mock<IRequestHandler<TestRequest, TestResponse>> handlerMock = new();
        handlerMock
            .Setup(h => h.Handle(It.IsAny<TestRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler error"));

        Mock<ILogger<LoggingDecorator<TestRequest, TestResponse>>> loggerMock = new();
        LoggingDecorator<TestRequest, TestResponse> decorator = new(handlerMock.Object, loggerMock.Object);

        // Act
        Func<Task> act = async () => await decorator.Handle(new TestRequest("test"));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Handler error");
    }

    #endregion
}
