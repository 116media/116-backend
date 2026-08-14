using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="AuthorizationExceptionHandler" />.
/// The title, instance and trace extensions are covered for every strategy by
/// <see cref="ExceptionStrategyContractTests" />; only the status and detail are asserted here.
/// </summary>
public class AuthorizationExceptionHandlerTests
{
    private readonly AuthorizationExceptionHandler _handler = new();

    [Fact]
    public void CreateProblemDetails_ShouldReturn403StatusCode()
    {
        // Arrange
        AuthorizationException exception = new("Forbidden");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void CreateProblemDetails_ShouldUseTheExceptionMessageAsDetail()
    {
        // Arrange
        string errorMessage = "You do not have permission to access this resource";
        AuthorizationException exception = new(errorMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(errorMessage);
    }
}
