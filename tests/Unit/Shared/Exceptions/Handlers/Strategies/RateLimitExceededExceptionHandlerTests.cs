using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="RateLimitExceededExceptionHandler"/>.
/// </summary>
public class RateLimitExceededExceptionHandlerTests
{
    private readonly RateLimitExceededExceptionHandler _handler = new();

    #region ExceptionType Tests

    [Fact]
    public void ExceptionType_ShouldReturnRateLimitExceededExceptionType()
    {
        // Act
        Type exceptionType = _handler.ExceptionType;

        // Assert
        exceptionType.Should().Be(typeof(RateLimitExceededException));
    }

    #endregion

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithCorrectTitle()
    {
        // Arrange
        RateLimitExceededException exception = new(TimeSpan.FromSeconds(60));
        DefaultHttpContext context = CreateHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be(nameof(RateLimitExceededException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithExceptionMessage()
    {
        // Arrange
        TimeSpan retryAfter = TimeSpan.FromSeconds(30);
        RateLimitExceededException exception = new(retryAfter);
        DefaultHttpContext context = CreateHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Contain("Rate limit exceeded");
        problemDetails.Detail.Should().Contain("30 seconds");
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn429StatusCode()
    {
        // Arrange
        RateLimitExceededException exception = new(TimeSpan.FromSeconds(60));
        DefaultHttpContext context = CreateHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status429TooManyRequests);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeRequestPath()
    {
        // Arrange
        RateLimitExceededException exception = new(TimeSpan.FromSeconds(60));
        DefaultHttpContext context = CreateHttpContext();
        string requestPath = "/api/v1/public/auth/login";
        context.Request.Path = requestPath;

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Instance.Should().Be(requestPath);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeTraceIdExtension()
    {
        // Arrange
        RateLimitExceededException exception = new(TimeSpan.FromSeconds(60));
        DefaultHttpContext context = CreateHttpContext();
        string traceId = "ratelimit-trace-505";
        context.TraceIdentifier = traceId;

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"].Should().Be(traceId);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeTimestampExtension()
    {
        // Arrange
        RateLimitExceededException exception = new(TimeSpan.FromSeconds(60));
        DefaultHttpContext context = CreateHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("timestamp");
        var timestamp = (DateTime)problemDetails.Extensions["timestamp"]!;
        timestamp.Should().NotBe(default(DateTime));
    }

    [Fact]
    public void CreateProblemDetails_ShouldSetRetryAfterHeader()
    {
        // Arrange
        TimeSpan retryAfter = TimeSpan.FromSeconds(120);
        RateLimitExceededException exception = new(retryAfter);
        DefaultHttpContext context = CreateHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        context.Response.Headers.Should().ContainKey("Retry-After");
        context.Response.Headers["Retry-After"].ToString().Should().Be("120");
    }

    [Fact]
    public void CreateProblemDetails_WithDifferentRetryAfterValues_ShouldSetCorrectHeader()
    {
        // Arrange
        TimeSpan retryAfter = TimeSpan.FromMinutes(5);
        RateLimitExceededException exception = new(retryAfter);
        DefaultHttpContext context = CreateHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        context.Response.Headers["Retry-After"].ToString().Should().Be("300");
    }

    [Fact]
    public void CreateProblemDetails_WithCustomMessage_ShouldUseCustomMessage()
    {
        // Arrange
        string customMessage = "Too many login attempts";
        TimeSpan retryAfter = TimeSpan.FromSeconds(60);
        RateLimitExceededException exception = new(customMessage, retryAfter);
        DefaultHttpContext context = CreateHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(customMessage);
    }

    [Fact]
    public void CreateProblemDetails_WithFractionalSeconds_ShouldRoundDownRetryAfter()
    {
        // Arrange
        TimeSpan retryAfter = TimeSpan.FromSeconds(45.7);
        RateLimitExceededException exception = new(retryAfter);
        DefaultHttpContext context = CreateHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        context.Response.Headers["Retry-After"].ToString().Should().Be("45");
    }

    #endregion

    #region Helper Methods

    private static DefaultHttpContext CreateHttpContext()
    {
        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        context.TraceIdentifier = "test-trace-id";
        return context;
    }

    #endregion
}
