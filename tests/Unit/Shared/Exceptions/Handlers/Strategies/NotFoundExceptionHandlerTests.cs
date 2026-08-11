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
/// Unit tests for <see cref="NotFoundExceptionHandler"/>.
/// </summary>
public class NotFoundExceptionHandlerTests
{
    private readonly NotFoundExceptionHandler _handler = new();
    private readonly SharedExceptionMessage i18n = LocalizerFactory.CreateMessage<SharedExceptionMessage>();

    #region ExceptionType Tests

    [Fact]
    public void ExceptionType_ShouldReturnNotFoundExceptionType()
    {
        // Act
        Type exceptionType = _handler.ExceptionType;

        // Assert
        exceptionType.Should().Be(typeof(NotFoundException));
    }

    #endregion

    #region CreateProblemDetails Tests

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithCorrectTitle()
    {
        // Arrange
        NotFoundException exception = new("Resource not found");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Title.Should().Be(nameof(NotFoundException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithExceptionMessage()
    {
        // Arrange
        string errorMessage = "User with id '123' was not found";
        NotFoundException exception = new(errorMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(errorMessage);
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn404StatusCode()
    {
        // Arrange
        NotFoundException exception = new("Not found");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeRequestPath()
    {
        // Arrange
        NotFoundException exception = new("Not found");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string requestPath = "/api/v1/admin/users/123";
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
        NotFoundException exception = new("Not found");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();
        string traceId = "notfound-trace-202";
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
        NotFoundException exception = new("Not found");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Extensions.Should().ContainKey("timestamp");
        var timestamp = (DateTime)problemDetails.Extensions["timestamp"]!;
        timestamp.Should().NotBe(default(DateTime));
    }

    #endregion

    #region Friendly Localized Message Tests

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndId_ShouldUseFriendlyLocalizedMessage()
    {
        // Arrange
        NotFoundException exception = new("UserEntity", (object)"abc-123");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert — friendly per-entity label, keyed on the cleaned entity name
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("User"));
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameKeyNameAndKeyValue_ShouldUseFriendlyLocalizedMessage()
    {
        // Arrange
        NotFoundException exception = new("UserEntity", "email", "test@test.com");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert — same friendly message regardless of which key the lookup used
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("User"));
    }

    [Fact]
    public void CreateProblemDetails_WithStringOnlyConstructor_ShouldUseExceptionMessage()
    {
        // Arrange
        string customMessage = "Custom not found message";
        NotFoundException exception = new(customMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert — no structured entity, so the raw (already-localized) message is used
        problemDetails.Detail.Should().Be(customMessage);
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndId_ShouldNotLeakEntityNameOrKeyValue()
    {
        // Arrange
        NotFoundException exception = new("SessionEntity", (object)"session-uuid");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert — the raw class name and the searched value never reach the user
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("Session"));
        problemDetails.Detail.Should().NotContain("session-uuid");
        problemDetails.Detail.Should().NotContain("SessionEntity");
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameKeyNameAndKeyValue_ShouldNotLeakKeyNameOrValue()
    {
        // Arrange
        NotFoundException exception = new("PermissionEntity", "resource", "articles.read");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert — neither the key name nor its value are exposed
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("Permission"));
        problemDetails.Detail.Should().NotContain("resource");
        problemDetails.Detail.Should().NotContain("articles.read");
    }

    [Fact]
    public void CreateProblemDetails_WithUnmappedEntity_ShouldFallBackToGenericLabel()
    {
        // Arrange — an entity with no specific label falls back to the generic one
        NotFoundException exception = new("SomethingObscureEntity", (object)"xyz-789");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("SomethingObscure"));
        problemDetails.Detail.Should().NotContain("SomethingObscure");
        problemDetails.Detail.Should().NotContain("xyz-789");
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndId_InFrench_ShouldReturnFrenchFriendlyMessage()
    {
        // Arrange
        string enDetail = i18n.EntityNotFound("User");
        using var scope = new CultureScope("fr");
        NotFoundException exception = new("UserEntity", (object)"abc-123");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert — localized to French, still friendly, still no leak
        problemDetails.Detail.Should().NotBe(enDetail);
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("User"));
        problemDetails.Detail.Should().NotContain("abc-123");
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameKeyNameAndKeyValue_InFrench_ShouldReturnFrenchFriendlyMessage()
    {
        // Arrange
        string enDetail = i18n.EntityNotFound("User");
        using var scope = new CultureScope("fr");
        NotFoundException exception = new("UserEntity", "email", "test@test.com");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotBe(enDetail);
        problemDetails.Detail.Should().Be(i18n.EntityNotFound("User"));
        problemDetails.Detail.Should().NotContain("test@test.com");
    }

    #endregion
}
