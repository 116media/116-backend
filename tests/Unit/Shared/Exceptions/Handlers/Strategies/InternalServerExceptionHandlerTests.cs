using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="InternalServerExceptionHandler"/>.
/// </summary>
public class InternalServerExceptionHandlerTests
{
    private readonly InternalServerExceptionHandler _handler = new();

    #region ExceptionType Tests

    [Fact]
    public void ExceptionType_ShouldReturnInternalServerExceptionType()
    {
        // Act
        Type exceptionType = _handler.ExceptionType;

        // Assert
        exceptionType.Should().Be(typeof(InternalServerException));
    }

    #endregion

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithCorrectTitle()
    {
        // Arrange
        InternalServerException exception = new("Internal error occurred");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be(nameof(InternalServerException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithExceptionMessage()
    {
        // Arrange
        string errorMessage = "An unexpected error occurred while processing the request";
        InternalServerException exception = new(errorMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(errorMessage);
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn500StatusCode()
    {
        // Arrange
        InternalServerException exception = new("Internal error");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeRequestPath()
    {
        // Arrange
        InternalServerException exception = new("Internal error");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string requestPath = "/api/v1/admin/users";
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
        InternalServerException exception = new("Internal error");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string traceId = "internal-trace-789";
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
        InternalServerException exception = new("Internal error");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("timestamp");
        var timestamp = (DateTime)problemDetails.Extensions["timestamp"]!;
        timestamp.Should().NotBe(default(DateTime));
    }

    #endregion
}
