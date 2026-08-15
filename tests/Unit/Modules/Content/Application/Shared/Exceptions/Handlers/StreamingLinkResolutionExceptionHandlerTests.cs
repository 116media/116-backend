using _116.Content.Application.Shared.Errors.Messages;
using _116.Content.Application.Shared.Exceptions;
using _116.Content.Application.Shared.Exceptions.Handlers;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Unit tests for <see cref="StreamingLinkResolutionExceptionHandler"/>. A rate-limited provider maps
/// to 429 with a Retry-After hint; any other provider failure maps to 502. Details come from
/// <see cref="StreamingLinkErrorMessage"/>.
/// </summary>
public class StreamingLinkResolutionExceptionHandlerTests
{
    private const string RetryAfterSeconds = "60";

    private readonly StreamingLinkResolutionExceptionHandler _handler = new();

    private static DefaultHttpContext CreateContext()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddLogging()
            .AddLocalization()
            .AddScoped<StreamingLinkErrorMessage>()
            .BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = provider,
            Request = { Path = "/api/test" },
            TraceIdentifier = "test-trace-id",
        };
    }

    [Fact]
    public void ExceptionType_ShouldReturnStreamingLinkResolutionExceptionType()
    {
        _handler.ExceptionType.Should().Be(typeof(StreamingLinkResolutionException));
    }

    [Fact]
    public void CreateProblemDetails_WhenRateLimited_ShouldReturn429WithRetryAfter()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        var i18n = context.RequestServices.GetRequiredService<StreamingLinkErrorMessage>();
        var exception = new StreamingLinkResolutionException("throttled", isRateLimited: true);

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status429TooManyRequests);
        problemDetails.Detail.Should().Be(i18n.ResolutionRateLimited());
        context.Response.Headers.RetryAfter.ToString().Should().Be(RetryAfterSeconds);
    }

    [Fact]
    public void CreateProblemDetails_WhenNotRateLimited_ShouldReturn502()
    {
        // Arrange
        DefaultHttpContext context = CreateContext();
        var i18n = context.RequestServices.GetRequiredService<StreamingLinkErrorMessage>();
        var exception = new StreamingLinkResolutionException("provider down");

        // Act
        ProblemDetails problemDetails = _handler.CreateProblemDetails(exception, context);

        // Assert
        problemDetails.Status.Should().Be(StatusCodes.Status502BadGateway);
        problemDetails.Detail.Should().Be(i18n.ResolutionFailed());
    }
}
