using _116.Identity.Application.Shared.Exceptions;
using _116.Identity.Application.Shared.Exceptions.Handlers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Unit tests for <see cref="AccessTokenExpiryExceptionHandler"/>.
/// </summary>
public class AccessTokenExpiryExceptionHandlerTests
{
    private readonly AccessTokenExpiryExceptionHandler _handler = new();

    [Fact]
    public void ExceptionType_ShouldReturnAccessTokenExpiryExceptionType()
    {
        Type exceptionType = _handler.ExceptionType;
        exceptionType.Should().Be(typeof(AccessTokenExpiryException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithCorrectTitle()
    {
        AccessTokenExpiryException exception = new("Access token expired");
        DefaultHttpContext context = CreateHttpContext();

        var problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Title.Should().Be(nameof(AccessTokenExpiryException));
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturnProblemDetailsWithExceptionMessage()
    {
        string errorMessage = "Your access token has expired";
        AccessTokenExpiryException exception = new(errorMessage);
        DefaultHttpContext context = CreateHttpContext();

        var problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Detail.Should().Be(errorMessage);
    }

    [Fact]
    public void CreateProblemDetails_ShouldReturn401StatusCode()
    {
        AccessTokenExpiryException exception = new("Token expired");
        DefaultHttpContext context = CreateHttpContext();

        var problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Status.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeRequestPath()
    {
        AccessTokenExpiryException exception = new("Token expired");
        DefaultHttpContext context = CreateHttpContext();
        string requestPath = "/api/users";
        context.Request.Path = requestPath;

        var problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Instance.Should().Be(requestPath);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeTraceIdExtension()
    {
        AccessTokenExpiryException exception = new("Token expired");
        DefaultHttpContext context = CreateHttpContext();
        string traceId = "test-trace-123";
        context.TraceIdentifier = traceId;

        var problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"].Should().Be(traceId);
    }

    [Fact]
    public void CreateProblemDetails_ShouldIncludeTimestampExtension()
    {
        AccessTokenExpiryException exception = new("Token expired");
        DefaultHttpContext context = CreateHttpContext();

        var problemDetails = _handler.CreateProblemDetails(exception, context);

        problemDetails.Extensions.Should().ContainKey("timestamp");
        var timestamp = (DateTime)problemDetails.Extensions["timestamp"]!;
        timestamp.Should().NotBe(default(DateTime));
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        DefaultHttpContext context = new();
        context.Request.Path = "/api/test";
        context.TraceIdentifier = "test-trace-id";
        return context;
    }
}
