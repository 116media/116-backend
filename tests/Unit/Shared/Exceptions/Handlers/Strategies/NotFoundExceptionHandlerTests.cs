using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers.Strategies;

/// <summary>
/// Unit tests for <see cref="NotFoundExceptionHandler"/>.
/// </summary>
public class NotFoundExceptionHandlerTests
{
    private readonly NotFoundExceptionHandler _handler = new();
    private readonly SharedExceptionMessage i18n = LocalizerFactory.CreateMessage<SharedExceptionMessage>("en");

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

    #region Localized Message Tests

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndId_ShouldUseLocalizedMessage()
    {
        // Arrange
        NotFoundException exception = new("UserEntity", (object)"abc-123");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.EntityNotFoundById("User", "abc-123"));
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndKeyNameAndKeyValue_ShouldUseLocalizedMessage()
    {
        // Arrange
        NotFoundException exception = new("UserEntity", "email", "test@test.com");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(i18n.EntityNotFoundByKey("User", "email", "test@test.com"));
    }

    [Fact]
    public void CreateProblemDetails_WithStringOnlyConstructor_ShouldUseFallbackMessage()
    {
        // Arrange
        string customMessage = "Custom not found message";
        NotFoundException exception = new(customMessage);
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Be(customMessage);
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndId_ShouldContainEntityName()
    {
        // Arrange
        NotFoundException exception = new("RoleEntity", (object)"role-uuid");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Contain("Role");
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndId_ShouldContainKey()
    {
        // Arrange
        NotFoundException exception = new("SessionEntity", (object)"session-uuid");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Contain("session-uuid");
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameKeyNameAndKeyValue_ShouldContainAllParts()
    {
        // Arrange
        NotFoundException exception = new("PermissionEntity", "resource", "articles.read");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().Contain("Permission");
        problemDetails.Detail.Should().Contain("resource");
        problemDetails.Detail.Should().Contain("articles.read");
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndId_InFrench_ShouldReturnFrenchMessage()
    {
        // Arrange
        string enDetail = i18n.EntityNotFoundById("User", "abc-123");
        using var scope = new CultureScope("fr");
        NotFoundException exception = new("UserEntity", (object)"abc-123");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotBe(enDetail);
        problemDetails.Detail.Should().Contain("User");
        problemDetails.Detail.Should().Contain("abc-123");
    }

    [Fact]
    public void CreateProblemDetails_WithEntityNameAndKeyNameAndKeyValue_InFrench_ShouldReturnFrenchMessage()
    {
        // Arrange
        string enDetail = i18n.EntityNotFoundByKey("User", "email", "test@test.com");
        using var scope = new CultureScope("fr");
        NotFoundException exception = new("UserEntity", "email", "test@test.com");
        DefaultHttpContext context = HttpTestHelpers.CreateDefaultHttpContext();

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Detail.Should().NotBe(enDetail);
        problemDetails.Detail.Should().Contain("User");
        problemDetails.Detail.Should().Contain("test@test.com");
    }

    #endregion
}
