using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="RateLimitExceededExceptionHandler"/>.
/// The title, instance and trace extensions are covered for every strategy by
/// <see cref="ExceptionStrategyContractTests" />; the status, the Retry-After header and the
/// custom-message branch are asserted here.
/// </summary>
public class RateLimitExceededExceptionHandlerTests
{
    private readonly RateLimitExceededExceptionHandler _handler = new();
    private readonly SharedExceptionMessage i18n = LocalizerFactory.CreateMessage<SharedExceptionMessage>();

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnLocalizedDetailMessage()
    {
        // Arrange
        TimeSpan retryAfter = TimeSpan.FromSeconds(30);
        RateLimitExceededException exception = new(retryAfter);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.RateLimitExceeded(30));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn429StatusCode()
    {
        // Arrange
        RateLimitExceededException exception = new(TimeSpan.FromSeconds(60));
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status429TooManyRequests);
    }

    [Fact]
    public void CreateProblemDetails_ShouldSetRetryAfterHeader()
    {
        // Arrange
        TimeSpan retryAfter = TimeSpan.FromSeconds(120);
        RateLimitExceededException exception = new(retryAfter);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        _handler.CreateProblemDetails(exception, context);

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
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        _handler.CreateProblemDetails(exception, context);

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
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(customMessage);
    }

    [Fact]
    public void CreateProblemDetails_WithFractionalSeconds_ShouldRoundDownRetryAfter()
    {
        // Arrange
        TimeSpan retryAfter = TimeSpan.FromSeconds(45.7);
        RateLimitExceededException exception = new(retryAfter);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        _handler.CreateProblemDetails(exception, context);

        // Assert
        context.Response.Headers["Retry-After"].ToString().Should().Be("45");
    }

    [Fact]
    public void CreateProblemDetails_InFrench_ShouldReturnFrenchDetailMessage()
    {
        // Arrange
        string enDetail = i18n.RateLimitExceeded(30);
        using var scope = new CultureScope("fr");
        RateLimitExceededException exception = new(TimeSpan.FromSeconds(30));
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotBe(enDetail);
    }

    [Fact]
    public void CreateProblemDetails_InFrench_ShouldContainSeconds()
    {
        // Arrange
        using var scope = new CultureScope("fr");
        RateLimitExceededException exception = new(TimeSpan.FromSeconds(60));
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Contain("60");
    }

    #endregion
}
