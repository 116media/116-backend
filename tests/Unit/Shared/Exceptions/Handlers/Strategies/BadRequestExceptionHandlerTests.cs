using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="BadRequestExceptionHandler" />.
/// The title, instance and trace extensions are covered for every strategy by
/// <see cref="ExceptionStrategyContractTests" />; the status and detail construction are asserted here.
/// </summary>
public class BadRequestExceptionHandlerTests
{
    private readonly BadRequestExceptionHandler _handler = new();

    [Fact]
    public void CreateProblemDetails_ShouldReturn400StatusCode()
    {
        // Arrange
        BadRequestException exception = new("Bad request");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void CreateProblemDetails_ShouldUseTheExceptionMessageAsDetail()
    {
        // Arrange
        string errorMessage = "Invalid email format";
        BadRequestException exception = new(errorMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(errorMessage);
    }

    [Fact]
    public void CreateProblemDetails_WithExceptionDetails_ShouldOnlyIncludeMessageInDetail()
    {
        // Arrange
        string message = "Invalid request";
        string details = "Additional context about the error";
        BadRequestException exception = new(message, details);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(message);
        problemDetails.Detail.Should().NotContain(details);
    }
}
