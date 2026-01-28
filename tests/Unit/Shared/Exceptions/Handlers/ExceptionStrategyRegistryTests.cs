using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Exceptions.Handlers;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Handlers;

/// <summary>
/// Unit tests for <see cref="ExceptionStrategyRegistry"/>.
/// </summary>
public class ExceptionStrategyRegistryTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_ShouldRegisterAllStrategies()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        BadRequestExceptionHandler badRequestStrategy = new();
        NotFoundExceptionHandler notFoundStrategy = new();

        List<IExceptionStrategy> strategies = [defaultStrategy, badRequestStrategy, notFoundStrategy];

        // Act
        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);

        // Assert - Should not throw and be able to create problem details
        DefaultHttpContext context = CreateHttpContext();
        BadRequestException badRequestException = new("Bad request", "TEST_001");
        ProblemDetails result = registry.CreateProblemDetails(badRequestException, context);
        result.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldExcludeDefaultStrategyFromRegistry()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        List<IExceptionStrategy> strategies = [defaultStrategy];

        // Act
        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);

        // Assert - Should use default strategy for unknown exception
        DefaultHttpContext context = CreateHttpContext();
        InvalidOperationException exception = new("Invalid operation");
        ProblemDetails result = registry.CreateProblemDetails(exception, context);
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region CreateProblemDetails - Direct Type Match Tests

    [Fact]
    public void CreateProblemDetails_WithRegisteredExceptionType_ShouldUseCorrectStrategy()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        BadRequestExceptionHandler badRequestStrategy = new();
        List<IExceptionStrategy> strategies = [defaultStrategy, badRequestStrategy];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();
        BadRequestException exception = new("Bad request", "TEST_001");

        // Act
        ProblemDetails result = registry.CreateProblemDetails(exception, context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusCodes.Status400BadRequest);
        result.Title.Should().Be("BadRequestException");
    }

    [Fact]
    public void CreateProblemDetails_WithUnregisteredExceptionType_ShouldUseDefaultStrategy()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        List<IExceptionStrategy> strategies = [defaultStrategy];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();
        InvalidOperationException exception = new("Invalid operation");

        // Act
        ProblemDetails result = registry.CreateProblemDetails(exception, context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusCodes.Status500InternalServerError);
        result.Title.Should().Be("InvalidOperationException");
    }

    [Fact]
    public void CreateProblemDetails_WithMultipleRegisteredTypes_ShouldUseMostSpecificStrategy()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        NotFoundExceptionHandler notFoundStrategy = new();
        ConflictExceptionHandler conflictStrategy = new();

        List<IExceptionStrategy> strategies = [defaultStrategy, notFoundStrategy, conflictStrategy];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();

        // Act - Test NotFoundException
        NotFoundException notFoundException = new("Not found", "TEST_002");
        ProblemDetails notFoundResult = registry.CreateProblemDetails(notFoundException, context);

        // Act - Test ConflictException
        ConflictException conflictException = new("Conflict", "TEST_003");
        ProblemDetails conflictResult = registry.CreateProblemDetails(conflictException, context);

        // Assert
        notFoundResult.Status.Should().Be(StatusCodes.Status404NotFound);
        conflictResult.Status.Should().Be(StatusCodes.Status409Conflict);
    }

    #endregion

    #region CreateProblemDetails - Inheritance Hierarchy Tests

    [Fact]
    public void CreateProblemDetails_WithDerivedExceptionType_ShouldUseBaseClassStrategy()
    {
        // Arrange - Custom exception inheriting from BadRequestException
        CustomBadRequestException customException = new("Custom bad request");

        DefaultExceptionHandler defaultStrategy = new();
        BadRequestExceptionHandler badRequestStrategy = new();
        List<IExceptionStrategy> strategies = [defaultStrategy, badRequestStrategy];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();

        // Act
        ProblemDetails result = registry.CreateProblemDetails(customException, context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void CreateProblemDetails_ShouldCacheDerivedTypeMapping()
    {
        // Arrange
        CustomBadRequestException customException = new("Custom bad request");

        DefaultExceptionHandler defaultStrategy = new();
        BadRequestExceptionHandler badRequestStrategy = new();
        List<IExceptionStrategy> strategies = [defaultStrategy, badRequestStrategy];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();

        // Act - First call should traverse hierarchy
        ProblemDetails firstResult = registry.CreateProblemDetails(customException, context);

        // Act - Second call should use cached mapping
        CustomBadRequestException secondCustomException = new("Another custom bad request");
        ProblemDetails secondResult = registry.CreateProblemDetails(secondCustomException, context);

        // Assert - Both should use the same strategy
        firstResult.Status.Should().Be(StatusCodes.Status400BadRequest);
        secondResult.Status.Should().Be(StatusCodes.Status400BadRequest);
    }

    #endregion

    #region CreateProblemDetails - Multiple Exception Types Tests

    [Fact]
    public void CreateProblemDetails_WithAuthenticationException_ShouldUseCorrectStrategy()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        AuthenticationExceptionHandler authStrategy = new();
        List<IExceptionStrategy> strategies = [defaultStrategy, authStrategy];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();
        AuthenticationException exception = new("Authentication failed", "TEST_004");

        // Act
        ProblemDetails result = registry.CreateProblemDetails(exception, context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void CreateProblemDetails_WithAuthorizationException_ShouldUseCorrectStrategy()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        AuthorizationExceptionHandler authzStrategy = new();
        List<IExceptionStrategy> strategies = [defaultStrategy, authzStrategy];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();
        AuthorizationException exception = new("Authorization failed", "TEST_005");

        // Act
        ProblemDetails result = registry.CreateProblemDetails(exception, context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public void CreateProblemDetails_WithInternalServerException_ShouldUseCorrectStrategy()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        InternalServerExceptionHandler internalServerStrategy = new();
        List<IExceptionStrategy> strategies = [defaultStrategy, internalServerStrategy];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();
        InternalServerException exception = new("Internal server error", "TEST_006");

        // Act
        ProblemDetails result = registry.CreateProblemDetails(exception, context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region CreateProblemDetails - Edge Cases Tests

    [Fact]
    public void CreateProblemDetails_WithNullException_ShouldThrow()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        List<IExceptionStrategy> strategies = [defaultStrategy];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();

        // Act
        Action act = () => registry.CreateProblemDetails(null!, context);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void CreateProblemDetails_CalledMultipleTimes_ShouldReturnConsistentResults()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        BadRequestExceptionHandler badRequestStrategy = new();
        List<IExceptionStrategy> strategies = [defaultStrategy, badRequestStrategy];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();
        BadRequestException exception = new("Bad request", "TEST_001");

        // Act
        ProblemDetails firstResult = registry.CreateProblemDetails(exception, context);
        ProblemDetails secondResult = registry.CreateProblemDetails(exception, context);

        // Assert
        firstResult.Status.Should().Be(secondResult.Status);
        firstResult.Title.Should().Be(secondResult.Title);
    }

    [Fact]
    public void CreateProblemDetails_WithEmptyStrategiesList_ShouldUseDefaultStrategy()
    {
        // Arrange
        DefaultExceptionHandler defaultStrategy = new();
        List<IExceptionStrategy> strategies = [];

        ExceptionStrategyRegistry registry = new(strategies, defaultStrategy);
        DefaultHttpContext context = CreateHttpContext();
        Exception exception = new("Test error");

        // Act
        ProblemDetails result = registry.CreateProblemDetails(exception, context);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region Helper Methods and Classes

    private static DefaultHttpContext CreateHttpContext()
    {
        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";
        context.TraceIdentifier = "test-trace-id";
        return context;
    }

    /// <summary>
    /// Custom exception for testing inheritance hierarchy.
    /// </summary>
    private class CustomBadRequestException : BadRequestException
    {
        public CustomBadRequestException(string message)
            : base(message, "CUSTOM_001") { }
    }

    #endregion
}
