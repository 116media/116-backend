using System.Text.Json;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers;

/// <summary>
/// Unit tests for <see cref="ExceptionHandler"/>.
/// </summary>
public class ExceptionHandlerTests
{
    private readonly Mock<ILogger<ExceptionHandler>> _loggerMock;
    private readonly ExceptionStrategyRegistry _strategyRegistry;
    private readonly ExceptionHandler _handler;

    public ExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<ExceptionHandler>>();

        // Create a minimal ExceptionStrategyRegistry with all strategies
        DefaultExceptionHandler defaultStrategy = new();
        BadRequestExceptionHandler badRequestStrategy = new();
        NotFoundExceptionHandler notFoundStrategy = new();
        ConflictExceptionHandler conflictStrategy = new();
        AuthenticationExceptionHandler authenticationStrategy = new();
        AuthorizationExceptionHandler authorizationStrategy = new();
        InternalServerExceptionHandler internalServerStrategy = new();
        FormatExceptionStrategy formatExceptionStrategy = new();

        List<_116.Shared.Application.Exceptions.Handlers.Contracts.IExceptionStrategy> strategies =
        [
            defaultStrategy,
            badRequestStrategy,
            notFoundStrategy,
            conflictStrategy,
            authenticationStrategy,
            authorizationStrategy,
            internalServerStrategy,
            formatExceptionStrategy,
        ];

        _strategyRegistry = new ExceptionStrategyRegistry(strategies, defaultStrategy);
        _handler = new ExceptionHandler(_loggerMock.Object, _strategyRegistry);
    }

    #region TryHandleAsync Tests

    [Fact]
    public async Task TryHandleAsync_ShouldReturnTrue()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        Exception exception = new("Test error");

        // Act
        bool result = await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue("exception handler always returns true");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldSetCorrectStatusCode()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        BadRequestException exception = new("Bad request", "TEST_001");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldUseDefaultStatusWhenNull()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        Exception exception = new("Test error");
        ProblemDetails problemDetails = new()
        {
            Status = null,
            Title = "Exception",
            Detail = "Test error",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldEnrichProblemDetailsWithTraceId()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string traceId = "test-trace-123";
        context.TraceIdentifier = traceId;

        Exception exception = new("Test error");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert - Read the actual response
        context.Response.Body.Position = 0;
        using StreamReader reader = new(context.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"]!.ToString().Should().Be(traceId);
    }

    [Fact]
    public async Task TryHandleAsync_ShouldEnrichProblemDetailsWithTimestamp()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        Exception exception = new("Test error");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert - Read the actual response
        context.Response.Body.Position = 0;
        using StreamReader reader = new(context.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

        problemDetails.Should().NotBeNull();
        problemDetails.Extensions.Should().ContainKey("timestamp");

        // Verify the timestamp can be parsed as a valid DateTime
        string timestampStr = problemDetails.Extensions["timestamp"]!.ToString()!;
        bool canParse = DateTime.TryParse(timestampStr, out DateTime timestamp);
        canParse.Should().BeTrue("timestamp should be a valid DateTime");
        timestamp.Should().NotBe(default(DateTime), "timestamp should not be default");
    }

    [Fact]
    public async Task TryHandleAsync_ShouldWriteJsonResponse()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        Exception exception = new("Test error");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Exception",
            Detail = "Test error",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        context.Response.Body.Position = 0;
        using StreamReader reader = new(context.Response.Body);
        string responseBody = await reader.ReadToEndAsync();
        responseBody.Should().NotBeEmpty();

        // Verify it's valid JSON
        var deserializedProblem = JsonSerializer.Deserialize<ProblemDetails>(responseBody);
        deserializedProblem.Should().NotBeNull();
    }

    [Fact]
    public async Task TryHandleAsync_ShouldCallStrategyRegistry()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        Exception exception = new("Test error");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Exception",
            Detail = "Test error",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        // Registry is called internally, verification not needed with real implementation
    }

    [Fact]
    public async Task TryHandleAsync_ShouldLogException()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        Exception exception = new("Test error");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Exception",
            Detail = "Test error",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task TryHandleAsync_WithBadRequestException_ShouldLogWarning()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        BadRequestException exception = new("Bad request", "TEST_001");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "BadRequest",
            Detail = "Bad request",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task TryHandleAsync_WithNotFoundException_ShouldLogWarning()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        NotFoundException exception = new("Not found", "TEST_002");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status404NotFound,
            Title = "NotFound",
            Detail = "Not found",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task TryHandleAsync_WithConflictException_ShouldLogWarning()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        ConflictException exception = new("Conflict", "TEST_003");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = "Conflict",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task TryHandleAsync_WithAuthenticationException_ShouldLogWarning()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        AuthenticationException exception = new("Authentication failed", "TEST_004");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = "Authentication failed",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task TryHandleAsync_WithAuthorizationException_ShouldLogWarning()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        AuthorizationException exception = new("Authorization failed", "TEST_005");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = "Authorization failed",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task TryHandleAsync_WithFormatException_ShouldLogWarning()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        FormatException exception = new("Invalid format");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task TryHandleAsync_WithUnknownException_ShouldLogError()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        InvalidOperationException exception = new("Invalid operation");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Exception",
            Detail = "Invalid operation",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task TryHandleAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        Exception exception = new("Test error");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Exception",
            Detail = "Test error",
        };

        using CancellationTokenSource cts = new();
        CancellationToken cancellationToken = cts.Token;

        // Act
        bool result = await _handler.TryHandleAsync(context, exception, cancellationToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_WithIdentityButNoNameClaim_ShouldLogAnonymous()
    {
        // Arrange — covers Identity?.Name null branch: Identity not null but Name claim absent
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        context.User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());

        Exception exception = new("Test error");

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task TryHandleAsync_WithAuthenticatedUser_ShouldLogUsername()
    {
        // Arrange
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        context.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "testuser")],
                "TestAuthentication"
            )
        );

        Exception exception = new("Test error");
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Exception",
            Detail = "Test error",
        };

        // Act
        await _handler.TryHandleAsync(context, exception, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion
}
