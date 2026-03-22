using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="FormatExceptionStrategy"/>.
/// </summary>
public class FormatExceptionStrategyTests
{
    private readonly FormatExceptionStrategy _handler = new();

    #region ExceptionType Tests

    [Fact]
    public void ExceptionType_ShouldReturnFormatExceptionType()
    {
        // Act
        Type exceptionType = _handler.ExceptionType;

        // Assert
        exceptionType.Should().Be<FormatException>();
    }

    #endregion

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithInvalidFormatTitle()
    {
        // Arrange
        FormatException exception = new("Input string was not in a correct format.");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be(nameof(InvalidFormatException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithUuidDetail()
    {
        // Arrange
        FormatException exception = new("Input string was not in a correct format.");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be("The provided identifier is invalid.");
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn400StatusCode()
    {
        // Arrange
        FormatException exception = new("Input string was not in a correct format.");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeRequestPath()
    {
        // Arrange
        FormatException exception = new("Input string was not in a correct format.");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        const string requestPath = "/api/v1/admin/roles/not-a-guid";
        context.Request.Path = requestPath;

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Instance.Should().Be(requestPath);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeTraceIdExtension()
    {
        // Arrange
        FormatException exception = new("Input string was not in a correct format.");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        const string traceId = "test-trace-123";
        context.TraceIdentifier = traceId;

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"].Should().Be(traceId);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeTimestampExtension()
    {
        // Arrange
        FormatException exception = new("Input string was not in a correct format.");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("timestamp");
        var timestamp = (DateTime)problemDetails.Extensions["timestamp"]!;
        timestamp.Should().NotBe(default);
    }

    #endregion

    #region InvalidFormatException Tests

    [Fact]
    public void InvalidFormatException_WithMessageOnly_ShouldSetMessage()
    {
        // Arrange
        const string message = "Invalid format error";

        // Act
        var exception = new InvalidFormatException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void InvalidFormatException_WithMessageOnly_ShouldHaveNullDetails()
    {
        // Arrange
        const string message = "Invalid format error";

        // Act
        var exception = new InvalidFormatException(message);

        // Assert
        exception.Details.Should().BeNull();
    }

    [Fact]
    public void InvalidFormatException_WithMessageAndDetails_ShouldSetMessage()
    {
        // Arrange
        const string message = "Invalid format error";
        const string details = "The value must be a valid UUID.";

        // Act
        var exception = new InvalidFormatException(message, details);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void InvalidFormatException_WithMessageAndDetails_ShouldSetDetails()
    {
        // Arrange
        const string message = "Invalid format error";
        const string details = "The value must be a valid UUID.";

        // Act
        var exception = new InvalidFormatException(message, details);

        // Assert
        exception.Details.Should().Be(details);
    }

    [Fact]
    public void InvalidFormatException_ShouldInheritFromException()
    {
        // Act
        var exception = new InvalidFormatException("error");

        // Assert
        exception.Should().BeAssignableTo<Exception>();
    }

    #endregion
}
