using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Content.Application.Shared.Exceptions.Handlers;

/// <summary>
/// Strategy for a streaming-link provider resolution failure. A rate-limited provider maps to 429
/// (with a Retry-After hint); any other provider failure maps to 502. The localized detail is
/// resolved from <see cref="StreamingLinkErrorMessage" />, so the infrastructure adapter never
/// touches i18n.
/// </summary>
public sealed class StreamingLinkResolutionExceptionHandler : BaseExceptionStrategy<StreamingLinkResolutionException>
{
    private const int RetryAfterSeconds = 60;

    /// <inheritdoc />
    public override ProblemDetails CreateProblemDetails(StreamingLinkResolutionException exception, HttpContext context)
    {
        var msg = context.RequestServices.GetRequiredService<StreamingLinkErrorMessage>();

        if (exception.IsRateLimited)
        {
            context.Response.Headers.RetryAfter = RetryAfterSeconds.ToString();

            return CreateStandardProblemDetails(
                title: nameof(StreamingLinkResolutionException),
                detail: msg.ResolutionRateLimited(),
                statusCode: StatusCodes.Status429TooManyRequests,
                context: context
            );
        }

        return CreateStandardProblemDetails(
            title: nameof(StreamingLinkResolutionException),
            detail: msg.ResolutionFailed(),
            statusCode: StatusCodes.Status502BadGateway,
            context: context
        );
    }
}
