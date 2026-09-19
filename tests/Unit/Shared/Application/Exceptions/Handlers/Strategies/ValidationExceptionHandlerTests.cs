using System.Reflection;
using System.Text.Json;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace _116.Unit.Tests.Shared.Application.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="ValidationExceptionHandler"/>.
/// The title, instance and trace extensions are covered for every strategy by
/// <see cref="ExceptionStrategyContractTests" />; the status and the errors extension are asserted
/// here. The null-errors case reaches a branch FluentValidation cannot produce on its own, so it
/// forces the property through its backing field.
/// </summary>
public class ValidationExceptionHandlerTests
{
    private readonly ValidationExceptionHandler _handler = new();

    #region CreateProblemDetails Tests

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

        var errors = problemDetails.Extensions["errors"] as Dictionary<string, string[]>;
        errors.Should().NotBeNull();
        errors!.Should().ContainKey("email").WhoseValue.Should().ContainSingle().Which.Should().Be("Email is required");
        errors
            .Should()
            .ContainKey("password")
            .WhoseValue.Should()
            .ContainSingle()
            .Which.Should()
            .Be("Password must be at least 8 characters");
    }

    [Fact]
    public void CreateProblemDetails_ShouldNotEchoTheSubmittedValue()
    {
        // Arrange
        List<ValidationFailure> failures =
        [
            new("Password", "Password must be at least 8 characters") { AttemptedValue = "hunter2-real-password" },
        ];
        ValidationException exception = new(failures);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        string serialized = JsonSerializer.Serialize(problemDetails);
        serialized.Should().NotContain("hunter2-real-password");
        serialized.Should().NotContain("AttemptedValue");
        serialized.Should().NotContain("FormattedMessagePlaceholderValues");
    }

    [Fact]
    public void CreateProblemDetails_ShouldNotPutTheFailureMessagesInTheDetail()
    {
        // Arrange
        List<ValidationFailure> failures = [new("Password", "Password must be at least 8 characters")];
        ValidationException exception = new(failures);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotContain("Password must be at least 8 characters");
        problemDetails.Detail.Should().NotBeNullOrWhiteSpace();
        problemDetails.Detail.Should().NotBe(exception.Message);
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
        // Arrange
        ValidationException exception = new("Validation failed");
        typeof(ValidationException)
            .GetField(
                $"<{nameof(ValidationException.Errors)}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance
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

        var errors = problemDetails.Extensions["errors"] as Dictionary<string, string[]>;
        errors.Should().NotBeNull();
        errors!.Should().HaveCount(3, "the four failures collapse onto three distinct fields");
        errors["email"].Should().BeEquivalentTo("Email is required", "Email format is invalid");
        errors["password"].Should().ContainSingle();
        errors["name"].Should().ContainSingle();
    }

    #endregion
}
