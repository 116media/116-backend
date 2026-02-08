using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="BadRequestExceptionHandler"/>.
/// </summary>
public class BadRequestExceptionHandlerTests
{
    private readonly BadRequestExceptionHandler _handler = new();

    #region ExceptionType Tests

    [Fact]
    public void ExceptionType_ShouldReturnBadRequestExceptionType()
    {
        // Act
        Type exceptionType = _handler.ExceptionType;

        // Assert
        exceptionType.Should().Be(typeof(BadRequestException));
    }

    #endregion

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithCorrectTitle()
    {
        // Arrange
        BadRequestException exception = new("Invalid request");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be(nameof(BadRequestException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithExceptionMessage()
    {
        // Arrange
        string errorMessage = "Invalid email format";
        BadRequestException exception = new(errorMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(errorMessage);
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn400StatusCode()
    {
        // Arrange
        BadRequestException exception = new("Bad request");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeRequestPath()
    {
        // Arrange
        BadRequestException exception = new("Bad request");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string requestPath = "/api/users";
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
        BadRequestException exception = new("Bad request");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string traceId = "test-trace-123";
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
        BadRequestException exception = new("Bad request");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("timestamp");
        var timestamp = (DateTime)problemDetails.Extensions["timestamp"]!;
        timestamp.Should().NotBe(default(DateTime));
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
        var problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(message);
    }

    #endregion
}
