using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="ValidationExceptionHandler"/>.
/// </summary>
public class ValidationExceptionHandlerTests
{
    private readonly ValidationExceptionHandler _handler = new();

    #region ExceptionType Tests

    [Fact]
    public void ExceptionType_ShouldReturnValidationExceptionType()
    {
        // Act
        Type exceptionType = _handler.ExceptionType;

        // Assert
        exceptionType.Should().Be(typeof(ValidationException));
    }

    #endregion

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithCorrectTitle()
    {
        // Arrange
        List<ValidationFailure> failures = [new("Email", "Email is required")];
        ValidationException exception = new(failures);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be(nameof(ValidationException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithExceptionMessage()
    {
        // Arrange
        List<ValidationFailure> failures = [new("Email", "Email is required")];
        ValidationException exception = new(failures);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn400StatusCode()
    {
        // Arrange
        List<ValidationFailure> failures = [new("Email", "Email is required")];
        ValidationException exception = new(failures);
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
        List<ValidationFailure> failures = [new("Email", "Email is required")];
        ValidationException exception = new(failures);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string requestPath = "/api/v1/public/auth/login";
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
        List<ValidationFailure> failures = [new("Email", "Email is required")];
        ValidationException exception = new(failures);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string traceId = "validation-trace-404";
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
        List<ValidationFailure> failures = [new("Email", "Email is required")];
        ValidationException exception = new(failures);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("timestamp");
        var timestamp = (DateTime)problemDetails.Extensions["timestamp"]!;
        timestamp.Should().NotBe(default(DateTime));
    }

    [Fact]
    public void CreateProblemDetails_WithValidationErrors_ShouldIncludeErrorsExtension()
    {
        // Arrange
        List<ValidationFailure> failures =
        [
            new("Email", "Email is required"),
            new("Password", "Password must be at least 8 characters"),
        ];
        ValidationException exception = new(failures);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("errors");
        problemDetails.Extensions["errors"].Should().BeEquivalentTo(failures);
    }

    [Fact]
    public void CreateProblemDetails_WithNoValidationErrors_ShouldNotIncludeErrorsExtension()
    {
        // Arrange
        List<ValidationFailure> failures = [];
        ValidationException exception = new(failures);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().NotContainKey("errors");
    }

    [Fact]
    public void CreateProblemDetails_WithNullErrors_ShouldNotIncludeErrorsExtension()
    {
        // Arrange — FluentValidation never sets Errors to null; force it via reflection
        // to cover the null-conditional branch in the handler.
        ValidationException exception = new("Validation failed");
        typeof(ValidationException)
            .GetField(
                "<Errors>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            )
            ?.SetValue(exception, null);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().NotContainKey("errors");
    }

    [Fact]
    public void CreateProblemDetails_WithMultipleValidationErrors_ShouldIncludeAllErrors()
    {
        // Arrange
        List<ValidationFailure> failures =
        [
            new("Email", "Email is required"),
            new("Email", "Email format is invalid"),
            new("Password", "Password is required"),
            new("Name", "Name must not exceed 50 characters"),
        ];
        ValidationException exception = new(failures);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("errors");
        var errors = problemDetails.Extensions["errors"] as IEnumerable<ValidationFailure>;
        errors.Should().HaveCount(4);
    }

    #endregion
}
