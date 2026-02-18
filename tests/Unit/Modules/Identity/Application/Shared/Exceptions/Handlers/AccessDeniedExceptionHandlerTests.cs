using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Unit tests for <see cref="AccessDeniedExceptionHandler"/>.
/// </summary>
public class AccessDeniedExceptionHandlerTests
{
    private readonly AccessDeniedExceptionHandler _handler = new();

    #region ExceptionType Tests

    [Fact]
    public void ExceptionType_ShouldReturnAccessDeniedExceptionType()
    {
        // Act
        Type exceptionType = _handler.ExceptionType;

        // Assert
        exceptionType.Should().Be(typeof(AccessDeniedException));
    }

    #endregion

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithCorrectTitle()
    {
        // Arrange
        AccessDeniedException exception = new("Access denied");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be(nameof(AccessDeniedException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithExceptionMessage()
    {
        // Arrange
        string errorMessage = "You do not have permission to access this resource";
        AccessDeniedException exception = new(errorMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(errorMessage);
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn403StatusCode()
    {
        // Arrange
        AccessDeniedException exception = new("Access denied");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeRequestPath()
    {
        // Arrange
        AccessDeniedException exception = new("Access denied");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string requestPath = "/api/admin/users";
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
        AccessDeniedException exception = new("Access denied");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string traceId = "test-trace-123";
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
        AccessDeniedException exception = new("Access denied");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("timestamp");
        var timestamp = (DateTime)problemDetails.Extensions["timestamp"]!;
        timestamp.Should().NotBe(default(DateTime));
    }

    #endregion

    #region Helper Methods

    #endregion
}
