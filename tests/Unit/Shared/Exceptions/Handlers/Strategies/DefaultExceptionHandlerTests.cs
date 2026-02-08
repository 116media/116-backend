using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="DefaultExceptionHandler"/>.
/// </summary>
public class DefaultExceptionHandlerTests
{
    private readonly DefaultExceptionHandler _handler = new();

    #region ExceptionType Tests

    [Fact]
    public void ExceptionType_ShouldReturnBaseExceptionType()
    {
        // Act
        Type exceptionType = _handler.ExceptionType;

        // Assert
        exceptionType.Should().Be(typeof(Exception));
    }

    #endregion

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithCorrectTitle()
    {
        // Arrange
        Exception exception = new("Test error");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be("Exception");
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithExceptionMessage()
    {
        // Arrange
        string errorMessage = "Test error message";
        Exception exception = new(errorMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(errorMessage);
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn500StatusCode()
    {
        // Arrange
        Exception exception = new("Test error");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeRequestPath()
    {
        // Arrange
        Exception exception = new("Test error");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string requestPath = "/api/test";
        context.Request.Path = requestPath;

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Instance.Should().Be(requestPath);
    }

    [Fact]
    public void CreateProblemDetails_WithDifferentExceptionTypes_ShouldUseActualTypeName()
    {
        // Arrange
        InvalidOperationException exception = new("Invalid operation");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be("InvalidOperationException");
    }

    [Fact]
    public void CreateProblemDetails_WithEmptyMessage_ShouldHandleGracefully()
    {
        // Arrange
        Exception exception = new(string.Empty);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(string.Empty);
        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion
}
