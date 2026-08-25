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
/// Unit tests for <see cref="ResourceNotFoundExceptionHandler"/>.
/// The title, instance and trace extensions are covered for every strategy by
/// <see cref="ExceptionStrategyContractTests" />; the status and the localized detail that replaces
/// the exception message are asserted here.
/// </summary>
public class ResourceNotFoundExceptionHandlerTests
{
    private readonly ResourceNotFoundExceptionHandler _handler = new();

    private readonly SharedExceptionMessage _i18n = LocalizerFactory.CreateMessage<SharedExceptionMessage>();

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnLocalizedResourceNotFoundMessage()
    {
        // Arrange
        ResourceNotFoundException exception = new(path: "/api/v1/public/nonexistent", method: "GET");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(_i18n.ResourceNotFound());
    }

    [Fact]
    public void CreateProblemDetails_ShouldNotLeakRequestPathOrMethod()
    {
        // Arrange
        ResourceNotFoundException exception = new(path: "/api/v1/public/nonexistent", method: "GET");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        exception.Message.Should().Contain("/api/v1/public/nonexistent").And.Contain("GET");
        problemDetails.Detail.Should().NotContain("/api/v1/public/nonexistent");
        problemDetails.Detail.Should().NotBe(exception.Message);
    }

    [Fact]
    public void CreateProblemDetails_InFrench_ShouldReturnFrenchResourceNotFoundMessage()
    {
        // Arrange
        string enDetail = _i18n.ResourceNotFound();
        using var scope = new CultureScope("fr");
        ResourceNotFoundException exception = new(path: "/api/v1/public/nonexistent", method: "GET");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotBe(enDetail);
        problemDetails.Detail.Should().Be(_i18n.ResourceNotFound());
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn404StatusCode()
    {
        // Arrange
        ResourceNotFoundException exception = new("Resource not found");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    #endregion
}
