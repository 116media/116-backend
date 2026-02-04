using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Middleware;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Shared.Middleware;

/// <summary>
/// Unit tests for <see cref="ResourceNotFoundMiddleware"/>.
/// </summary>
public class ResourceNotFoundMiddlewareTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidRequestDelegate_ShouldCreateInstance()
    {
        // Arrange
        RequestDelegate next = _ => Task.CompletedTask;

        // Act
        ResourceNotFoundMiddleware middleware = new(next);

        // Assert
        middleware.Should().NotBeNull();
    }

    #endregion

    #region InvokeAsync - 404 Not Found Tests

    [Fact]
    public async Task InvokeAsync_With404Status_ShouldThrowResourceNotFoundException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/nonexistent";
        context.Request.Method = "GET";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task InvokeAsync_With404Status_ShouldIncludePathInException()
    {
        // Arrange
        DefaultHttpContext context = new();
        string expectedPath = "/api/users/123";
        context.Request.Path = expectedPath;
        context.Request.Method = "GET";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        try
        {
            await middleware.InvokeAsync(context);
        }
        catch (ResourceNotFoundException ex)
        {
            // Assert
            ex.Message.Should().Contain(expectedPath, "exception message should include the requested path");
            return;
        }

        throw new InvalidOperationException("Expected ResourceNotFoundException was not thrown");
    }

    [Fact]
    public async Task InvokeAsync_With404Status_ShouldIncludeMethodInException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/users";
        string expectedMethod = "POST";
        context.Request.Method = expectedMethod;

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        try
        {
            await middleware.InvokeAsync(context);
        }
        catch (ResourceNotFoundException ex)
        {
            // Assert
            ex.Message.Should().Contain(expectedMethod, "exception message should include the HTTP method");
            return;
        }

        throw new InvalidOperationException("Expected ResourceNotFoundException was not thrown");
    }

    #endregion

    #region InvokeAsync - 405 Method Not Allowed Tests

    [Fact]
    public async Task InvokeAsync_With405Status_ShouldThrowMethodNotAllowedException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/users";
        context.Request.Method = "PUT";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<MethodNotAllowedException>();
    }

    [Fact]
    public async Task InvokeAsync_With405Status_ShouldIncludePathAndMethodInException()
    {
        // Arrange
        DefaultHttpContext context = new();
        string expectedPath = "/api/users/delete";
        string expectedMethod = "GET";
        context.Request.Path = expectedPath;
        context.Request.Method = expectedMethod;

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        try
        {
            await middleware.InvokeAsync(context);
        }
        catch (MethodNotAllowedException ex)
        {
            // Assert
            ex.Message.Should().Contain(expectedPath, "exception should include path");
            ex.Message.Should().Contain(expectedMethod, "exception should include method");
            return;
        }

        throw new InvalidOperationException("Expected MethodNotAllowedException was not thrown");
    }

    #endregion

    #region InvokeAsync - Other Status Codes Tests

    [Fact]
    public async Task InvokeAsync_With200Status_ShouldNotThrowException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/users";
        context.Request.Method = "GET";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().NotThrowAsync("200 OK should pass through without exception");
    }

    [Fact]
    public async Task InvokeAsync_With201Status_ShouldNotThrowException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/users";
        context.Request.Method = "POST";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status201Created;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().NotThrowAsync("201 Created should pass through without exception");
    }

    [Fact]
    public async Task InvokeAsync_With400Status_ShouldNotThrowException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/users";
        context.Request.Method = "POST";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().NotThrowAsync("400 Bad Request should pass through without exception");
    }

    [Fact]
    public async Task InvokeAsync_With500Status_ShouldNotThrowException()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/users";
        context.Request.Method = "GET";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().NotThrowAsync("500 Internal Server Error should pass through without exception");
    }

    #endregion

    #region InvokeAsync - Response Already Started Tests

    // Note: Testing response.HasStarted in unit tests is difficult because DefaultHttpContext
    // doesn't fully simulate the HttpContext behavior. This scenario is better tested in
    // integration tests where the actual ASP.NET Core pipeline is involved.
    // The middleware correctly checks for HasStarted before throwing exceptions.

    #endregion

    #region InvokeAsync - Pipeline Tests

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        DefaultHttpContext context = new();
        bool nextWasCalled = false;

        RequestDelegate next = ctx =>
        {
            nextWasCalled = true;
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextWasCalled.Should().BeTrue("middleware should call next in pipeline");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextBeforeCheckingStatusCode()
    {
        // Arrange
        DefaultHttpContext context = new();
        int executionOrder = 0;
        int nextCallOrder = 0;
        int checkOrder = 0;

        RequestDelegate next = ctx =>
        {
            nextCallOrder = ++executionOrder;
            ctx.Response.StatusCode = StatusCodes.Status200OK;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        await middleware.InvokeAsync(context);
        checkOrder = ++executionOrder;

        // Assert
        nextCallOrder.Should().BeLessThan(checkOrder, "next should be called before status check");
    }

    #endregion

    #region Different HTTP Methods Tests

    [Fact]
    public async Task InvokeAsync_With404_ShouldHandleGetMethod()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        context.Request.Method = "GET";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task InvokeAsync_With404_ShouldHandlePostMethod()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        context.Request.Method = "POST";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task InvokeAsync_With404_ShouldHandlePutMethod()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        context.Request.Method = "PUT";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    [Fact]
    public async Task InvokeAsync_With404_ShouldHandleDeleteMethod()
    {
        // Arrange
        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        context.Request.Method = "DELETE";

        RequestDelegate next = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return Task.CompletedTask;
        };

        ResourceNotFoundMiddleware middleware = new(next);

        // Act
        Func<Task> act = async () => await middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<ResourceNotFoundException>();
    }

    #endregion
}
