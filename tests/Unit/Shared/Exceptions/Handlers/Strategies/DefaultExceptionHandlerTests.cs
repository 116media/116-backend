using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="DefaultExceptionHandler" />.
/// This is the only strategy that builds its ProblemDetails inline instead of routing through the
/// shared envelope helper, so the trace extensions it omits are pinned here rather than in
/// <see cref="ExceptionStrategyContractTests" />.
/// </summary>
public class DefaultExceptionHandlerTests
{
    private readonly DefaultExceptionHandler _handler = new();

    [Fact]
    public void CreateProblemDetails_ShouldReturn500StatusCode()
    {
        // Arrange
        Exception exception = new("Test error");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void CreateProblemDetails_ShouldUseTheExceptionMessageAsDetail()
    {
        // Arrange
        string errorMessage = "Test error message";
        Exception exception = new(errorMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(errorMessage);
    }

    [Fact]
    public void CreateProblemDetails_WithDifferentExceptionTypes_ShouldUseActualTypeName()
    {
        // Arrange
        InvalidOperationException exception = new("Invalid operation");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be(nameof(InvalidOperationException));
    }

    [Fact]
    public void CreateProblemDetails_WithEmptyMessage_ShouldHandleGracefully()
    {
        // Arrange
        Exception exception = new(string.Empty);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(string.Empty);
        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void CreateProblemDetails_ShouldNotCarryTheTraceExtensions()
    {
        // Arrange
        Exception exception = new("Test error");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().NotContainKey("traceId");
        problemDetails.Extensions.Should().NotContainKey("timestamp");
    }
}
