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
/// shared envelope helper, so its trace extensions (traceId + timestamp) are pinned here rather than in
/// <see cref="ExceptionStrategyContractTests" />. It also withholds the raw exception message outside
/// Development; the tests run in the Development environment provided by the test host.
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
    public void CreateProblemDetails_ShouldCarryTheTraceExtensions()
    {
        // Arrange
        Exception exception = new("Test error");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions.Should().ContainKey("timestamp");
    }
}
